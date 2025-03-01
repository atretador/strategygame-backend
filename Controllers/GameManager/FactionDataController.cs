using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class FactionDataController : ControllerBase
    {
        private readonly IFactionManagerService _factionService;
        private readonly IMapper _mapper;

        public FactionDataController(IFactionManagerService factionService, IMapper mapper)
        {
            _factionService = factionService;
            _mapper = mapper;
        }

        [HttpPost("add")]
        public async Task<ActionResult<FactionDto>> AddFaction([FromBody] FactionDto factionDto)
        {
            if (factionDto == null)
                return BadRequest("Faction cannot be null.");

            var newFaction = _mapper.Map<Faction>(factionDto);
            var addedFaction = await _factionService.AddFactionAsync(newFaction);
            var addedFactionDto = _mapper.Map<FactionDto>(addedFaction);
            return CreatedAtAction(nameof(GetFactionById), new { id = addedFactionDto.Id.ToString() }, addedFactionDto);
        }

        [HttpPut("update")]
        public async Task<ActionResult<FactionDto>> UpdateFaction([FromBody] FactionDto factionDto)
        {
            if (!ObjectId.TryParse(factionDto.Id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }

            var faction = _mapper.Map<Faction>(factionDto);

            var updatedFaction = await _factionService.UpdateFactionAsync(objectId, faction);
            if (updatedFaction == null)
                return NotFound($"Faction with ID {factionDto.Id} not found.");
            
            var updatedFactionDto = _mapper.Map<FactionDto>(updatedFaction);

            return Ok(updatedFactionDto);
        }

        [HttpDelete("remove/{id}")]
        public async Task<ActionResult> RemoveFaction(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }

            await _factionService.RemoveFactionAsync(objectId);
            return Ok("Faction has been removed.");
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<FactionDto>> GetFactionById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }
            
            var faction = await _factionService.GetFactionByIdAsync(objectId);
            if (faction == null)
                return NotFound($"Faction with ID {id} not found.");
            
            var factionDto = _mapper.Map<FactionDto>(faction);
            return Ok(factionDto);
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<FactionDto>>> GetAllFactions()
        {          
            var factions = await _factionService.GetAllFactionsAsync();
            var factionDtos = factions.Select(faction => _mapper.Map<FactionDto>(faction)).ToList();
            return Ok(factionDtos);
        }
    }
}