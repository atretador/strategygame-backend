using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Models
{
    public class AttackForce : MongoDbEntity
    {
        public string OriginCityId { get; set; }
        public string DestinationCityId { get; set; }
        public List<CityArmy> Units = new List<CityArmy>();
        public DateTime ArrivesAt { get; set; }
    }
}