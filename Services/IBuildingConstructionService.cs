using MongoDB.Bson;
using StrategyGame.Models;
using System.Threading.Tasks;

namespace StrategyGame.Services
{
    public interface IBuildingConstructionService
    {
        /// <summary>
        /// Starts the construction of a building in the specified city.
        /// </summary>
        /// <param name="city">The city where the building construction should take place.</param>
        /// <param name="blueprint">The blueprint of the building to be constructed.</param>
        /// <param name="level">The level of the building being constructed.</param>
        Task StartBuildingConstructionAsync(City city, ObjectId building);

        /// <summary>
        /// Cancels an ongoing building construction.
        /// </summary>
        /// <param name="city">The city where the construction should be canceled.</param>
        /// <param name="construction">The building construction to be canceled.</param>
        Task CancelBuildingConstructionAsync(City city, OnQueueProcess construction);

        /// <summary>
        /// Destroys a building in the specified city.
        /// </summary>
        /// <param name="city">The city where the building should be destroyed.</param>
        /// <param name="building">The building to be destroyed.</param>
        Task DestroyBuildingAsync(City city, ObjectId building);
    }
}