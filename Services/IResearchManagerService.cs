using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Models;

namespace StrategyGame.Services
{
    public interface IResearchManagerService
    {
        Task<Research> AddResearchAsync(Research research);
        Task<bool> RemoveResearchAsync(ObjectId id);
        Task<Research> UpdateResearchAsync(ObjectId id, Research research);
        Task<Research> GetResearchByIdAsync(ObjectId id);
        Task<List<Research>> GetAllResearchesAsync();
    }
}