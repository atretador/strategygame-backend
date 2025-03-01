using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Models;
using StrategyGame.Enums;
using StrategyGame.Result;

namespace StrategyGame.Services
{
    public class BattleSimulator : IBattleSimulator
    {
        public async Task<BattleResult> SimulateBattleAsync(AttackForce attackForce, City city)
        {
            var defendingArmy = city.CityContents.Armies;

            // Simulate battle rounds until either force is eliminated
            return await SimulateBattleRoundsAsync(attackForce, city);
        }

        public Dictionary<MilitaryUnit, float> CalculateUnitStrengthContribution(List<CityArmy> army)
        {
            var strengthContributions = new Dictionary<MilitaryUnit, float>();

            foreach (var armyUnit in army)
            {
                var unitStrength = armyUnit.MilitaryUnit.Damage.Amount;
                strengthContributions.Add(armyUnit.MilitaryUnit, unitStrength * armyUnit.Units);
            }

            return strengthContributions;
        }

        public void ApplyDamageToArmy(List<CityArmy> army, float damage)
        {
            // We apply damage and reduce units accordingly
            foreach (var armyUnit in army)
            {
                var unitsToKill = (int)(damage / armyUnit.MilitaryUnit.Damage.Amount);

                if (armyUnit.Units <= unitsToKill)
                {
                    armyUnit.Units = 0; // Unit dies
                }
                else
                {
                    armyUnit.Units -= unitsToKill;
                }

                if (armyUnit.Units <= 0)
                {
                    // Remove units from the army once they reach 0
                    army.Remove(armyUnit);
                }
            }
        }

        public async Task<BattleResult> SimulateBattleRoundsAsync(AttackForce attackForce, City city)
        {
            var attackingArmy = attackForce.Units;
            var defendingArmy = city.CityContents.Armies;

            while (attackingArmy.Any() && defendingArmy.Any())
            {
                var attackerStrengths = CalculateUnitStrengthContribution(attackingArmy);
                var defenderStrengths = CalculateUnitStrengthContribution(defendingArmy);

                var totalAttackerStrength = attackerStrengths.Values.Sum();
                var totalDefenderStrength = defenderStrengths.Values.Sum();

                // Each unit attacks proportionally to its strength
                var totalDamage = (totalAttackerStrength / totalDefenderStrength) * totalAttackerStrength;

                // Calculate damage based on unit type matchups (rock-paper-scissors logic)
                foreach (var attacker in attackingArmy)
                {
                    foreach (var defender in defendingArmy)
                    {
                        var damage = CalculateDamage(attacker, defender);
                        ApplyDamageToArmy(new List<CityArmy> { defender }, damage);
                    }
                }

                // If defending army is depleted, attacker wins
                if (!defendingArmy.Any(a => a.Units > 0))
                {
                    return GenerateBattleResult(attackingArmy, defendingArmy);
                }

                // If attacking army is depleted, defender wins
                if (!attackingArmy.Any(a => a.Units > 0))
                {
                    return GenerateBattleResult(attackingArmy, defendingArmy);
                }
            }

            return GenerateBattleResult(attackingArmy, defendingArmy);
        }

        public float CalculateDamage(CityArmy attacker, CityArmy defender)
        {
            // Rock-paper-scissors logic
            if (attacker.MilitaryUnit.Damage.DamageType == defender.MilitaryUnit.Damage.DamageType)
            {
                return attacker.MilitaryUnit.Damage.DamageType == DamageType.Rock ? attacker.MilitaryUnit.Damage.Amount * 1f :
                       attacker.MilitaryUnit.Damage.DamageType == DamageType.Paper ? attacker.MilitaryUnit.Damage.Amount * 1f :
                       attacker.MilitaryUnit.Damage.Amount * 1f;
            }
            else if ((attacker.MilitaryUnit.Damage.DamageType == DamageType.Rock && defender.MilitaryUnit.Damage.DamageType == DamageType.Scissors) ||
                     (attacker.MilitaryUnit.Damage.DamageType == DamageType.Scissors && defender.MilitaryUnit.Damage.DamageType == DamageType.Paper) ||
                     (attacker.MilitaryUnit.Damage.DamageType == DamageType.Paper && defender.MilitaryUnit.Damage.DamageType == DamageType.Rock))
            {
                return attacker.MilitaryUnit.Damage.Amount * 2f; // Double damage for attacker winning
            }
            else
            {
                return attacker.MilitaryUnit.Damage.Amount * 0.5f; // Half damage if defender is the counter
            }
        }

        public BattleResult GenerateBattleResult(List<CityArmy> attackerUnits, List<CityArmy> defenderUnits)
        {
            var attackerSurvivors = attackerUnits.Where(u => u.Units > 0).ToList();
            var defenderSurvivors = defenderUnits.Where(u => u.Units > 0).ToList();

            var winner = attackerSurvivors.Any() ? "Attacker" : "Defender";
            var survivorUnits = winner == "Attacker" ? attackerSurvivors : defenderSurvivors;

            // Create the BattleResult object
            var battleResult = new BattleResult
            {
                Winner = winner,
                Survivors = survivorUnits,
                BattleOutcomeTime = DateTime.Now
            };

            // Populate the RemainingUnits list
            var remainingUnits = new List<RemainingUnit>();
            foreach (var unit in survivorUnits)
            {
                remainingUnits.Add(new RemainingUnit
                {
                    UnitName = unit.MilitaryUnit.Name,
                    UnitsRemaining = unit.Units
                });
            }

            battleResult.RemainingUnits = remainingUnits;

            return battleResult;
        }
    }
}