using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Enums;

namespace StrategyGame.Dto
{
    public class WorldSettingsDto
    {
        public string Id { get; set; } = string.Empty;
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
        // enum of the types of research on world -
        public ResearchModel ResearchModel { get; set; }
        public int? BoostedResearchBoost { get; set; }
    }
}