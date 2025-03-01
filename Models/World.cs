using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class World : MongoDbEntity
    {
        public string Name { get; set; }
        public List<ObjectId> Factions  { get; set; } = new();
        public List<ObjectId> UnitsDisabled  { get; set; } = new();
        public List<ObjectId> BuildingsDisabled  { get; set; } = new();
        public List<ObjectId> ResearchDisabled  { get; set; } = new();
        public List<ClimateType> EnabledClimates  { get; set; } = new();
        public WorldSettings Settings { get; set; }
        public WorldStatistics WorldStatistics { get; set; }
    }
}