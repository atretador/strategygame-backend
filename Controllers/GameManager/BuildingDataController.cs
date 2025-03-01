using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StrategyGame.Models;
using StrategyGame.Services;

namespace StrategyGame.Controllers.GameManager
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class BuildingDataController : ControllerBase
    {
        private readonly IBuildingManagerService _buildingService;

        public BuildingDataController(IBuildingManagerService buildingService)
        {
            _buildingService = buildingService;
        }

        // Add Building
        [HttpPost("addBuilding")]
        public async Task<ActionResult<Building>> AddBuilding([FromBody] Building building)
        {
            if (building == null)
                return BadRequest("Building cannot be null.");

            var addedBuilding = await _buildingService.AddBuildingAsync(building);
            return CreatedAtAction(nameof(GetBuildingById), new { id = addedBuilding.Id }, addedBuilding);
        }


        // Get Building by Id
        [HttpGet("getBuilding/{id}")]
        public async Task<ActionResult<Building>> GetBuildingById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }
            
            var building = await _buildingService.GetBuildingByIdAsync(objectId);
            if (building == null)
                return NotFound($"Building with ID {id} not found.");
            return Ok(building);
        }


        // Update Building
        [HttpPut("updateBuilding/{id}")]
        public async Task<ActionResult<Building>> UpdateBuilding(string id, [FromBody] Building building)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }
            var updatedBuilding = await _buildingService.UpdateBuildingAsync(objectId, building);
            if (updatedBuilding == null)
                return NotFound($"Building with ID {id} not found.");
            return Ok(updatedBuilding);
        }


        // Delete Building
        [HttpDelete("deleteBuilding/{id}")]
        public async Task<ActionResult> DeleteBuilding(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }
            var success = await _buildingService.DeleteBuildingAsync(objectId);
            if (!success)
                return NotFound($"Building with ID {id} not found.");
            return NoContent();
        }

        // Get All Buildings
        [HttpGet("getAllBuildings")]
        public async Task<ActionResult<IEnumerable<Building>>> GetAllBuildings()
        {
            var buildings = await _buildingService.GetAllBuildingsAsync();
            return Ok(buildings);
        }
    }
}