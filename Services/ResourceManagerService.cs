using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Models;
using StrategyGame.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Services
{
    public class ResourceManagerService : IResourceManagerService
    {
        private readonly IMongoCollection<Resource> _resources;
        private readonly MongoDbContext _dbcontext;

        public ResourceManagerService(MongoDbContext dbContext)
        {
            _dbcontext = dbContext;
            _resources = dbContext.Resources;
        }

        // Add a Resource
        public async Task<Resource> AddResourceAsync(Resource resource)
        {
            await _resources.InsertOneAsync(resource);
            return resource;
        }

        // Remove a Resource
        public async Task<bool> RemoveResourceAsync(ObjectId id)
        {
            var result = await _resources.DeleteOneAsync(r => r.Id == id);
            return result.DeletedCount > 0;
            return true;
        }

        // Update a Resource
        public async Task<Resource> UpdateResourceAsync(ObjectId id, Resource resource)
        {
            var existingResource = await _resources.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (existingResource == null) return null;

            existingResource.Name = resource.Name;
            await _resources.ReplaceOneAsync(r => r.Id == id, existingResource);
            return existingResource;
        }

        // Get a Resource by Id
        public async Task<Resource> GetResourceByIdAsync(ObjectId id)
        {
            return await _resources.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        // Get All Resources
        public async Task<List<Resource>> GetAllResourcesAsync()
        {
            return await _resources.Find(_ => true).ToListAsync();
        }

        public async Task<ResourceValidationResult> DeductResourcesWithLockAsync(ObjectId cityId, Dictionary<ObjectId, int> resources)
        {
            var lockToken = Guid.NewGuid().ToString();
            var lockTimeout = TimeSpan.FromSeconds(10);
            var maxRetryAttempts = 5;

            for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
            {
                if (await TryAcquireLockAsync(cityId, lockToken, lockTimeout))
                {
                    try
                    {
                        var city = await GetCityByIdAsync(cityId);
                        if (city == null) throw new Exception($"City with ID {cityId} not found.");

                        var validationResult = await ValidateResources(city, resources);
                        if (!validationResult.HasEnough) return validationResult;

                        foreach (var resource in resources)
                        {
                            city.CityContents.ResourceStorage[resource.Key] -= resource.Value;
                        }

                        await UpdateCityInDatabaseAsync(city);
                        return validationResult;
                    }
                    finally
                    {
                        await ReleaseLockAsync(cityId, lockToken);
                    }
                }

                await Task.Delay(100);
            }

            throw new TimeoutException("Failed to acquire lock after multiple attempts.");
        }

        public async Task AddResourcesWithLockAsync(ObjectId cityId, Dictionary<ObjectId, int> resources)
        {
            var lockToken = Guid.NewGuid().ToString();
            var lockTimeout = TimeSpan.FromSeconds(10);
            var maxRetryAttempts = 5;

            for (int attempt = 0; attempt < maxRetryAttempts; attempt++)
            {
                if (await TryAcquireLockAsync(cityId, lockToken, lockTimeout))
                {
                    try
                    {
                        var city = await GetCityByIdAsync(cityId);
                        if (city == null) throw new Exception($"City with ID {cityId} not found.");

                        foreach (var resource in resources)
                        {
                            if (city.CityContents.ResourceStorage.ContainsKey(resource.Key))
                            {
                                city.CityContents.ResourceStorage[resource.Key] += resource.Value;
                            }
                            else
                            {
                                city.CityContents.ResourceStorage.Add(resource.Key, resource.Value);
                            }
                        }

                        await UpdateCityInDatabaseAsync(city);
                        return;
                    }
                    finally
                    {
                        await ReleaseLockAsync(cityId, lockToken);
                    }
                }

                await Task.Delay(100);
            }

            throw new TimeoutException("Failed to acquire lock after multiple attempts.");
        }

        public async Task<ResourceValidationResult> ValidateResources(City city, Dictionary<ObjectId, int> resources)
        {
            var result = new ResourceValidationResult { HasEnough = true };

            foreach (var requiredResource in resources)
            {
                if (!city.CityContents.ResourceStorage.TryGetValue(requiredResource.Key, out int currentAmount) || currentAmount < requiredResource.Value)
                {
                    result.HasEnough = false;
                    result.MissingResources[requiredResource.Key] = requiredResource.Value - currentAmount;
                }
            }

            return result;
        }

        private async Task<City> GetCityByIdAsync(ObjectId cityId)
        {
            return await _dbcontext.Cities.Find(Builders<City>.Filter.Eq(c => c.Id, cityId)).FirstOrDefaultAsync();
        }

        private async Task<bool> TryAcquireLockAsync(ObjectId cityId, string lockToken, TimeSpan lockTimeout)
        {
            var filter = Builders<City>.Filter.And(
                Builders<City>.Filter.Eq(c => c.Id, cityId),
                Builders<City>.Filter.Or(
                    Builders<City>.Filter.Eq(c => c.LockToken, null),
                    Builders<City>.Filter.Lt(c => c.LockExpiration, DateTime.UtcNow)
                )
            );

            var update = Builders<City>.Update
                .Set(c => c.LockToken, lockToken)
                .Set(c => c.LockExpiration, DateTime.UtcNow.Add(lockTimeout));

            var result = await _dbcontext.Cities.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        private async Task ReleaseLockAsync(ObjectId cityId, string lockToken)
        {
            var filter = Builders<City>.Filter.And(
                Builders<City>.Filter.Eq(c => c.Id, cityId),
                Builders<City>.Filter.Eq(c => c.LockToken, lockToken)
            );

            var update = Builders<City>.Update
                .Set(c => c.LockToken, null)
                .Set(c => c.LockExpiration, null);

            await _dbcontext.Cities.UpdateOneAsync(filter, update);
        }

        private async Task UpdateCityInDatabaseAsync(City city)
        {
            var filter = Builders<City>.Filter.Eq(c => c.Id, city.Id);
            var update = Builders<City>.Update.Set(c => c.CityContents, city.CityContents);
            await _dbcontext.Cities.UpdateOneAsync(filter, update);
        }
    }
}