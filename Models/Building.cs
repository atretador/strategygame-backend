using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class Building : MongoDbEntity
    {
        public string Name { get; set; } = string.Empty;
        public BuildingType BuildingType;
        public BuildingBlueprint BuildingBlueprint { get; set; }
        public ObjectId FactionId { get; set; }
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}