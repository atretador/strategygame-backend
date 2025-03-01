using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class Resource : MongoDbEntity
    {
        public string Name { get; set; } = string.Empty;

        public Resource()
        {}
    }
}