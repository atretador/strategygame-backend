using System;
using MongoDB.Driver;
using StrategyGame.Models;

namespace StrategyGame.Context
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;
        private readonly string _databaseName;

        public MongoDbContext(IMongoClient client, string databaseName)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));

            try
            {
                // Attempt to list databases as a way to check if the connection is valid
                _client.ListDatabases(); // This will throw an exception if the connection fails.
                _database = _client.GetDatabase(_databaseName);
            }
            catch (Exception ex)
            {
                // Log the exception or rethrow a more specific exception
                throw new InvalidOperationException($"Unable to connect to the MongoDB server. Ensure that the server is running and the connection string is correct.", ex);
            }
        }

        public IMongoCollection<City> Cities =>
            _database.GetCollection<City>("Cities");

        public IMongoCollection<Building> Buildings =>
            _database.GetCollection<Building>("Buildings");

        public IMongoCollection<Resource> Resources =>
            _database.GetCollection<Resource>("Resources");

        public IMongoCollection<Research> Researches =>
            _database.GetCollection<Research>("Researches");

        public IMongoCollection<Faction> Factions =>
            _database.GetCollection<Faction>("Factions");

        public IMongoCollection<MilitaryUnit> MilitaryUnits =>
            _database.GetCollection<MilitaryUnit>("MilitaryUnits");
        public IMongoCollection<WorldSettings> WorldSettings =>
            _database.GetCollection<WorldSettings>("WorldSettings");

        public IMongoCollection<AttackForce> AttackForces =>
            _database.GetCollection<AttackForce>("AttackForces");

        public IMongoCollection<World> Worlds =>
            _database.GetCollection<World>("Worlds");

        public IMongoCollection<Sector> Sectors =>
            _database.GetCollection<Sector>("Sectors");
    }
}