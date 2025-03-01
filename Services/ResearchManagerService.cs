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
    public class ResearchManagerService : IResearchManagerService
    {
        private readonly IMongoCollection<Research> _researches;

        public ResearchManagerService(MongoDbContext dbContext)
        {
            _researches = dbContext.Researches;
        }

        // Add a Research
        public async Task<Research> AddResearchAsync(Research research)
        {
            await _researches.InsertOneAsync(research);
            return research;
        }

        // Remove a Research
        public async Task<bool> RemoveResearchAsync(ObjectId id)
        {
            var deleteResult = await _researches.DeleteOneAsync(r => r.Id == id);
            return deleteResult.DeletedCount > 0;
        }

        // Update a Research 
        public async Task<Research> UpdateResearchAsync(ObjectId id, Research research)
        {
            var existingResearch = await _researches.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (existingResearch == null) return null;

            existingResearch.Name = research.Name;

            await _researches.ReplaceOneAsync(r => r.Id == id, existingResearch);
            return existingResearch;
        }

        // Get a Research by Id
        public async Task<Research> GetResearchByIdAsync(ObjectId id)
        {
            return await _researches.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        // Get All Researches
        public async Task<List<Research>> GetAllResearchesAsync()
        {
            return await _researches.Find(_ => true).ToListAsync();
        }
    }
}