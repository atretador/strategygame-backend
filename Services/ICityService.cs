using System.Threading.Tasks;
using StrategyGame.Enums;
using StrategyGame.Models;
using StrategyGame.Result;

namespace StrategyGame.Services
{
    public interface ICityService
    {
        Task<TaskResult<bool>> UserHasCityAsync(string userId, string worldId);
        Task<TaskResult<City>> GetUserCityAsync(string userId, string worldId, string cityId);
        Task<TaskResult<List<City>>> GetUserCitiesAsync(string userId, string worldId);
        Task<TaskResult<City>> CreateCityAsync(string userId, string worldId, string FactionId, string cityName, Direction direction);
    }
}
