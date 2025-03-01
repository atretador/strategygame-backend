using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StrategyGame.Enums;

namespace StrategyGame.Dto
{
    public class SectorDto
    {
        public string Id { get; set; } = string.Empty;
        public ClimateType Climate { get; set; }
        public List<DecorationTileDto> DecorationTiles { get; set; } = new List<DecorationTileDto>();
        public List<CityTileDto> CityTiles { get; set; } = new List<CityTileDto>();
    }

    public class TileDto
    {
        public int X { get; set; } // Global X coordinate
        public int Y { get; set; } // Global Y coordinate
    }

    public class DecorationTileDto
    {
        public DecorationType Decoration { get; set; } = DecorationType.None; // Defaults to no decoration
    }

    public class CityTileDto
    {
        public string CityId { get; set; } // City occupying the tile
        public string CityName { get; set; }
        public int points { get; set; }
    }
}