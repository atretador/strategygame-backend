using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace StrategyGame.Result
{
        public class ResourceValidationResult
        {
            public bool HasEnough { get; set; }
            public Dictionary<ObjectId, int> MissingResources { get; set; }

            public ResourceValidationResult()
            {
                MissingResources = new Dictionary<ObjectId, int>();
            }
        }

}