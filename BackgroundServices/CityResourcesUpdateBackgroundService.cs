using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Models;
using StrategyGame.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StrategyGame.Enums;

namespace StrategyGame.BackgroundServices
{
    public class CityResourcesUpdateBackgroundService : BackgroundService
    {
        private readonly ILogger<CityResourcesUpdateBackgroundService> _logger;
        private readonly IResourceManagerService _resourceManager;
        private readonly MongoDbContext _dbContext;
        private readonly IWorldManagerService _worldService;
        private readonly IBuildingManagerService _buildingService;

        public CityResourcesUpdateBackgroundService(
            ILogger<CityResourcesUpdateBackgroundService> logger,
            IResourceManagerService resourceManager,
            MongoDbContext dbContext,
            IWorldManagerService worldService,
            IBuildingManagerService buildingService)
        {
            _logger = logger;
            _resourceManager = resourceManager;
            _dbContext = dbContext;
            _worldService = worldService;
            _buildingService = buildingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("City Resources Update Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateAllCitiesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the resource update cycle.");
                }

                await Task.Delay(1000, stoppingToken); // You can adjust this delay based on world tick rate
            }

            _logger.LogInformation("City Resources Update Background Service is stopping.");
        }

        private async Task UpdateAllCitiesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting resource update for all cities at {Time}", DateTime.UtcNow);

            // Retrieve all worlds with their tick rates
            var worlds = await _dbContext.Worlds.Find(_ => true).ToListAsync(stoppingToken);

            var tasks = worlds.Select(async world =>
            {
                try
                {
                    // Fetch cities for this world
                    var cities = await _dbContext.Cities.Find(c => c.WorldId == world.Id).ToListAsync(stoppingToken);
                    var worldSettings = await _worldService.GetWorldSettingsAsync(world.Id);
                    var cityTasks = cities.Select(async city =>
                    {
                        try
                        {
                            // Calculate resources to add based on city buildings
                            var resourcesToAdd = await CalculateResourcesForCity(city, worldSettings.ResourceBaseProductionRate, worldSettings.ResourceProductionGrowthFactor);

                            // Add resources to the city
                            await _resourceManager.AddResourcesWithLockAsync(city.Id, resourcesToAdd);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred during the resource update for city {CityId}.", city.Id);
                        }
                    });

                    // Wait for all cities in this world to update
                    await Task.WhenAll(cityTasks);

                    // Adjust the delay based on the world tick rate (if applicable)
                    await Task.Delay(world.Settings.TickRateInMilliseconds, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the resource update for world {WorldId}.", world.Id);
                }
            });

            // Wait for all worlds to update
            await Task.WhenAll(tasks);

            _logger.LogInformation("Resource update for all cities completed at {Time}", DateTime.UtcNow);
        }

        private async Task<Dictionary<ObjectId, int>> CalculateResourcesForCity(City city, int resourceBaseProductionRate, float resourceProductionGrowthFactor)
        {
            // Example calculation based on city buildings
            var resourcesToAdd = new Dictionary<ObjectId, int>();
            // a list of all resource buildings
            List<ResourceBuilding> resourceBuildings = (await _buildingService.GetBuildingsByTypeAsync(BuildingType.Resource)).Cast<ResourceBuilding>().ToList();

            // Iterate through each resource building and check if it exists in the city's buildings
            foreach (var resourceBuilding in resourceBuildings)
            {
                var buildingId = resourceBuilding.Id;

                // Check if this resource building exists in the city and get its level
                if (city.CityContents.Buildings.ContainsKey(buildingId))
                {
                    var buildingLevel = city.CityContents.Buildings[buildingId]; // Building level in the city

                    // Calculate the resource production based on the building level using the exponential growth formula
                    double production = resourceBaseProductionRate * Math.Pow(resourceProductionGrowthFactor, buildingLevel - 1);

                    // Add the calculated production to the resourcesToAdd dictionary
                    if (resourcesToAdd.ContainsKey(resourceBuilding.ResourceId))
                    {
                        resourcesToAdd[resourceBuilding.ResourceId] += (int)production; // Add to existing value if building already in dictionary
                    }
                    else
                    {
                        resourcesToAdd[resourceBuilding.ResourceId] = (int)production; // Create new entry if building is not yet in dictionary
                    }
                }
            }

            return resourcesToAdd;
        }
    }
}
