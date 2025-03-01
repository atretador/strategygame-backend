using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Enums
{
    public static class EnumUtilities
    {
        private static readonly Random Random = new();

        /// <summary>
        /// Gets a random value from an enum.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>A random enum value.</returns>
        public static T GetRandomEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Next(values.Length))!;
        }
    }
}