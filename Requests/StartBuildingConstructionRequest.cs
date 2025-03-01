using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Requests
{
    public class StartBuildingConstructionRequest
    {
        public string CityId { get; set; }
        public string BuildingId { get; set; }
    }
}