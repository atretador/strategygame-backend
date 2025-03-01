using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StrategyGame.Models
{
    public class Research : MongoDbEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    }
}