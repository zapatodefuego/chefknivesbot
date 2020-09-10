using ChefKnivesBot.Data;
using ChefKnivesCommentsDatabase.Utility;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace ChefKnivesCommentsDatabase
{
    public class DatabaseService<T> : IDisposable where T : RedditThing
    {
        private readonly string _databaseName;
        private readonly string _collectionName;
        private readonly MongoClient _mongoClient;
        private readonly DatabaseCache<RedditThing> _cache = new DatabaseCache<RedditThing>(10000);

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
        public void Insert(RedditThing thing)
        {
            Insert(new List<RedditThing> { thing });
        }

        /// <summary>
        /// Ensures each post in the collection is in the database
        /// </summary>
        /// <param name="posts"></param>
        public void Insert(IEnumerable<RedditThing> things)
        {
            foreach (RedditThing thing in things)
            {
                if (_cache.Contains(thing))
                {
                    continue;
                }

                _cache.Add(thing);
                UpsertIntoCollection(thing);
            }
        }

        public RedditComment Get(string id)
        {
            throw new NotImplementedException();
        }

        public RedditComment GetByAuthor(string author)
        {
            throw new NotImplementedException();
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