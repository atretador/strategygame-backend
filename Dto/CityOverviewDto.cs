using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Enums;

namespace StrategyGame.Dto
{
    public class CityOverviewDto
    {
        // city name, id, coordinates, points, tag
        public CityIndexDto CityIndex { get; set; }
        public CityContentDto CityContent { get; set; }
    }
}