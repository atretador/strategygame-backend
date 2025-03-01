using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Models;
using StrategyGame.Result;

namespace StrategyGame.Services
{
    public interface IWorldManagerService
    {
        Task<TaskResult<World>> GenerateWorldAsync(string name, string WorldSettingsId);
        Task<TaskResult<World>> GetWorldAsync(ObjectId worldId);
        Task<TaskResult<List<World>>> GetWorldsAsync();
        Task<TaskResult<List<Sector>>> GetSectorsAsync(ObjectId worldId);
        Task<TaskResult<Sector>> GetSectorAsync(ObjectId sectorId);
        
        // WorldSettings
        Task<WorldSettings> GetWorldSettingsAsync(ObjectId settingsId);
        Task<List<WorldSettings>> GetAllWorldSettingsAsync();
        Task<WorldSettings> AddWorldSettingsAsync(WorldSettings settings);
        Task<WorldSettings> UpdateWorldSettingsAsync(ObjectId settingsId, WorldSettings settings);
        Task<bool> RemoveWorldSettingsAsync(ObjectId settingsId);
    }
}