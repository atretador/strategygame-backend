using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StrategyGame.Models
{
    public class Faction : MongoDbEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // we can make each faction have a different start
        public CityContents GetStartingCity()
        {
            throw new NotImplementedException();
        }
    }
}