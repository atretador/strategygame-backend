using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Models;
using StrategyGame.Services;

namespace StrategyGame.BackgroundServices
{
    public class CityContentsBackgroundService : BackgroundService
    {
        private readonly MongoDbContext _dbContext;
        private readonly ILogger<CityContentsBackgroundService> _logger;
        private readonly IUnitManagerService _unitManagerService;
        private readonly IMilitaryUnitManagerService _militaryService;
        private readonly int _batchSize = 100; // Number of cities per batch
        private readonly TimeSpan _tickInterval = TimeSpan.FromMilliseconds(250); // Check every 1/4 of a second

        public CityContentsBackgroundService(
            MongoDbContext dbContext,
            ILogger<CityContentsBackgroundService> logger,
            IUnitManagerService unitManagerService,
            IMilitaryUnitManagerService militaryService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _unitManagerService = unitManagerService;
            _militaryService = militaryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("City Contents Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var stopWatch = Stopwatch.StartNew();
                try
                {
                    var processedEntries = await ProcessCityContentsAsync(stoppingToken);
                    stopWatch.Stop();
                    if(processedEntries > 0)
                    {
                        _logger.LogInformation("{ProcessedEntries} cities processed in {ElapsedMilliseconds} ms.", processedEntries, stopWatch.ElapsedMilliseconds);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing city contents.");
                }

                await Task.Delay(_tickInterval, stoppingToken);
            }

            _logger.LogInformation("City Contents Service stopped.");
        }

        private async Task<int> ProcessCityContentsAsync(CancellationToken stoppingToken)
        {
            var collection = _dbContext.Cities;

            // Fetch all distinct sectors to parallelize by sector
            var sectors = await collection.Distinct(c => c.SectorId, FilterDefinition<City>.Empty).ToListAsync(stoppingToken);
            var processedCount = 0;

            var tasks = new List<Task>();

            // Process cities in parallel, one for each sector
            foreach (var sectorId in sectors)
            {
                tasks.Add(Task.Run(async () =>
                {
                    if (stoppingToken.IsCancellationRequested)
                        return;

                    // Process cities in this sector
                    var citiesToProcess = await collection.Find(Builders<City>.Filter.Eq(c => c.SectorId, sectorId))
                                                          .Limit(_batchSize) // Process in batches
                                                          .ToListAsync(stoppingToken);

                    var sectorProcessedCount = 0;

                    foreach (var city in citiesToProcess)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        // Process unit training, building, and research queues
                        bool processed = await ProcessCityQueuesAsync(city, stoppingToken);

                        if (processed)
                        {
                            // Save changes back to the database
                            await collection.ReplaceOneAsync(
                                Builders<City>.Filter.Eq(c => c.Id, city.Id),
                                city,
                                new ReplaceOptions { IsUpsert = false },
                                stoppingToken);

                            sectorProcessedCount++;
                        }

                        // Delay to avoid overwhelming the server
                        await Task.Delay(10, stoppingToken);
                    }

                    // Log progress per sector
                    _logger.LogInformation("Processed {SectorProcessedCount} cities in sector {SectorId}.", sectorProcessedCount, sectorId);
                    processedCount += sectorProcessedCount;
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            return processedCount;
        }

        private async Task<bool> ProcessCityQueuesAsync(City city, CancellationToken stoppingToken)
        {
            bool changesMade = false;

            // Process unit training queue
            changesMade |= await ProcessUnitTrainingQueueAsync(city, stoppingToken);

            // Process construction queue
            changesMade |= await ProcessConstructionQueueAsync(city, stoppingToken);

            // Process research queue
            changesMade |= await ProcessResearchQueueAsync(city, stoppingToken);

            return changesMade;
        }

        private async Task<bool> ProcessUnitTrainingQueueAsync(City city, CancellationToken stoppingToken)
        {
            bool changesMade = false;

            var trainingQueueCopy = city.CityContents.UnitTrainingQueu.ToList(); // Safely iterate over a snapshot

            foreach (var training in trainingQueueCopy)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                if (training.Quantity == 0)
                {
                    _logger.LogWarning("Training of 0 units found in city {CityName}. Removing from queue.", city.Name);
                    city.CityContents.UnitTrainingQueu.Remove(training);
                }
                else if (training.ReadyAt <= DateTime.UtcNow)
                {
                    var unit = await _militaryService.GetMilitaryUnitByIdAsync(training.Id);
                    if (unit == null)
                    {
                        _logger.LogWarning("Unit with ID {UnitId} not found for training in city {CityName}.", training.Id, city.Name);
                        continue;
                    }

                    await _unitManagerService.AddUnitsToCityArmy(city, unit, training.Quantity ?? 0);
                    city.CityContents.UnitTrainingQueu.Remove(training);
                    changesMade = true;
                    _logger.LogInformation("Completed training of {Quantity} {UnitName} units for city {CityName}.", training.Quantity, unit.Name, city.Name);
                }
            }

            return changesMade;
        }

        private async Task<bool> ProcessConstructionQueueAsync(City city, CancellationToken stoppingToken)
        {
            bool changesMade = false;

            var buildingQueueCopy = city.CityContents.ConstructionQueu.ToList();

            foreach (var construction in buildingQueueCopy)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                if (construction.ReadyAt <= DateTime.UtcNow)
                {
                    var buildingLevel = city.CityContents.GetBuildingLevel(construction.Id);
                    if (buildingLevel == 0)
                    {
                        _logger.LogWarning("Building with ID {BuildingId} not found for construction in city {CityName}.", construction.Id, city.Name);
                        continue;
                    }

                    // Proceed with construction logic (example: upgrade or complete building)
                    // Add any other related logic here, such as resources consumption, etc.

                    city.CityContents.ConstructionQueu.Remove(construction);
                    changesMade = true;
                    _logger.LogInformation("Completed construction of building {BuildingId} in city {CityName}.", construction.Id, city.Name);
                }
            }

            return changesMade;
        }

        private async Task<bool> ProcessResearchQueueAsync(City city, CancellationToken stoppingToken)
        {
            bool changesMade = false;

            var researchQueueCopy = city.CityContents.ResearchQueu.ToList();

            foreach (var research in researchQueueCopy)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                if (research.ReadyAt <= DateTime.UtcNow)
                {
                    var researchLevel = city.CityContents.GetResearchLevel(research.Id);
                    if (researchLevel == 0)
                    {
                        _logger.LogWarning("Research with ID {ResearchId} not found for city {CityName}.", research.Id, city.Name);
                        continue;
                    }

                    // Proceed with research logic (e.g., complete research, apply bonuses)
                    // Add any other related logic here.

                    city.CityContents.ResearchQueu.Remove(research);
                    changesMade = true;
                    _logger.LogInformation("Completed research {ResearchId} in city {CityName}.", research.Id, city.Name);
                }
            }

            return changesMade;
        }
    }
}