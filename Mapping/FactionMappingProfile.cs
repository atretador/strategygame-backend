using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MongoDB.Bson;
using StrategyGame.Dto;
using StrategyGame.Models;
using ZstdSharp.Unsafe;

namespace StrategyGame.Mapping
{
    public class FactionMappingProfile : Profile
    {
        public FactionMappingProfile()
        {
            // Mapping from FactionDto to Faction
            CreateMap<FactionDto, Faction>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.Id) ? (ObjectId?)null : new ObjectId(src.Id))) // Convert empty string to null, otherwise parse to ObjectId
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Mapping from Faction to FactionDto
            CreateMap<Faction, FactionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString())) // Convert ObjectId to string, allowing null
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}