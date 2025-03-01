using MongoDB.Bson;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class ResourceBuilding : Building
    {
        public ObjectId ResourceId { get; set; } // which resource we produce
    }
}