using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Enums;

namespace StrategyGame.Requests
{
    public class CreateCityRequest
    {
        public string WorldId { get; set; }
        public string FactionId { get; set; }
        public Direction Direction { get; set; }
        public string CityName { get; set; } // Name of the city
    }
}