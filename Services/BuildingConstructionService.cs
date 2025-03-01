using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Enums;
using StrategyGame.Models;

namespace StrategyGame.Services
{
    public class BuildingConstructionService : IBuildingConstructionService
    {
        private readonly MongoDbContext _dbContext;
        private readonly ILogger<BuildingConstructionService> _logger;
        private readonly IResourceManagerService _resourceManager;
        private readonly IBuildingManagerService _buildingService;

        public BuildingConstructionService(MongoDbContext dbContext,
                                           ILogger<BuildingConstructionService> logger,
                                           IResourceManagerService resourceManager,
                                           IBuildingManagerService buildingService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _resourceManager = resourceManager;
            _buildingService = buildingService;
        }

        public async Task StartBuildingConstructionAsync(City city, ObjectId buildingId)
        {
            // Fetch the building by ObjectId
            var building = await _buildingService.GetBuildingByIdAsync(buildingId);

            var blueprint = building.BuildingBlueprint;
            
            // Find the current building in the city based on the blueprint
            var cityBuilding = city.CityContents.Buildings
                .FirstOrDefault(b => b.Key == building.Id);

            // Validate if the city has enough resources
            var validationResult = await _resourceManager.ValidateResources(city, blueprint.Price);
            if (!validationResult.HasEnough)
            {
                throw new InvalidOperationException("Not enough resources to start building construction.");
            }

            // Deduct resources
            await _resourceManager.DeductResourcesWithLockAsync(city.Id, blueprint.Price);

            // Calculate construction time based on the level (use the current level)
            var constructionTime = blueprint.BaseConstructionTime * cityBuilding.Value * blueprint.ContructionTimeMultiplierPerLevel;

            // Create a new construction entry
            var construction = new OnQueueProcess
            {
                Type = QueueType.Building,
                Id = building.Id,
                StartedAt = DateTime.Now,
                ReadyAt = DateTime.Now.AddSeconds(constructionTime)
            };

            // Add the construction to the city's ongoing constructions queue
            city.CityContents.ConstructionQueu.Add(construction);

            // Update the city document in the database
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbContext.Cities.UpdateOneAsync(filter, update);

            _logger.LogInformation($"Started construction of {building.Name} in city {city.Id}.");
        }

        public async Task CancelBuildingConstructionAsync(City city, OnQueueProcess construction)
        {
            // Remove the construction from the list of ongoing constructions
            city.CityContents.ConstructionQueu.Remove(construction);

            //refund
            var building = await _buildingService.GetBuildingByIdAsync(construction.Id);

            await _resourceManager.AddResourcesWithLockAsync(city.Id, building.BuildingBlueprint.Price);

            // Update the city document in the database
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbContext.Cities.UpdateOneAsync(filter, update);

            _logger.LogInformation($"Canceled construction of {building.Name} in city {city.Name}.");
        }

        public async Task DestroyBuildingAsync(City city, ObjectId building)
        {
            // Remove the building from the city's building list
            city.CityContents.Buildings.Remove(building);

            // Optionally, refund resources used for the building
            // Add logic here to return resources based on the building's cost and level

            // Update the city document in the database
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbContext.Cities.UpdateOneAsync(filter, update);

            _logger.LogInformation($"Destroyed building with ObjectId {building} in city {city.Name}.");
        }
    }
}