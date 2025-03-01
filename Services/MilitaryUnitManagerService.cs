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
    public class MilitaryUnitManagerService : IMilitaryUnitManagerService
    {
        private readonly IMongoCollection<MilitaryUnit> _units;
        public MilitaryUnitManagerService(MongoDbContext dbContext)
        {
            _units = dbContext.MilitaryUnits;
        }

        public async Task<MilitaryUnit> AddMilitaryUnitAsync(MilitaryUnit unit)
        {
            await _units.InsertOneAsync(unit);
            return unit;
        }

        // Get a Military Unit by Id
        public async Task<MilitaryUnit> GetMilitaryUnitByIdAsync(ObjectId id)
        {
            var unit = await _units.Find(u => u.Id == id).FirstOrDefaultAsync();
            return unit ?? throw new InvalidOperationException("Unit not found");
        }

        // Get all Military Units
        public async Task<List<MilitaryUnit>> GetAllMilitaryUnitsAsync()
        {
            return await _units.Find(_ => true).ToListAsync();
        }

        // Update a Military Unit
        public async Task<MilitaryUnit> UpdateMilitaryUnitAsync(ObjectId id, MilitaryUnit unit)
        {
            var existingUnit = await _units.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (existingUnit == null) return null;

            existingUnit.Name = unit.Name;
            existingUnit.Price = unit.Price;
            existingUnit.Damage = unit.Damage;
            existingUnit.Defenses = unit.Defenses;
            existingUnit.Hp = unit.Hp;
            existingUnit.Population = unit.Population;

            await _units.ReplaceOneAsync(u => u.Id == id, existingUnit);
            return existingUnit;
        }

        // Delete a Military Unit
        public async Task<bool> DeleteMilitaryUnitAsync(ObjectId id)
        {
            var unit = _units.Find(u => u.Id == id).FirstOrDefault();
            if (unit == null) return false;

            await _units.DeleteOneAsync(u => u.Id == id);

            return true;
        }

    }
}