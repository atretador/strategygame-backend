using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class Requirement : MongoDbEntity
    {
        public RequirementType RequirementType { get; }
        public ObjectId ObjectId { get; } // Id of the object that is required
        public int RequiredLevel { get; } // Level of the object that is required

        public Requirement(ObjectId id, RequirementType type, int requiredLevel)
        {
            RequirementType = type;
            ObjectId = id;
            RequiredLevel = requiredLevel;
        }

        public bool IsMet(CityContents city)
        {
            switch (RequirementType)
            {
                case RequirementType.Building:
                    return city.Buildings.TryGetValue(ObjectId, out int blevel) && blevel >= RequiredLevel;
                case RequirementType.Research:
                    return city.Researches.TryGetValue(ObjectId, out int rlevel) && rlevel >= RequiredLevel;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string Description => $"Requires {RequirementType} with ID {ObjectId} at level {RequiredLevel}";
    }

}