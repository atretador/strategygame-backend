using MongoDB.Bson;
using StrategyGame.Models;
using StrategyGame.Result;

namespace StrategyGame.Services
{
    public interface IResourceManagerService
    {
        Task<ResourceValidationResult> ValidateResources(City city, Dictionary<ObjectId, int> resources);
        Task<ResourceValidationResult> DeductResourcesWithLockAsync(ObjectId cityId, Dictionary<ObjectId, int> resources);
        Task AddResourcesWithLockAsync(ObjectId cityId, Dictionary<ObjectId, int> resources);
        Task<Resource> AddResourceAsync(Resource resource);
        Task<bool> RemoveResourceAsync(ObjectId id);
        Task<Resource> UpdateResourceAsync(ObjectId id, Resource resource);
        Task<Resource> GetResourceByIdAsync(ObjectId id);
        Task<List<Resource>> GetAllResourcesAsync();
    }
}