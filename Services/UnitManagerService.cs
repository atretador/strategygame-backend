using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using StrategyGame.Context;
using StrategyGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using StrategyGame.Enums;

namespace StrategyGame.Services
{
    public class UnitManagerService : IUnitManagerService
    {
        private readonly MongoDbContext _dbContext;
        private readonly ILogger<UnitManagerService> _logger;
        private readonly IResourceManagerService _resourceManager;
        private readonly IMilitaryUnitManagerService _militaryService;
        private readonly WorldSettings _worldSettings;

        public UnitManagerService(MongoDbContext dbContext,
                                   ILogger<UnitManagerService> logger,
                                   IResourceManagerService resourceManagerService,
                                   IOptions<WorldSettings> worldSettings,
                                   IMilitaryUnitManagerService militaryService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _resourceManager = resourceManagerService;
            _worldSettings = worldSettings.Value;
            _militaryService = militaryService;
        }

        public async Task StartUnitTrainingAsync(City city, MilitaryUnit unit, int amount)
        {
            var totalPrice = unit.Price
                .Select(resource => new Dictionary<ObjectId, int> {
                    { resource.Key, (int)(resource.Value * amount) }}).ToDictionary(x => x.Keys.First(), x => x.Values.First());

            // Validate if the city has enough resources
            var validationResult = await _resourceManager.ValidateResources(city, totalPrice);
            if (!validationResult.HasEnough)
            {
                throw new InvalidOperationException("Not enough resources to start unit training.");
            }

            // Deduct resources
            await _resourceManager.DeductResourcesWithLockAsync(city.Id, totalPrice);

            // Calculate training time based on the unit blueprint (you can adjust this based on unit or city level)
            var trainingTime = unit.Price.Count * 10; // Simple logic for demo, replace with your own logic

            // Create a new unit training entry
            var unitTraining = new OnQueueProcess
            {
                Type = QueueType.Training,
                Id = unit.Id,
                Quantity = amount,
                StartedAt = DateTime.Now,
                ReadyAt = DateTime.Now.AddSeconds(trainingTime * _worldSettings.DefaultUnitTrainingSpeed)
            };

            // Add the training to the city's training queue
            city.CityContents.UnitTrainingQueu.Add(unitTraining);

            // Update the city document in the database
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbContext.Cities.UpdateOneAsync(filter, update);

            _logger.LogInformation($"Started training of {amount} {unit.Name} units in city {city.Name}.");
        }

        public async Task CancelUnitTrainingAsync(City city, OnQueueProcess unitTraining)
        {
            // Refund resources that were spent on the unit training
            var unit = await _militaryService.GetMilitaryUnitByIdAsync(unitTraining.Id);

            var totalPrice = unit.Price
                .Select(resource => new Dictionary<ObjectId, int> {
                    { resource.Key, (int)(resource.Value * unitTraining.Quantity) }})
                    .ToList();

            foreach (var res in totalPrice)
            {
                var resourceStorage = city.CityContents.ResourceStorage
                    .FirstOrDefault(r => r.Key == res.Keys.First());

                
            }

            // Remove the training from the city's training queue
            city.CityContents.UnitTrainingQueu.Remove(unitTraining);

            // Update the city document in the database
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbContext.Cities.UpdateOneAsync(filter, update);

            _logger.LogInformation($"Canceled training of {unitTraining.Quantity} {unitTraining.Id} units in city {city.Name}.");
        }

        public async Task AddUnitsToCityArmy(City city, MilitaryUnit unit, int amount)
        {
            var cityArmy = city.CityContents.Armies
                .FirstOrDefault(a => a.MilitaryUnit.Id == unit.Id);

            if (cityArmy == null)
            {
                // If the unit doesn't exist in the city's army, add it
                city.CityContents.Armies.Add(new CityArmy(unit, amount));
                _logger.LogInformation($"Added {amount} {unit.Name} to the army in city {city.Name}.");
            }
            else
            {
                // If the unit exists, increase the number of units
                cityArmy.Units += amount;
                _logger.LogInformation($"Added {amount} {unit.Name} to the existing army in city {city.Name}.");
            }

            // Update the city document in the database
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbContext.Cities.UpdateOneAsync(filter, update);
        }

        public async Task RemoveUnitsFromCityArmy(City city, MilitaryUnit unit, int amount)
        {
            var cityArmy = city.CityContents.Armies
                .FirstOrDefault(a => a.MilitaryUnit.Id == unit.Id);

            if (cityArmy == null)
            {
                // If the unit doesn't exist in the city's army, log a message
                _logger.LogWarning($"No {unit.Name} units found in the army of city {city.Name}.");
                return;
            }

            // Remove or reduce the number of units
            cityArmy.Units -= amount;

            // If no units of this type are left, remove it from the army
            if (cityArmy.Units <= 0)
            {
                city.CityContents.Armies.Remove(cityArmy);
                _logger.LogInformation($"Removed all {unit.Name} units from the army in city {city.Name}.");
            }
            else
            {
                _logger.LogInformation($"Removed {amount} {unit.Name} from the army in city {city.Name}.");
            }

            // Update the city document in the database
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbContext.Cities.UpdateOneAsync(filter, update);
        }
    }
}