using StrategyGame.Enums;

namespace StrategyGame.Models
{
    // Represents a map object (e.g., player, city, NPC) in the map
    // we return this object to the client to render the map and display basic information
    public class MapObject
    {
        public string UserId { get; set; } // Identity id so we can fetch more user related data if required
        public string? Name { get; set; } // Optional name for the object
        public StanceType Stance { get; set; } // Relationship with other players (Friend, Ally, Neutral, Enemy)
        public string CityId { get; set; } // Reference to the City object, so we can fetch detailed data if required
        public string MapAppearance { get; set; } // Icon to display on the map
        public int Points { get; set; } // Points that influence the city’s appearance (e.g., size, importance)
        public int X { get; set; } // X coordinate on the map
        public int Y { get; set; } // Y coordinate on the map
    }
}