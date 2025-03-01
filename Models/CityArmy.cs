using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Models
{
    public class CityArmy
    {
        public int Units;
        public MilitaryUnit MilitaryUnit;

        public CityArmy(MilitaryUnit unit, int amount)
        {
            Units = amount;
            MilitaryUnit = unit;
        }
    }
}