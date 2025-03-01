using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Models;

namespace StrategyGame.Services
{
    public class FactionManagerService : IFactionManagerService
    {
        private readonly IMongoCollection<Faction> _factions;

        public FactionManagerService(MongoDbContext dbContext)
        {
            _factions = dbContext.Factions;
        }
        // Add a Faction
        public async Task<Faction> AddFactionAsync(Faction faction)
        {
            await _factions.InsertOneAsync(faction);
            return faction;
        }

        // Remove a Faction
        public async Task<bool> RemoveFactionAsync(ObjectId id)
        {
            await _factions.DeleteOneAsync(f => f.Id == id);
            return true;
        }

        // Update a Faction
        public async Task<Faction> UpdateFactionAsync(ObjectId id, Faction faction)
        {
            var existingFaction = await _factions.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (existingFaction == null) return null;

            existingFaction.Name = faction.Name;
            existingFaction.Description = faction.Description;
            await _factions.ReplaceOneAsync(f => f.Id == id, existingFaction);

            return existingFaction;
        }

        // Get a Faction by Id
        public async Task<Faction> GetFactionByIdAsync(ObjectId id)
        {
            return await _factions.Find(f => f.Id == id).FirstOrDefaultAsync();
        }

        // Get All Factions
        public async Task<List<Faction>> GetAllFactionsAsync()
        {
            return await _factions.Find(_ => true).ToListAsync();
        }
    }
}