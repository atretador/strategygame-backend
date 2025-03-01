using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Enums;
using StrategyGame.Models;

namespace StrategyGame.Dto
{
    public class CityIndexDto
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Sector { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Points { get; set; }
        public CityTag Tag { get; set; }
    }
}