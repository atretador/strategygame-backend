using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class ResourceDataController : ControllerBase
    {
        private readonly IResourceManagerService _resourceService;

        public ResourceDataController(IResourceManagerService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpPost("add")]
        public async Task<ActionResult<ResourceDto>> Add([FromBody] ResourceDto resourceDto)
        {
            if (resourceDto == null)
                return BadRequest("Resource cannot be null.");

            var newResource = new Resource { Name = resourceDto.Name };

            var addedResource = await _resourceService.AddResourceAsync(newResource);
            var addedResourceDto = new ResourceDto { Id = addedResource.Id.ToString(), Name = addedResource.Name };

            return CreatedAtAction(nameof(GetResourceById), new { id = addedResource.Id }, addedResource);
        }

        [HttpPut("update")]
        public async Task<ActionResult<ResourceDto>> UpdateResource([FromBody] ResourceDto resourceDto)
        {
            if (!ObjectId.TryParse(resourceDto.Id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }

            var resource = new Resource { Id = objectId, Name = resourceDto.Name };

            var updatedResource = await _resourceService.UpdateResourceAsync(objectId, resource);
            if (updatedResource == null)
                return NotFound($"Resource with ID {resourceDto.Id} not found.");
            
            var updatedResourceDto = new ResourceDto { Id = updatedResource.Id.ToString(), Name = updatedResource.Name};

            return Ok(updatedResourceDto);
        }

        [HttpDelete("remove/{id}")]
        public async Task<ActionResult> RemoveResource(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }

            await _resourceService.RemoveResourceAsync(objectId);
            return Ok("Resource has been removed.");
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult<ResourceDto>> GetResourceById(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid id format.");
            }
            
            var resource = await _resourceService.GetResourceByIdAsync(objectId);
            if (resource == null)
                return NotFound($"Resource with ID {id} not found.");
            
            var resourceDto = new ResourceDto { Id = resource.Id.ToString(), Name = resource.Name };
            return Ok(resourceDto);
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<ResourceDto>>> GetAllResources()
        {          
            var resources = await _resourceService.GetAllResourcesAsync();
            var resourceDtos = resources.Select(resource => new ResourceDto { Id = resource.Id.ToString(), Name = resource.Name }).ToList();
            return Ok(resourceDtos);
        }
    }
}