using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace StrategyGame.Models
{
    public class BuildingBlueprint : MongoDbEntity
    {
        //time in seconds it take to build this
        public float BaseConstructionTime { get; set; }
        // eg BaseConstructionTime * level *  ContructionTimeMultiplierPerLevel * DefaultConstructionSpeed
        public float ContructionTimeMultiplierPerLevel { get; set; }
        public Dictionary<ObjectId, int> Price = new();
        public float ConstructionPriceMultiplierPerLevel { get; set; }
    }
}