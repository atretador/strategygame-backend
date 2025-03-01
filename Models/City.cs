using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class City : MongoDbEntity
    {
        public string? UserId { get; set; } // Null if the city is unclaimed Settlements
        public string Name { get; set; } = string.Empty;
        public ObjectId WorldId { get; set; }
        public ObjectId SectorId { get; set; } // Reference to the sector
        public int X { get; set; }
        public int Y { get; set; }
        public ObjectId FactionId { get; set; }
        public CityContents CityContents { get; set; } = new CityContents();
        
        
        public City()
        {}
    }
}