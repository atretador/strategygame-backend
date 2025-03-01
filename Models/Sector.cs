using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class Sector : MongoDbEntity
    {
        public ObjectId WorldId { get; set; } // Reference to the world
        public Tile TopLeftCorner { get; set; }
        public int SectorNumber { get; set; }
        public int Size { get; set; }
        public ClimateType Climate { get; set; }
        public List<Tile> Tiles { get; set; } = new();
    }
}