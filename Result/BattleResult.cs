using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Models;

namespace StrategyGame.Result
{
    public class BattleResult
    {
        // The winner of the battle ("Attacker" or "Defender")
        public string Winner { get; set; }

        // List of surviving units
        public List<CityArmy> Survivors { get; set; }

        // List of remaining units with their count
        public List<RemainingUnit> RemainingUnits { get; set; }

        // Time when the battle outcome was decided
        public DateTime BattleOutcomeTime { get; set; }

        public BattleResult()
        {
            Survivors = new List<CityArmy>();
            RemainingUnits = new List<RemainingUnit>();
        }
    }

    public class RemainingUnit
    {
        public string UnitName { get; set; }
        public int UnitsRemaining { get; set; }
    }
}