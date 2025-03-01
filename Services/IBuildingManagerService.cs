using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Enums;
using StrategyGame.Models;

namespace StrategyGame.Services
{
    public interface IBuildingManagerService
    {
        // Buildings
        Task<Building> AddBuildingAsync(Building building);
        Task<Building> GetBuildingByIdAsync(ObjectId id);
        Task<List<Building>> GetBuildingsByTypeAsync(BuildingType type);
        Task<Building> UpdateBuildingAsync(ObjectId id, Building building);
        Task<bool> DeleteBuildingAsync(ObjectId id);
        Task<List<Building>> GetAllBuildingsAsync();
    }
}