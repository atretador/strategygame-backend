using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using StrategyGame.Enums;

namespace StrategyGame.Models
{
    public class Tile
    {
        public int X { get; set; } // Global X coordinate
        public int Y { get; set; } // Global Y coordinate
        public DecorationType Decoration { get; set; } = DecorationType.None; // Defaults to no decoration
        public ObjectId CityId { get; set; } // City occupying the tile
        public bool isOccupied => Decoration != DecorationType.None || ObjectId.TryParse(CityId.ToString(), out _);

        public Tile(int X, int Y, DecorationType Decoration)
        {
            this.X = X;
            this.Y = Y;
            this.Decoration = Decoration;
        }
        public Tile(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            this.Decoration = DecorationType.None;
        }
    }
}