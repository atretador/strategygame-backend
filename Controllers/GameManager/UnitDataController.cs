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
    public class UnitDataController : ControllerBase
    {
        private readonly IMilitaryUnitManagerService _militaryUnitService;

        public UnitDataController(IMilitaryUnitManagerService militaryUnitService)
        {
            _militaryUnitService = militaryUnitService;
        }

        [HttpPost("addMilitaryUnit")]
        public async Task<ActionResult<MilitaryUnit>> AddMilitaryUnit([FromBody] MilitaryUnit unit)
        {
            if (unit == null)
                return BadRequest("Military unit cannot be null.");

            var addedUnit = await _militaryUnitService.AddMilitaryUnitAsync(unit);
            return CreatedAtAction(nameof(GetMilitaryUnitById), new { id = addedUnit.Id }, addedUnit);
        }
        
        // Get Military Unit by Id
        [HttpGet("getMilitaryUnit/{id}")]
        public async Task<ActionResult<MilitaryUnit>> GetMilitaryUnitById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }
            var unit = await _militaryUnitService.GetMilitaryUnitByIdAsync(objectId);
            if (unit == null)
                return NotFound($"Military unit with ID {id} not found.");
            return Ok(unit);
        }

        // Update Military Unit
        [HttpPut("updateMilitaryUnit/{id}")]
        public async Task<ActionResult<MilitaryUnit>> UpdateMilitaryUnit(string id, [FromBody] MilitaryUnit unit)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }

            var updatedUnit = await _militaryUnitService.UpdateMilitaryUnitAsync(objectId, unit);
            if (updatedUnit == null)
                return NotFound($"Military unit with ID {id} not found.");
            return Ok(updatedUnit);
        }

        // Delete Military Unit
        [HttpDelete("deleteMilitaryUnit/{id}")]
        public async Task<ActionResult> DeleteMilitaryUnit(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }

            var success = await _militaryUnitService.DeleteMilitaryUnitAsync(objectId);
            if (!success)
                return NotFound($"Military unit with ID {id} not found.");
            return NoContent();
        }
        
        // Get All Military Units
        [HttpGet("getAllMilitaryUnits")]
        public async Task<ActionResult<IEnumerable<MilitaryUnit>>> GetAllMilitaryUnits()
        {
            var units = await _militaryUnitService.GetAllMilitaryUnitsAsync();
            return Ok(units);
        }
    }
}