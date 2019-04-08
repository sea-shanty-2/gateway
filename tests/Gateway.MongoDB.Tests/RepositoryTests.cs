using Mongo2Go;
using MongoDB.Driver;
using NUnit.Framework;
using Gateway.MongoDB;
using System;
using Gateway.Models;
using Gateway.MongoDB.Repositories;
using Gateway.Repositories;
using System.Threading.Tasks;
using Bogus;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

namespace Gateway.MongoDB.Tests
{
    [TestFixture]
    public sealed class RepositoryTests : IDisposable
    {
        /// <summary>
        /// Entity class for the testing fixture
        /// </summary>
        private class Person : IEntity
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int Points { get; set; }
        }

        private readonly MongoDbRunner _runner;
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IRepository<Person> _repo;
        private readonly Faker<Person> _persons;
        private readonly string _dbname = "testDatabase";

        public RepositoryTests()
        {

            _runner = MongoDbRunner.StartForDebugging();
            _client = new MongoClient(_runner.ConnectionString);
            _client.DropDatabase(_dbname);
            _database = _client.GetDatabase(_dbname);
            _repo = new Repository<Person>(_database);
            _persons = new Faker<Person>()
                .CustomInstantiator(f =>
                {
                    return new Person
                    {
                        Name = f.Person.FullName,
                        Email = f.Person.Email,
                        Points = f.Random.Int(0, 10000)
                    };
                });
        }

        [SetUp]
        public async Task SetupAsync()
        {
            // Drop the database between each test
            await _client.DropDatabaseAsync(_dbname);
        }

        [Test]
        public async Task AddAsync()
        {
            var entity = await _repo.AddAsync(_persons.Generate());

            // The added entity should be returned
            Assert.IsNotNull(entity);

            // The entity should have been assigned an id
            Assert.IsNotNull(entity.Id);
        }

        [Test]
        public async Task AddRangeAsync([Values(10, 50, 100)] int x)
        {
            var entities = await _repo.AddRangeAsync(_persons.Generate(x));

            // The added entities should be returned, so not empty
            Assert.IsNotEmpty(entities);

            // The number of added entities should correspond to the number of returned entities
            Assert.AreEqual(x, entities.Count());
        }

        [Test]
        public async Task FindAsync()
        {
            // Find the first or default entity in the db
            var entity = await _repo.FindAsync(_ => true);

            // There should be no entities in the db, so should return default val
            Assert.IsNull(entity);

            // Add an entity to the db
            await _repo.AddAsync(_persons.Generate());

            // Attempt to find the first entity in the db
            entity = await _repo.FindAsync(_ => true);

            // The entity should not be null
            Assert.IsNotNull(entity);
        }

        [Test]
        public async Task FindRangeAsync([Values(10, 50, 100)] int x)
        {
            // Find all entities in the db
            var entities = await _repo.FindRangeAsync(_ => true);

            // The enumerable should be empty
            Assert.IsEmpty(entities);

            // Add x entities to the db
            await _repo.AddRangeAsync(_persons.Generate(x));

            // Attmept to find entities in the db
            entities = await _repo.FindRangeAsync(_ => true);

            // The enumerable should not be empty
            Assert.IsNotEmpty(entities);
        }

        [Test]
        public async Task UpdateAsync()
        {
            // Instantiate an entity
            var entity = new Person
            {
                Name = "John Doe",
                Email = "john.doe@example.com"
            };

            // Insert into the database
            entity = await _repo.AddAsync(entity);

            // Save the current entity property value before update
            var oldname = entity.Name;

            // Define a variable to hold the new property value
            var newname = "Jane Doe";

            // Set entity property value to the new value
            entity.Name = newname;

            // Perform the update
            entity = await _repo.UpdateAsync(x => x.Id == entity.Id, entity);

            // Verify the integrity of the entity property update
            Assert.AreEqual(newname, entity.Name);
            Assert.AreNotEqual(oldname, entity.Name);

            // Set the entity property value to null
            entity.Name = null;

            // Perform the update
            entity = await _repo.UpdateAsync(x => x.Id == entity.Id, entity);

            // Verify that the entity property update did not null the value in the repo
            Assert.IsNotNull(entity.Name);
        }

        [Test]
        public async Task UpdateRangeAsync([Values(10, 50, 100)] int x)
        {
            // Add a range of persons to the database with the name "Jane Doe"
            await _repo.AddRangeAsync(_persons.Generate(x).Select(p =>
            {
                p.Name = "Jane Doe";
                return p;
            }));

            // Change the name of the x entities named "Jane Doe" to "John Doe"
            var entities = await _repo.UpdateRangeAsync(p => p.Name == "Jane Doe", new Person { Name = "John Doe" });

            // The updated entities should be returned, so not empty
            Assert.IsNotEmpty(entities);

            // Assert that the list contains x updated entities
            Assert.AreEqual(x, entities.Count());

            // Assert that the name of the entities is "John Doe"
            foreach (var entity in entities)
            {
                Assert.AreEqual("John Doe", entity.Name);
            }
        }

        [TearDown]
        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}