using AutoMapper;
using MongoDB.Bson;
using StrategyGame.Dto;
using StrategyGame.Models;
using System.Collections.Generic;

public class WorldSettingsProfile : Profile
{
    public WorldSettingsProfile()
    {
        CreateMap<WorldSettings, WorldSettingsDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString())); // Map ObjectId to string

        CreateMap<WorldSettingsDto, WorldSettings>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => ParseObjectId(src.Id))); // Convert string to ObjectId with validation
    }

    private List<string> ConvertObjectIdsToStrings(List<ObjectId> objectIds)
    {
        if (objectIds == null)
        {
            return new List<string>();
        }
        return objectIds.ConvertAll(id => id.ToString());
    }

    private List<ObjectId> ConvertStringsToObjectIds(List<string> strings)
    {
        if (strings == null)
        {
            return new List<ObjectId>();
        }
        return strings.ConvertAll(id => new ObjectId(id));
    }

    // Validate and parse the ObjectId from a string, return a default ObjectId if invalid
    private ObjectId ParseObjectId(string id)
    {
        if (ObjectId.TryParse(id, out var objectId))
        {
            return objectId;
        }
        // Return a default ObjectId (empty) or handle the error appropriately
        return ObjectId.Empty;
    }
}