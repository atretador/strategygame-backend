using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Models;

namespace StrategyGame.Services
{
    public interface IMilitaryUnitManagerService
    {
        Task<MilitaryUnit> AddMilitaryUnitAsync(MilitaryUnit unit);
        Task<MilitaryUnit> GetMilitaryUnitByIdAsync(ObjectId id);
        Task<MilitaryUnit> UpdateMilitaryUnitAsync(ObjectId id, MilitaryUnit unit);
        Task<bool> DeleteMilitaryUnitAsync(ObjectId id);
        Task<List<MilitaryUnit>> GetAllMilitaryUnitsAsync();
    }
}