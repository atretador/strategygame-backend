using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using StrategyGame.Context;
using StrategyGame.Models;
using StrategyGame.Requests;
using StrategyGame.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StrategyGame.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitTrainingController : ControllerBase
    {
        private readonly IUnitManagerService _UnitManagerService;
        private readonly MongoDbContext _dbContext;
        private readonly ILogger<UnitTrainingController> _logger;
        private readonly IMilitaryUnitManagerService _militaryService;

        public UnitTrainingController(
            IUnitManagerService UnitManagerService,
            MongoDbContext dbContext,
            ILogger<UnitTrainingController> logger,
            IMilitaryUnitManagerService militaryService)
        {
            _UnitManagerService = UnitManagerService;
            _dbContext = dbContext;
            _logger = logger;
            _militaryService = militaryService;
        }

        // Endpoint to start unit training
        [HttpPost("train")]
        [Authorize]
        public async Task<IActionResult> TrainUnit([FromBody] TrainUnitRequest request)
        {
            if (string.IsNullOrEmpty(request.CityId) || string.IsNullOrEmpty(request.UnitId) || request.Amount <= 0)
            {
                return BadRequest("Invalid request.");
            }

            if (!ObjectId.TryParse(request.CityId, out var objectId))
            {
                return BadRequest("Invalid CityId format.");
            }

            // Fetch the city by ObjectId
            var city = await _dbContext.Cities.Find(c => c.Id == objectId).FirstOrDefaultAsync();

            if (city == null)
            {
                return NotFound("City not found.");
            }

            // Check if the user is the owner of the city
            var userId = User.Identity.Name; // Assuming the user is authenticated
            if (city.UserId != userId)
            {
                return Unauthorized("You do not have permission to train units in this city.");
            }

            if (!ObjectId.TryParse(request.UnitId, out var unitId))
            {
                return BadRequest("Invalid unitId format.");
            }

            var unit = await _militaryService.GetMilitaryUnitByIdAsync(unitId);
            if (unit == null)
            {
                _logger.LogError($"Military unit with ID {request.UnitId} not found.");
                return BadRequest($"Military unit with ID {request.UnitId} not found.");
            }

            // Start unit training if resources are available
            await _UnitManagerService.StartUnitTrainingAsync(city, unit, request.Amount);

            return Ok(new { Message = $"Training of {request.Amount} {unit.Name} units started in city {city.Name}." });
        }

        // Endpoint to cancel unit training
        [HttpPost("cancel")]
        [Authorize]
        public async Task<IActionResult> CancelUnitTraining([FromBody] CancelUnitTrainingRequest request)
        {
            if (string.IsNullOrEmpty(request.CityId) || string.IsNullOrEmpty(request.UnitTrainingId))
            {
                return BadRequest("Invalid request.");
            }

            // Fetch the city by Id
            if (!ObjectId.TryParse(request.CityId, out var objectId))
            {
                return BadRequest("Invalid CityId format.");
            }

            // Fetch the city by ObjectId
            var city = await _dbContext.Cities.Find(c => c.Id == objectId).FirstOrDefaultAsync();

            if (city == null)
            {
                return NotFound("City not found.");
            }

            // Check if the user is the owner of the city
            var userId = User.Identity.Name; // Assuming the user is authenticated
            if (city.UserId != userId)
            {
                return Unauthorized("You do not have permission to cancel unit training in this city.");
            }


            if (!ObjectId.TryParse(request.UnitTrainingId, out var unitTrainingId))
            {
                return BadRequest("Invalid unitId format.");
            }


            // Fetch the unit training to be canceled
            var unitTraining = city.CityContents.UnitTrainingQueu
                .FirstOrDefault(ut => ut.Id == unitTrainingId);

            if (unitTraining == null)
            {
                return NotFound("Unit training not found.");
            }

            // Cancel the unit training
            await _UnitManagerService.CancelUnitTrainingAsync(city, unitTraining);

            return Ok(new { Message = $"Canceled training of {unitTraining.Quantity} {unitTraining.Id} units in city {city.Name}." });
        }
    }
}