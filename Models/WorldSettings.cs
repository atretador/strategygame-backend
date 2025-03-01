using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class WorldSettings : MongoDbEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TickRateInMilliseconds { get; set; }
        public int ResourceBaseProductionRate { get; set; }
        public float ResourceProductionGrowthFactor { get; set; }
        public float DefaultUnitMovementSpeed { get; set; }
        public float DefaultUnitTrainingSpeed { get; set; }
        public float DefaultConstructionSpeed { get; set; }
        public float DefaultResearchSpeed { get; set; }
        public int MaxSectorColumns { get; set; }
        public int MaxSectorRows { get; set; }
        public int SectorSize { get; set; }
        public ResearchModel ResearchModel { get; set; }
        public int? BoostedResearchBoost { get; set; }
    }
}