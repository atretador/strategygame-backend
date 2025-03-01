using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StrategyGame.Models
{
    public class MongoDbEntity
    {
        public ObjectId Id { get; set; }
        public string? LockToken { get; set; }
        public DateTime? LockExpiration { get; set; }
    }
}