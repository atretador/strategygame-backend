using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Enums;
using StrategyGame.Models;
using StrategyGame.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Services
{
    public class CityService : ICityService
    {
        private readonly IMongoCollection<City> _cities;
        private readonly IMongoCollection<Sector> _sectors;
        private readonly IMongoCollection<World> _worlds;
        private readonly IWorldManagerService _worldService;
        private readonly IFactionManagerService _factionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CityService(MongoDbContext dbContext,
                           UserManager<ApplicationUser> userManager,
                           IFactionManagerService factionService,
                           IWorldManagerService worldService)
        {
            _cities = dbContext.Cities;
            _sectors = dbContext.Sectors;
            _worlds = dbContext.Worlds;
            _worldService = worldService;
            _factionService = factionService;
            _userManager = userManager;
        }

        public async Task<TaskResult<bool>> UserHasCityAsync(string userId, string worldId)
        {
            if (!ObjectId.TryParse(worldId, out var worldObjectId))
                return new TaskResult<bool>(TaskOutcome.Fail, "Invalid world ID.", false);

            var hasCity = await _cities.CountDocumentsAsync(city => city.UserId == userId && city.WorldId == worldObjectId) > 0;
            return new TaskResult<bool>(TaskOutcome.Success, "City check completed successfully.", hasCity);
        }

        public async Task<TaskResult<City>> GetUserCityAsync(string userId, string worldId, string cityId)
        {
            if (!ObjectId.TryParse(worldId, out var worldObjectId))
                return new TaskResult<City>(TaskOutcome.Fail, "Invalid world ID.", null);

            if (!ObjectId.TryParse(cityId, out var cityObjectId))
                return new TaskResult<City>(TaskOutcome.Fail, "Invalid city ID.", null);

            var city = await _cities.Find(city => city.UserId == userId && city.WorldId == worldObjectId && city.Id == cityObjectId).FirstOrDefaultAsync();
            if (city == null)
                return new TaskResult<City>(TaskOutcome.Fail, "City not found.", null);

            return new TaskResult<City>(TaskOutcome.Success, "City found successfully.", city);
        }

        public async Task<TaskResult<List<City>>> GetUserCitiesAsync(string userId, string worldId)
        {
            if (!ObjectId.TryParse(worldId, out var worldObjectId))
                return new TaskResult<List<City>>(TaskOutcome.Fail, "Invalid world ID.", null);

            var cities = await _cities.Find(city => city.UserId == userId && city.WorldId == worldObjectId).ToListAsync();
            return new TaskResult<List<City>>(TaskOutcome.Success, "Cities retrieved successfully.", cities);
        }

        public async Task<TaskResult<City>> CreateCityAsync(string userId, string selectedWorld, string factionId, string cityName, Direction direction)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new TaskResult<City>(TaskOutcome.Fail, "User not found.", null);

            if (!ObjectId.TryParse(selectedWorld, out var worldId))
                return new TaskResult<City>(TaskOutcome.Fail, "Invalid world ID.", null);

            var world = await _worlds.Find(w => w.Id == worldId).FirstOrDefaultAsync();
            if (world == null)
                return new TaskResult<City>(TaskOutcome.Fail, "World not found.", null);
            
            if (!ObjectId.TryParse(factionId, out var factionObjectId))
                return new TaskResult<City>(TaskOutcome.Fail, "Invalid faction ID.", null);


            var faction = await _factionService.GetFactionByIdAsync(factionObjectId);

            // calculate coordinates
            // check for unclaimed Settlements spawn

            var newCity = new City
            {
                UserId = userId,
                WorldId = worldId,
                FactionId = factionObjectId,
                Name = cityName,
                CityContents = faction.GetStartingCity()           
            };

            await _cities.InsertOneAsync(newCity);

            return new TaskResult<City>(TaskOutcome.Success, "City created successfully.", newCity);
        }

        private async Task<(int x, int y)> CalculateCityCoordinates(Sector sector, Direction direction, int sectorSize, ObjectId worldId)
        {
            WorldSettings settings = await _worldService.GetWorldSettingsAsync(worldId);

            int x = settings.MaxSectorColumns * sectorSize;
            int y = settings.MaxSectorRows * sectorSize;

            var directionOffsets = new Dictionary<Direction, (int dx, int dy)>
            {
                { Direction.North, (0, 1) },
                { Direction.Northeast, (1, 1) },
                { Direction.East, (1, 0) },
                { Direction.Southeast, (1, -1) },
                { Direction.South, (0, -1) },
                { Direction.Southwest, (-1, -1) },
                { Direction.West, (-1, 0) },
                { Direction.Northwest, (-1, 1) }
            };

            if (direction == Direction.Random)
            {
                var random = new Random();
                x += random.Next(-sectorSize, sectorSize);
                y += random.Next(-sectorSize, sectorSize);
            }
            else if (directionOffsets.TryGetValue(direction, out var offset))
            {
                x += offset.dx;
                y += offset.dy;
            }

            return (x, y);
        }
    }
}
