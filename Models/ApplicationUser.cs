using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class ApplicationUser : IdentityUser
    {
        // we can keep track of the worlds the user is playing in
        // as well as their capital city on that world <WorldObjectId, CityObjectId>
        public Dictionary<string, string> PlayerWorlds = new();
        // player custom tags
        public Dictionary<CityTag, string> PlayerTags = new();
    }
}