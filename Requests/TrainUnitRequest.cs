using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Requests
{
    public class TrainUnitRequest
    {
        public string CityId { get; set; }
        public string UnitId { get; set; }
        public int Amount { get; set; }
    }
}