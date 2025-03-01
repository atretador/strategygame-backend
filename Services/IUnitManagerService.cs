using MongoDB.Bson;
using StrategyGame.Models;
using System.Threading.Tasks;

namespace StrategyGame.Services
{
    public interface IUnitManagerService
    {
        Task StartUnitTrainingAsync(City city, MilitaryUnit unit, int amount);
        Task CancelUnitTrainingAsync(City city, OnQueueProcess unitTraining);
        Task RemoveUnitsFromCityArmy(City city, MilitaryUnit unit, int amount);
        Task AddUnitsToCityArmy(City city, MilitaryUnit unit, int amount);
    }
}