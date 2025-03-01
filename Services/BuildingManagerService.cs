using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Enums;
using StrategyGame.Models;

namespace StrategyGame.Services
{
    public class BuildingManagerService : IBuildingManagerService
    {
        private IMongoCollection<Building> _buildings;
        private readonly IMemoryCache _cache;
        private const string BuildingsCacheKeyPrefix = "Buildings";
        private const string BuildingsCacheKeysTrackerKey = "BuildingsCacheKeys";
        private readonly object _buildingsCacheKeysLock = new();

        public BuildingManagerService(MongoDbContext dbContext, IMemoryCache cache)
        {
            _buildings = dbContext.Buildings;
            _cache = cache;
        }
        // Save a Building to cache and track the key
        private void CacheBuilding(Building building)
        {
            var cacheKey = GetBuildingCacheKey(building.Id);
            _cache.Set(cacheKey, building);
        }
        
        // Generate a unique cache key for a Building
        private string GetBuildingCacheKey(ObjectId? buildingId) => $"{BuildingsCacheKeyPrefix}:Id:{buildingId}";
        private string GetBuildingTypeCacheKey(BuildingType type) => $"{BuildingsCacheKeyPrefix}:Type:{type}";
        
        // Retrieve the list of tracked cache keys for Buildings
        private HashSet<string> GetTrackedBuildingCacheKeys()
        {
            return _cache.Get<HashSet<string>>(BuildingsCacheKeysTrackerKey) ?? new HashSet<string>();
        }

        private Building? GetBuildingFromCache(ObjectId buildingId)
        {
            var cacheKey = GetBuildingCacheKey(buildingId);
            return _cache.TryGetValue(cacheKey, out Building building) ? building : null;
        }

        private void CacheBuildingIdsByType(BuildingType type, List<ObjectId> buildingIds)
        {
            var cacheKey = GetBuildingTypeCacheKey(type);
            _cache.Set(cacheKey, buildingIds);
        }

        private List<ObjectId>? GetBuildingIdsByTypeFromCache(BuildingType type)
        {
            var cacheKey = GetBuildingTypeCacheKey(type);
            return _cache.TryGetValue(cacheKey, out List<ObjectId> buildingIds) ? buildingIds : null;
        }

        private void ClearBuildingCache(ObjectId buildingId, BuildingType? type = null)
        {
            _cache.Remove(GetBuildingCacheKey(buildingId));

            if (type.HasValue)
            {
                _cache.Remove(GetBuildingTypeCacheKey(type.Value));
            }
        }

        // Clear cache for all Buildings
        public void ClearAllBuildingsCache()
        {
            lock (_buildingsCacheKeysLock)
            {
                var cacheKeys = GetTrackedBuildingCacheKeys();
                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }

                // Clear the tracker
                _cache.Remove(BuildingsCacheKeysTrackerKey);
            }
        }

        // Add a Building
        public async Task<Building> AddBuildingAsync(Building building)
        {
            await _buildings.InsertOneAsync(building);

            // Update type cache
            var buildingIds = GetBuildingIdsByTypeFromCache(building.BuildingType) ?? new List<ObjectId>();

            buildingIds.Add(building.Id);
            CacheBuildingIdsByType(building.BuildingType, buildingIds);

            // Cache the building
            CacheBuilding(building);

            return building;
        }


        // Get a Building by Id
        public async Task<Building> GetBuildingByIdAsync(ObjectId id)
        {
            var building = GetBuildingFromCache(id);
            if (building != null) return building;

            building = _buildings.AsQueryable().FirstOrDefault(b => b.Id == id);

            if (building != null) CacheBuilding(building);
            return building!;
        }

        // get building by type
        public async Task<List<Building>> GetBuildingsByTypeAsync(BuildingType type)
        {
            // Check cache for building IDs
            var buildingIds = GetBuildingIdsByTypeFromCache(type);
            var buildings = new List<Building>();

            if (buildingIds != null)
            {
                // Retrieve buildings from cache
                foreach (var id in buildingIds)
                {
                    var building = GetBuildingFromCache(id);
                    if (building != null)
                    {
                        buildings.Add(building);
                    }
                }

                // If all buildings were found in cache, return them
                if (buildings.Count == buildingIds.Count)
                {
                    return buildings;
                }
            }

            // Fallback: Query database if cache misses occur
            buildings = await _buildings.Find(b => b.BuildingType == type).ToListAsync();

            // Cache the results
            if (buildings.Any())
            {
                var idsToCache = buildings.Select(b => b.Id).ToList();
                CacheBuildingIdsByType(type, idsToCache);

                // Cache each building
                foreach (var building in buildings)
                {
                    CacheBuilding(building);
                }
            }

            return buildings;
        }



        // Update a Building
        public async Task<Building> UpdateBuildingAsync(ObjectId id, Building building)
        {
            var existingBuilding = await _buildings.Find(b => b.Id == id).FirstOrDefaultAsync();
            if (existingBuilding == null) return null;

            // Handle type change
            if (existingBuilding.BuildingType != building.BuildingType)
            {
                // Remove from old type cache
                var oldTypeIds = GetBuildingIdsByTypeFromCache(existingBuilding.BuildingType) ?? new List<ObjectId>();

                oldTypeIds.Remove(existingBuilding.Id);
                CacheBuildingIdsByType(existingBuilding.BuildingType, oldTypeIds);

                // Add to new type cache
                var newTypeIds = GetBuildingIdsByTypeFromCache(building.BuildingType) ?? new List<ObjectId>();
                newTypeIds.Add(building.Id);

                CacheBuildingIdsByType(building.BuildingType, newTypeIds);
            }

            // Update the building
            existingBuilding.Name = building.Name;
            existingBuilding.BuildingType = building.BuildingType;
            existingBuilding.BuildingBlueprint = building.BuildingBlueprint;

            await _buildings.ReplaceOneAsync(b => b.Id == id, existingBuilding);

            // Refresh cache
            CacheBuilding(existingBuilding);

            return existingBuilding;
        }


        // Delete a Building
        public async Task<bool> DeleteBuildingAsync(ObjectId id)
        {
            var building = _buildings.AsQueryable().FirstOrDefault(b => b.Id == id);
            if (building == null) return false;

            await _buildings.DeleteOneAsync(b => b.Id == id);

            // Remove from type cache
            var typeIds = GetBuildingIdsByTypeFromCache(building.BuildingType) ?? new List<ObjectId>();
            typeIds.Remove(building.Id);
            CacheBuildingIdsByType(building.BuildingType, typeIds);

            // Clear building cache
            ClearBuildingCache(id);

            return true;
        }

        // Get All Buildings
        public async Task<List<Building>> GetAllBuildingsAsync()
        {
            return await _buildings.Find(Builders<Building>.Filter.Empty).ToListAsync();
        }

    }
}