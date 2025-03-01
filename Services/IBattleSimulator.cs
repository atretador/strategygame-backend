using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StrategyGame.Models;
using StrategyGame.Result;

namespace StrategyGame.Services
{
    public interface IBattleSimulator
    {
        /// <summary>
        /// Simulates a battle between an attacking force and a defending city.
        /// </summary>
        /// <param name="attackForce">The attacking force.</param>
        /// <param name="city">The defending city.</param>
        /// <returns>A task that represents the battle result.</returns>
        Task<BattleResult> SimulateBattleAsync(AttackForce attackForce, City city);

        /// <summary>
        /// Calculates the strength contribution of each unit type in an army.
        /// </summary>
        /// <param name="army">The army to calculate strength for.</param>
        /// <returns>A dictionary where the key is the unit type, and the value is the strength contribution.</returns>
        Dictionary<MilitaryUnit, float> CalculateUnitStrengthContribution(List<CityArmy> army);

        /// <summary>
        /// Applies damage to an army, updating the number of units.
        /// </summary>
        /// <param name="army">The army to apply damage to.</param>
        /// <param name="damage">The damage to apply.</param>
        void ApplyDamageToArmy(List<CityArmy> army, float damage);

        /// <summary>
        /// Simulates the battle rounds between an attacking force and a defending city.
        /// </summary>
        /// <param name="attackForce">The attacking force.</param>
        /// <param name="city">The defending city.</param>
        /// <returns>A task that represents the battle result after all rounds.</returns>
        Task<BattleResult> SimulateBattleRoundsAsync(AttackForce attackForce, City city);

        /// <summary>
        /// Calculates the damage between two specific units using the battle logic (e.g., rock-paper-scissors).
        /// </summary>
        /// <param name="attacker">The attacking army unit.</param>
        /// <param name="defender">The defending army unit.</param>
        /// <returns>The calculated damage.</returns>
        float CalculateDamage(CityArmy attacker, CityArmy defender);

        /// <summary>
        /// Generates the battle result based on the army status after the battle.
        /// </summary>
        /// <param name="attackerUnits">The list of surviving attacker units.</param>
        /// <param name="defenderUnits">The list of surviving defender units.</param>
        /// <returns>The battle result.</returns>
        BattleResult GenerateBattleResult(List<CityArmy> attackerUnits, List<CityArmy> defenderUnits);
    }
}