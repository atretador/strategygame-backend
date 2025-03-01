using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class Damage : MongoDbEntity
    {
        public DamageType DamageType;
        public int Amount { get; set; }
    }
}