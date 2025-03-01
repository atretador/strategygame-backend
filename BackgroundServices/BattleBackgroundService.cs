using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Models;
using StrategyGame.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StrategyGame.BackgroundServices
{
    public class BattleBackgroundService : BackgroundService
    {
        private readonly MongoDbContext _dbContext;
        private readonly ILogger<BattleBackgroundService> _logger;
        private readonly IUnitManagerService _unitManagerService;  // Inject the IUnitManagerService
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);  // Check every 30 seconds

        public BattleBackgroundService(MongoDbContext dbContext, 
                                       ILogger<BattleBackgroundService> logger,
                                       IUnitManagerService unitManagerService)  // Inject the service
        {
            _dbContext = dbContext;
            _logger = logger;
            _unitManagerService = unitManagerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Battle Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBattlesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing battles.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Battle Service stopped.");
        }

        private async Task ProcessBattlesAsync()
        {
            _logger.LogInformation("Checking for battles to process at {Time}", DateTime.Now);

            // Fetch all attacks that have arrived (ArrivesAt <= current time)
            var attackForces = await _dbContext.AttackForces
                                               .Find(af => af.ArrivesAt <= DateTime.Now)
                                               .ToListAsync();

            // Parallel processing of each battle
            var battleTasks = attackForces.Select(af => ProcessBattleAsync(af));
            await Task.WhenAll(battleTasks);
        }

        private async Task ProcessBattleAsync(AttackForce attackForce)
        {
            _logger.LogInformation($"Processing battle for attack force from city {attackForce.OriginCityId} to target city {attackForce.DestinationCityId}.");

            if (!ObjectId.TryParse(attackForce.DestinationCityId, out var objectId))
            {
                _logger.LogWarning($"City with ID {attackForce.DestinationCityId} not found. Skipping battle.");
                return;
            }

            // Get the defending city using the DestinationCityId
            var city = await _dbContext.Cities
                                       .Find(c => c.Id == objectId)  // Find the city by DestinationCityId
                                       .FirstOrDefaultAsync();

            if (city == null)
            {
                _logger.LogWarning($"City with ID {attackForce.DestinationCityId} not found. Skipping battle.");
                return;
            }

            // Get the defending army from the city's stored armies
            var defendingArmy = city.CityContents.Armies;

            // Now, let's process the battle with the attacking force and the defending army
            await ProcessUnitBattleAsync(attackForce, defendingArmy, city);  // Pass 'city' here
        }

        private async Task ProcessUnitBattleAsync(AttackForce attackForce, List<CityArmy> defendingArmy, City city)
        {
            _logger.LogInformation($"Starting battle between attacking force from {attackForce.OriginCityId} and defending army from {city.Name}.");

            // Calculate damage and resolve the battle
            foreach (var attackingUnit in attackForce.Units)
            {
                var defendingUnit = defendingArmy.FirstOrDefault(da => da.MilitaryUnit.Id == attackingUnit.MilitaryUnit.Id);

                if (defendingUnit != null)
                {
                    // Calculate the damage from attack and defense (you can implement your battle logic here)
                    var damage = CalculateBattleDamage(attackingUnit, defendingUnit);
                    // Apply damage to the defending units
                    defendingUnit.Units -= damage;

                    // Check if the defending unit is completely defeated
                    if (defendingUnit.Units <= 0)
                    {
                        // Remove the defeated unit from the city's army
                        await _unitManagerService.RemoveUnitsFromCityArmy(city, defendingUnit.MilitaryUnit, 1);
                    }
                }
            }

            // After the battle, check if any units were defeated in the attack force and remove them
            foreach (var attackingUnit in attackForce.Units)
            {
                // Check if the attacking unit was defeated (this example assumes units have health)
                if (attackingUnit.Units <= 0)
                {
                    // Remove the defeated unit from the attack force (can be done similarly to the defender's army)
                    // Example: Update the attack force or mark them as defeated
                }
            }

            _logger.LogInformation("Battle resolved. Updating armies.");

            // Here, the city’s army has been updated, and we assume the attack force is also updated.
            // The update logic may depend on whether you want to store the remaining units of the attack force.
        }

        private int CalculateBattleDamage(CityArmy attackingUnit, CityArmy defendingUnit)
        {
            // Implement your damage calculation logic here (e.g., rock-paper-scissors or other game mechanics)
            // For simplicity, let's just apply basic damage based on the unit counts
            int damage = Math.Min(attackingUnit.Units, defendingUnit.Units);
            return damage;
        }
    }
}