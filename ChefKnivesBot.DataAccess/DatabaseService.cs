using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess.Utility;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (!_mongoClient.GetDatabase(databaseName).ListCollections(new ListCollectionsOptions { Filter = new BsonDocument("name", collectionName) }).Any())
            {
                GetMongoCollection().InsertOne(new BsonDocument()); // completely empty, but ensures the database and collection is set up the first time
            }
        }

        /// <summary>
        /// Ensures a post is in is in the database
        /// </summary>
        /// <param name="post"></param>
        public void Insert(T thing)
        {
            Insert(new List<T> { thing });
        }

        /// <summary>
        /// Ensures each post in the collection is in the database
        /// </summary>
        /// <param name="posts"></param>
        public void Insert(IEnumerable<T> things)
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

        public T Get(string id)
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

        public IEnumerable<T> GetByAuthor(string author)
        {
            var collection = GetMongoCollection();
            var filter = Builders<BsonDocument>.Filter.Eq("Author", author);
            var queryResults = collection.Find(filter).ToList();
            
            return queryResults.Select(r => BsonSerializer.Deserialize<T>(r));
        }

        public void Delete(string id)
        {

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