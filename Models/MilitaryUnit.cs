using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StrategyGame.Models
{
    public class MilitaryUnit : MongoDbEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Population { get; set; }
        public Dictionary<ObjectId, int> Price { get; set; } = new();
        // damage resistance %
        public List<Damage> Defenses { get; set; }
        public int Hp { get; set; }
        public Damage Damage { get; set; }
        // tiles per tick
        public int MovementSpeed { get; set; }
        public ObjectId ProducedAt { get; set; }
        public int TrainingTime { get; set; }
        public ObjectId FactionId { get; set; }
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}