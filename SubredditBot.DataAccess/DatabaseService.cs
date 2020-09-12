using SubredditBot.Data;
using SubredditBot.DataAccess.Utility;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChefKnivesCommentsDatabase
{
    public class DatabaseService<T> : IDisposable where T : RedditThing
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
        /// <param name="post"></param>
        public void Upsert(T thing)
        {
            Upsert(new List<T> { thing });
        }

        /// <summary>
        /// Ensures each post in the collection is in the database
        /// </summary>
        /// <param name="posts"></param>
        public void Upsert(IEnumerable<T> things)
        {
            foreach (T thing in things)
            {
                if (_cache.Contains(thing))
                {
                    continue;
                }

                _cache.Add(thing);
                UpsertIntoCollection(thing);
            }
        }

        public async Task<IEnumerable<T>> GetBy(string propertyName, string propertyValue)
        {
            if (propertyName.Equals(nameof(RedditThing.Id)))
            {
                throw new InvalidOperationException($"Use {nameof(GetById)} to query by Id");
            }

            var filter = Builders<BsonDocument>.Filter.Eq(propertyName, propertyValue);
            return await GetByFilter(filter);
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
                return BsonSerializer.Deserialize<T>(queryResult);
            }

            return null;
        }

        public async Task<IEnumerable<T>> GetByAuthor(string author)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Author", author);
            return await GetByFilter(filter);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await GetByFilter(Builders<BsonDocument>.Filter.Empty);
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