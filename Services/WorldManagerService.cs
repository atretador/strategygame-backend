using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Enums;
using StrategyGame.Models;
using StrategyGame.Result;

namespace StrategyGame.Services
{
    public class WorldManagerService : IWorldManagerService
    {
        private readonly IMongoCollection<World> _worlds;
        private readonly IMongoCollection<Sector> _sectors;
        private readonly Random _random = new();
        private readonly IMongoCollection<WorldSettings> _worldSettingsCollection;

        public WorldManagerService(MongoDbContext dbcontext)
        {
            _worlds = dbcontext.Worlds;
            _sectors = dbcontext.Sectors;
            _worldSettingsCollection = dbcontext.WorldSettings;
        }


        // Retrieve WorldSettings from cache or database
        public async Task<WorldSettings> GetWorldSettingsAsync(ObjectId settingsId)
        {
            // Fetch from database if not in cache
            var settings = await _worldSettingsCollection
                .Find(w => w.Id == settingsId)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                return null; // No settings found
            }

            return settings;
        }

        public async Task<List<WorldSettings>> GetAllWorldSettingsAsync()
        {
            var settings = await _worldSettingsCollection
                .Find(w => true)
                .ToListAsync();

            return settings;
        }

        // Add WorldSettings
        public async Task<WorldSettings> AddWorldSettingsAsync(WorldSettings settings)
        {
            // Ensure no duplicate world settings by Id
            var existingSettings = await _worldSettingsCollection
                .Find(w => w.Id == settings.Id)
                .FirstOrDefaultAsync();
            if (existingSettings != null)
            {
                return null; // WorldSettings with this Id already exists
            }

            await _worldSettingsCollection.InsertOneAsync(settings);
            return settings;
        }

        // Update WorldSettings
        public async Task<WorldSettings> UpdateWorldSettingsAsync(ObjectId settingsId, WorldSettings settings)
        {
            var existingSettings = await _worldSettingsCollection
                .Find(w => w.Id == settingsId)
                .FirstOrDefaultAsync();
            if (existingSettings == null)
            {
                return null; // No world settings found to update
            }

            settings.Id = settingsId; // Ensure the Id remains unchanged
            await _worldSettingsCollection.ReplaceOneAsync(
                w => w.Id == settingsId, settings
            );
            return settings;
        }

        // Remove WorldSettings
        public async Task<bool> RemoveWorldSettingsAsync(ObjectId settingsId)
        {
            var result = await _worldSettingsCollection.DeleteOneAsync(w => w.Id == settingsId);
            if (result.DeletedCount > 0)
            {
                return true;
            }
            return false;
        }

        // world creation
        public async Task<TaskResult<World>> GenerateWorldAsync(string name, string worldSettingsId)
        {
            //check if worldsettingsid is valid ObjectId
            if (!ObjectId.TryParse(worldSettingsId, out ObjectId worldSettingsObjectId))
            {
                return new TaskResult<World>(TaskOutcome.Fail, "Invalid world settings id.", null);
            }

            var worldSettings = await GetWorldSettingsAsync(worldSettingsObjectId);

            // Get the world settings from cache

            World world = new World
            {
                Name = name,
                Settings = worldSettings
            };

            await _worlds.InsertOneAsync(world);

            // Generate sectors in a grid pattern, left to right, top to bottom
            for (int row = 0; row < worldSettings.MaxSectorRows; row++)
            {
                for (int col = 0; col < worldSettings.MaxSectorColumns; col++)
                {
                    int sectorNumber = row * worldSettings.MaxSectorColumns + col;
                    var sector = new Sector
                    {
                        WorldId = world.Id,
                        SectorNumber = sectorNumber,
                        TopLeftCorner = new Tile(row * worldSettings.SectorSize, col * worldSettings.SectorSize),
                        Size = worldSettings.SectorSize
                    };

                    // Generate positions within the sector, some will be occupied
                    for (int i = 0; i < sector.Size; i++)
                    {
                        for (int j = 0; j < sector.Size; j++)
                        {
                            // Randomly determine if the position is occupied (30% chance)
                            var RandomDecoration = _random.Next(0, 100) < 30 ? (DecorationType)_random.Next(1, 3) : DecorationType.None;

                            // Add position with all required arguments: row, column, and isOccupied
                            sector.Tiles.Add(new Tile(row * worldSettings.SectorSize + i, col * worldSettings.SectorSize + j, RandomDecoration));
                        }
                    }
                    await _sectors.InsertOneAsync(sector);
                }
            }

            return new TaskResult<World>(TaskOutcome.Success, "World generated successfully.", world);
        }

        public async Task<TaskResult<World>> GetWorldAsync(ObjectId worldId)
        {
            var world = await _worlds.Find(w => w.Id == worldId).FirstOrDefaultAsync();
            if (world == null)
            {
                return new TaskResult<World>(TaskOutcome.Fail, "World not found.", null);
            }

            return new TaskResult<World>(TaskOutcome.Success, "World found.", world);
        }

        public async Task<TaskResult<List<World>>> GetWorldsAsync()
        {
            var worlds = await _worlds.Find(w => true).ToListAsync();
            if (worlds == null)
            {
                return new TaskResult<List<World>>(TaskOutcome.Fail, "Worlds not found.", null);
            }

            return new TaskResult<List<World>>(TaskOutcome.Success, "Worlds found.", worlds);
        }

        public async Task<TaskResult<List<Sector>>> GetSectorsAsync(ObjectId worldId)
        {
            var sectors = await _sectors.Find(s => s.WorldId == worldId).ToListAsync();
            if (sectors == null)
            {
                return new TaskResult<List<Sector>>(TaskOutcome.Fail, "Sectors not found.", null);
            }

            return new TaskResult<List<Sector>>(TaskOutcome.Success, "Sectors found.", sectors);
        }

        public async Task<TaskResult<Sector>> GetSectorAsync(ObjectId sectorId)
        {
            var sector = await _sectors.Find(s => s.Id == sectorId).FirstOrDefaultAsync();
            if (sector == null)
            {
                return new TaskResult<Sector>(TaskOutcome.Fail, "Sector not found.", null);
            }

            return new TaskResult<Sector>(TaskOutcome.Success, "Sector found.", sector);
        }
    }
}