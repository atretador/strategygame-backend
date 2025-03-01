using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Models;

namespace StrategyGame.Services
{
    public interface IFactionManagerService
    {
        Task<Faction> AddFactionAsync(Faction faction);
        Task<Faction> GetFactionByIdAsync(ObjectId id);
        Task<Faction> UpdateFactionAsync(ObjectId id, Faction faction);
        Task<bool> RemoveFactionAsync(ObjectId id);
        Task<List<Faction>> GetAllFactionsAsync();
    }
}