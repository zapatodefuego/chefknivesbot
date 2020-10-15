using SubredditBot.Data;
using SubredditBot.DataAccess.Utility;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubredditBot.DataAccess
{
    public class DatabaseService<T> : IDisposable, IDatabaseService<T> where T : RedditThing
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly MongoClient _mongoClient;
        private readonly DatabaseCache<T> _cache = new DatabaseCache<T>(10000);

        public DatabaseService(string connectionString, string databaseName, string collectionName)
        {
            _mongoClient = new MongoClient(connectionString);
            _databaseName = databaseName;
            _collectionName = collectionName;

            BsonTypeMapInitializer.RunOnce();

            // ensures the database and collection is set up the first time
            if (!_mongoClient.GetDatabase(databaseName).ListCollections(new ListCollectionsOptions { Filter = new BsonDocument("name", collectionName) }).Any())
            {
                var defaultObject = (T)Activator.CreateInstance(typeof(T));
                defaultObject.Id = Guid.NewGuid().ToString();
                GetMongoCollection().InsertOne(defaultObject.ToBsonDocument());
            }
        }

        /// <summary>
        /// Ensures a post is in is in the database
        /// </summary>
        /// <returns>The original value, if any</returns>
        public T Upsert(T thing)
        {
            return Upsert(new List<T> { thing }).SingleOrDefault();
        }

        /// <summary>
        /// Ensures each post in the collection is in the database
        /// </summary>
        /// <returns>The original values, if any</returns>
        public IEnumerable<T> Upsert(IEnumerable<T> things)
        {
            var updatedThings = new List<T>();
            foreach (T thing in things)
            {
                // If an object in the cache matches fully, do nothing and return
                if (_cache.Contains(thing, new CacheAllPropertiesComparer<T>()))
                {
                    return updatedThings;
                }

                // If an object in the cache matches by the default comparer, 
                // then this upsert is updating something so remove it
                // and capture the updated object
                if (_cache.Contains(thing))
                {
                    updatedThings.Add(GetById(thing.Id));
                    _cache.Remove(thing);
                }

                // Add and upsert the item 
                _cache.Add(thing);
                UpsertIntoCollection(thing);
            }

            return updatedThings;
        }

        /// <summary>
        /// Tries to get any item by item first from the cache and then from the database
        /// </summary>
        public async Task<T> GetAny(string propertyName, string propertyValue)
        {
            var cacheResult = _cache.GetBy(propertyName, propertyValue);
            if (!cacheResult.Any())
            {
                await GetByFilter(propertyName, propertyValue);
            }

            return null;
        }

        public T GetById(string id)
        {
            if (_cache.GetById(id, out T cacheResult))
            {
                return cacheResult;
            }

            var collection = GetMongoCollection();
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var queryResult = collection.Find(filter).FirstOrDefault();

            if (queryResult != null)
            {
                var results = BsonSerializer.Deserialize<T>(queryResult);
                _cache.Add(results);
                return results;
            }

            return null;
        }

        public async Task<IEnumerable<T>> GetByFilter(string propertyName, string propertyValue)
        {
            if (propertyName.Equals(nameof(RedditThing.Id)))
            {
                throw new InvalidOperationException($"Use {nameof(GetById)} to query by Id");
            }

            var filter = Builders<BsonDocument>.Filter.Eq(propertyName, propertyValue);
            var results = await GetByFilter(filter);
            foreach (var result in results)
            {
                if (!_cache.Contains(result))
                {
                    _cache.Add(result);
                }
            }

            return results;
        }

        public async Task<IEnumerable<T>> GetByQueryable(string propertyName, string propertyValue)
        {
            if (propertyName.Equals(nameof(RedditThing.Id)))
            {
                throw new InvalidOperationException($"Use {nameof(GetById)} to query by Id");
            }

            var bsonResults = GetMongoCollection().AsQueryable();
            var results = new List<T>();
            foreach (var bsonResult in bsonResults)
            {
                if (bsonResult.TryGetValue(propertyName, out BsonValue bsonValue) &&
                    bsonValue.IsString &&
                    bsonValue.AsString.ToLower().Equals(propertyValue.ToLower()))
                {
                    results.Add(BsonSerializer.Deserialize<T>(bsonResult));
                }
            }

            foreach (var result in results)
            {
                if (!_cache.Contains(result))
                {
                    _cache.Add(result);
                }
            }

            return results;
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            var results = await GetByFilter(Builders<BsonDocument>.Filter.Empty);
            foreach (var result in results)
            {
                if (!_cache.Contains(result))
                {
                    _cache.Add(result);
                }
            }

            return results;
        }

        public void Delete(string id)
        {

        }

        private async Task<IEnumerable<T>> GetByFilter(FilterDefinition<BsonDocument> filter)
        {
            var collection = GetMongoCollection();
            var queryResults = await collection.Find(filter).ToListAsync();
            return queryResults.Select(r => BsonSerializer.Deserialize<T>(r));
        }

        private IMongoCollection<BsonDocument> GetMongoCollection()
        {
            return _mongoClient.GetDatabase(_databaseName).GetCollection<BsonDocument>(_collectionName);
        }

        protected virtual void UpsertIntoCollection(RedditThing thing)
        {
            var collection = GetMongoCollection();
            collection.ReplaceOne(
                filter: new BsonDocument("_id", thing.Id),
                options: new ReplaceOptions { IsUpsert = true },
                replacement: thing.ToBsonDocument());
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}