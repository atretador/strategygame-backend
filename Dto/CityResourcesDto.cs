using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Dto
{
    public class CityResourcesDto
    {
        // in case we just want to return the resources of a city, instead of the entire city object
        // resource ObjectId and amount
        public Dictionary<string, int> Resources { get; set; }
    }
}