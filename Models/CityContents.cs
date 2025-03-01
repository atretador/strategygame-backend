using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace StrategyGame.Models
{
    public class CityContents : MongoDbEntity
    {
        public ObjectId CityId; // reference to the city
        public List<CityArmy> Armies = new List<CityArmy>();
        public Dictionary<ObjectId, int> ResourceStorage = new Dictionary<ObjectId, int>();
        public Dictionary<ObjectId, int> Buildings = new Dictionary<ObjectId, int>();
        public Dictionary<ObjectId, int> Researches = new Dictionary<ObjectId, int>();
        
        // queue for buildings, researches and unit training
        public List<OnQueueProcess> ResearchQueu = new List<OnQueueProcess>();
        public List<OnQueueProcess> UnitTrainingQueu = new List<OnQueueProcess>();
        public List<OnQueueProcess> ConstructionQueu = new List<OnQueueProcess>();

        public int GetBuildingLevel(ObjectId buildingId)
        {
            if (Buildings.TryGetValue(buildingId, out int level))
            {
                return level;
            }
            return 0;
        }

        public int GetResearchLevel(ObjectId researchId)
            => Researches.TryGetValue(researchId, out int level) ? level : 0;

    }
}