using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Models
{
    public class WorldStatistics
    {
        public int PlayersRegistered { get; set; }
        public int PlayerSettlements { get; set; }
        public int UnclaimedSettlements { get; set; }
        public long TotalUnitsProduced { get; set; }
        public long UnitsLost { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}