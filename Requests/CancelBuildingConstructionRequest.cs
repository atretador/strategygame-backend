using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Requests
{
    public class CancelBuildingConstructionRequest
    {
        public string CityId { get; set; }
        public string ConstructionId { get; set; }
    }
}