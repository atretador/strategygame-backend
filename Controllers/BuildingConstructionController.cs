using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Models;
using StrategyGame.Requests;
using StrategyGame.Services;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingConstructionController : ControllerBase
    {
        private readonly IBuildingConstructionService _buildingConstructionService;
        private readonly MongoDbContext _dbContext;
        private readonly ILogger<BuildingConstructionController> _logger;

        public BuildingConstructionController(
            IBuildingConstructionService buildingConstructionService,
            MongoDbContext dbContext,
            ILogger<BuildingConstructionController> logger)
        {
            _buildingConstructionService = buildingConstructionService;
            _dbContext = dbContext;
            _logger = logger;
        }

        // Endpoint to start building construction (automatic level handling)
        [HttpPost("start")]
        [Authorize]
        public async Task<IActionResult> StartBuildingConstruction([FromBody] StartBuildingConstructionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CityId) || string.IsNullOrEmpty(request.BuildingId))
            {
                return BadRequest("Invalid request.");
            }

            if (!ObjectId.TryParse(request.CityId, out var cityObjectId))
            {
                return BadRequest("Invalid CityId format.");
            }

            if (!ObjectId.TryParse(request.BuildingId, out var buildingObjectId))
            {
                return BadRequest("Invalid ID format.");
            }

            // Fetch the city by ObjectId
            var city = await _dbContext.Cities.Find(c => c.Id == cityObjectId).FirstOrDefaultAsync();

            if (city == null)
            {
                return NotFound("City not found.");
            }

            // Check if the user is the owner of the city
            var userId = User.Identity.Name; // Assuming the user is authenticated
            
            if (city.UserId != userId)
            {
                return Unauthorized("You do not have permission to build in this city.");
            }
            
            if(!city.CityContents.Buildings.TryGetValue(buildingObjectId, out var cityBuilding))
            {
                return NotFound("Building not found");
            }

            // Start the building construction with the updated building
            await _buildingConstructionService.StartBuildingConstructionAsync(city, buildingObjectId);

            return Ok(new { Message = $"Construction of {buildingObjectId} started for city {city.Name}." });
        }

        // Endpoint to cancel building construction
        [HttpPost("cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBuildingConstruction([FromBody] CancelBuildingConstructionRequest request)
        {
            if (string.IsNullOrEmpty(request.CityId) || string.IsNullOrEmpty(request.ConstructionId))
            {
                return BadRequest("Invalid request.");
            }

            if (!ObjectId.TryParse(request.CityId, out var cityObjectId))
            {
                return BadRequest("Invalid CityId format.");
            }

            if (!ObjectId.TryParse(request.ConstructionId, out var constructionObjectId))
            {
                return BadRequest("Invalid CityId format.");
            }

            // Fetch the city by ObjectId
            var city = await _dbContext.Cities.Find(c => c.Id == cityObjectId).FirstOrDefaultAsync();

            if (city == null)
            {
                return NotFound("City not found.");
            }

            // Check if the user is the owner of the city
            var userId = User.Identity.Name; // Assuming the user is authenticated
            if (city.UserId != userId)
            {
                return Unauthorized("You do not have permission to cancel construction in this city.");
            }

            // Fetch the construction to be canceled
            var construction = city.CityContents.ConstructionQueu
                .FirstOrDefault(bc => bc.Id == constructionObjectId);

            if (construction == null)
            {
                return NotFound("Construction not found.");
            }

            // Cancel the construction
            await _buildingConstructionService.CancelBuildingConstructionAsync(city, construction);

            return Ok(new { Message = $"Construction of building {constructionObjectId} canceled on {cityObjectId} City." });
        }

        // Endpoint to destroy a building
        [HttpPost("destroy")]
        [Authorize]
        public async Task<IActionResult> DestroyBuilding([FromBody] DestroyBuildingRequest request)
        {
            if (string.IsNullOrEmpty(request.CityId) || string.IsNullOrEmpty(request.BuildingId))
            {
                return BadRequest("Invalid request.");
            }

            if (!ObjectId.TryParse(request.CityId, out var cityObjectId))
            {
                return BadRequest("Invalid CityId format.");
            }

            // Fetch the city by ObjectId
            var city = await _dbContext.Cities.Find(c => c.Id == cityObjectId).FirstOrDefaultAsync();

            if (city == null)
            {
                return NotFound("City not found.");
            }

            // Check if the user is the owner of the city
            var userId = User.Identity.Name; // Assuming the user is authenticated
            if (city.UserId != userId)
            {
                return Unauthorized("You do not have permission to destroy buildings in this city.");
            }

            if (!ObjectId.TryParse(request.BuildingId, out var buildingObjectId))
            {
                return BadRequest("Invalid ID format.");
            }

            if(!city.CityContents.Buildings.TryGetValue(buildingObjectId, out var cityBuilding))
            {
                return NotFound("Building not found");
            }


            // Destroy the building
            await _buildingConstructionService.DestroyBuildingAsync(city, buildingObjectId);

            return Ok(new { Message = $"Building {buildingObjectId} destroyed on {cityObjectId}." });
        }
    }
}