# Disclaimer
This is a work in project, that may or may not be completed.

# Strategy Game Backend

Backend for a strategy game providing similar features to games like Tribal Wars/Travian such as city management and conquest. The backend is built with .NET Core and uses MongoDB for data storage.

## Technologies Used
- **Framework**: .NET Core
- **Database**: MongoDB
- **Authentication**: OAuth 2.0 with IdentityFramework

## Features

### Authentication and Authorization
- OAuth 2.0 implementation using JWT for secure user authentication.

### Player spawning
- Players can choose a direction where to spawn in the world

#### Endpoints
- `GET /api/city/check` - Check if a user has a city.
- `POST /api/city/create` - Create a new city for the user.

### Building Construction
- Start, cancel, and destroy building constructions.
- Automatically handle construction queuing and resource validation.

#### Endpoints
- `POST /api/buildingconstruction/start` - Start building construction.
- `POST /api/buildingconstruction/cancel` - Cancel ongoing building construction.
- `POST /api/buildingconstruction/destroy` - Destroy an existing building.

### Unit Training
- Manages unit training queue.

#### Endpoints
- `POST /api/unittraining/train` - Start training units.
- `POST /api/unittraining/cancel` - Cancel ongoing unit training.
- `POST /api/unittraining/kill` - Deletes selected units


### Background Services
- Periodic adds resources to cities based on building levels.
- Handle construction and unit training completions.

## Getting Started

### Prerequisites
- .NET Core SDK
- MongoDB instance

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/strategy-game-backend.git
   ```
2. Navigate to the project directory:
   ```bash
   cd StrategyGame
   ```
3. Install dependencies:
   ```bash
   dotnet restore
   ```
4. Update the `appsettings.json` file with your MongoDB connection string and JWT configuration.

### Running the Application
1. Start MongoDB.
2. Set up the application:
   ```bash
   dotnet run --Setup=true --AdminPassword="YourPasswordHere"
   ```
3. The API will be available at `https://localhost:5001`.
4. Utilize the GameManager Endpoints to customize your game

## License
This project is licensed under the MIT License.
