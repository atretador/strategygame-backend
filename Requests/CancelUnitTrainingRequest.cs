using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Requests
{        
    public class CancelUnitTrainingRequest
    {
        public string CityId { get; set; }
        public string UnitTrainingId { get; set; }
    }
}