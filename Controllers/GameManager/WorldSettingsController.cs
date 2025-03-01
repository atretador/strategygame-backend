using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using StrategyGame.Dto;
using StrategyGame.Models;
using StrategyGame.Services;

namespace StrategyGame.Controllers.GameManager
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class WorldSettingsController : ControllerBase
    {
        private readonly IWorldManagerService _worldService;
        private readonly IMapper _mapper;
        public WorldSettingsController(IWorldManagerService worldService, IMapper mapper)
        {
            _worldService = worldService;
            _mapper = mapper;
        }
        // Add or Update World Settings
        [HttpPost("add")]
        public async Task<IActionResult> AddWorldSettings([FromBody] WorldSettingsDto worldSettingsDto)
        {
            if (worldSettingsDto == null || string.IsNullOrEmpty(worldSettingsDto.Name))
                return BadRequest("Invalid world settings.");

            var newWorldSettings = _mapper.Map<WorldSettings>(worldSettingsDto);

            await _worldService.AddWorldSettingsAsync(newWorldSettings);
            return Ok($"World settings for '{worldSettingsDto.Name}' have been added or updated.");
        }

        // Update World Settings
        [HttpPut("update/{Id}")]
        public async Task<ActionResult> UpdateWorldSettings(string Id, [FromBody] WorldSettingsDto worldSettingsDto)
        {
            if (worldSettingsDto == null || string.IsNullOrEmpty(worldSettingsDto.Name))
                return BadRequest("Invalid world settings.");

            if (!ObjectId.TryParse(Id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }
            var worldSettings = _mapper.Map<WorldSettings>(worldSettingsDto);
            
            await _worldService.UpdateWorldSettingsAsync(objectId, worldSettings);
            return Ok($"World settings for '{worldSettings.Name}' have been updated.");
        }

        // Remove World Settings
        [HttpDelete("remove/{Id}")]
        public async Task<ActionResult> RemoveWorldSettings(string Id)
        {
            if (!ObjectId.TryParse(Id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }

            await _worldService.RemoveWorldSettingsAsync(objectId);
            return Ok("World settings have been removed.");
        }

        // Get World Settings by Id
        [HttpGet("get/{Id}")]
        public async Task<ActionResult<WorldSettingsDto>> GetWorldSettings(string Id)
        {
            try
            {
                if(!ObjectId.TryParse(Id, out var worldId))
                {
                    return BadRequest("Invalid id format.");
                }

                var settings = await _worldService.GetWorldSettingsAsync(worldId);
                var settingsDto = _mapper.Map<WorldSettingsDto>(settings);
                return Ok(settingsDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Get All World Settings
        [HttpGet("getAll")]
        public async Task<ActionResult<List<SimpleWorldSettingsDto>>> GetAllWorldSettings()
        {
            var settings = await _worldService.GetAllWorldSettingsAsync();
            var simpleSettings = settings.Select( c => new SimpleWorldSettingsDto {
                Id = c.Id.ToString(),
                Name = c.Name,
                Description = c.Description} )
                .ToList();

            return Ok(simpleSettings);
        }
    }
}