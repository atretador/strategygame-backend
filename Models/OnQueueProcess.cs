using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class OnQueueProcess : MongoDbEntity
    {
        public QueueType Type { get; set; } // building, research or unit
        public ObjectId Id { get; set; } // references the building, research or units being processed
        public DateTime StartedAt { get; set; }
        public DateTime ReadyAt { get; set; }
        public int? Quantity { get; set; } // only for units
    }
}