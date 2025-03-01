using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Enums;
using StrategyGame.Models;

namespace StrategyGame.Dto
{
    public class BuildingDto
    {
        public string Name { get; set; } = string.Empty;
        public BuildingType BuildingType;
        public BuildingBlueprint BuildingBlueprint { get; set; }
        public string FactionId { get; set; }
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}