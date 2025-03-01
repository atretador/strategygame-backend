using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StrategyGame.Models;
using StrategyGame.Requests;
using StrategyGame.Services;
using System.Threading.Tasks;

namespace StrategyGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CityController(ICityService cityService, UserManager<ApplicationUser> userManager)
        {
            _cityService = cityService;
            _userManager = userManager;
        }

        // Check if the user has a city
        [HttpGet("check")]
        public async Task<IActionResult> CheckUserCity([FromBody] string worldId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }
            var user = await _userManager.GetUserAsync(User);  // Get the current logged-in user
            var existingCity = await _cityService.UserHasCityAsync(user.Id, worldId);

            if (existingCity.Failed)
            {
                return Ok(new { Message = "You don't have a city yet.", HasCity = false });
            }

            return Ok(new { Message = "You already have a city.", HasCity = true });
        }

        // Create a city for the user if they don't have one
        [HttpPost("create")]
        public async Task<IActionResult> CreateCity([FromBody] CreateCityRequest request)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }
            
            var user = await _userManager.GetUserAsync(User);  // Get the current logged-in user
            var existingCity = await _cityService.UserHasCityAsync(user.Id, request.WorldId);

            if (existingCity.Succeeded)
            {
                return BadRequest(new { Message = "You already have a city." });
            }

            var city = await _cityService.CreateCityAsync(user.Id, request.WorldId, request.FactionId, request.CityName, request.Direction);
            return Ok(new { Message = "City created successfully", City = city });
        }

        // Return list of user controlled cities
        [HttpGet("listmycities")]
        public async Task<IActionResult> GetUserCities([FromBody] string worldId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            var user = await _userManager.GetUserAsync(User);  // Get the current logged-in user
            var cities = await _cityService.GetUserCitiesAsync(user.Id, worldId);

            return Ok(new { Message = "User cities retrieved successfully", Cities = cities });
        }
    }
}
