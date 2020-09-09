using ChefKnivesCommentsDatabase.Utility;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace ChefKnivesCommentsDatabase
{
    public class RedditContentDatabase : IDisposable
    {
        private readonly string subreddit;
        private readonly MongoClient mongoClient;
        private const string commentsCollectionName = "comments";
        private const string postsCollectionName = "posts";
        private readonly DatabaseCache<RedditComment> commentCache = new DatabaseCache<RedditComment>(10000);
        private readonly DatabaseCache<RedditPost> postCache = new DatabaseCache<RedditPost>(1000);

        public RedditContentDatabase(IConfiguration configuration, string subreddit)
        {
            this.mongoClient = new MongoClient(configuration["ConnectionString"]);
            this.subreddit = subreddit;
            if (!mongoClient.GetDatabase(subreddit).ListCollections(new ListCollectionsOptions { Filter = new BsonDocument("name", commentsCollectionName) }).Any())
            {
                GetMongoCommentCollection().InsertOne(new BsonDocument()); // completely empty, but ensures the database and collection is set up the first time
            }
        }

        public void EnsurePostsInDatabase(IEnumerable<RedditPost> posts)
        {
            foreach (RedditPost post in posts)
            {
                if (postCache.Contains(post))
                {
                    continue;
                }

                postCache.Add(post);
                UpsertIntoCollection(post);
                Console.WriteLine($"Wrote post id {post.Id} by {post.Author} to database");
            }
        }

        /// <summary>
        /// Ensures each comment in the collection is in the database
        /// </summary>
        public void EnsureCommentsInDatabase(IEnumerable<RedditComment> comments)
        {
            foreach (RedditComment comment in comments)
            {
                if (commentCache.Contains(comment))
                {
                    continue;
                }

                commentCache.Add(comment);
                UpsertIntoCollection(comment);
                Console.WriteLine($"Wrote comment id {comment.Id} by {comment.Author} to database");
            }
        }

        private IMongoCollection<BsonDocument> GetMongoCommentCollection()
        {
            return mongoClient.GetDatabase(subreddit).GetCollection<BsonDocument>(commentsCollectionName);
        }

        private IMongoCollection<BsonDocument> GetMongoPostCollection()
        {
            return mongoClient.GetDatabase(subreddit).GetCollection<BsonDocument>(postsCollectionName);
        }

        protected virtual void UpsertIntoCollection(RedditComment comment)
        {
            var collection = GetMongoCommentCollection();
            collection.ReplaceOne(
                filter: new BsonDocument("_id", comment.Id),
                options: new ReplaceOptions { IsUpsert = true },
                replacement: comment.ToBsonDocument());
        }

        protected virtual void UpsertIntoCollection(RedditPost post)
        {
            var collection = GetMongoPostCollection();
            collection.ReplaceOne(
                filter: new BsonDocument("_id", post.Id),
                options: new ReplaceOptions { IsUpsert = true },
                replacement: post.ToBsonDocument());
        }

        public void Dispose()
        {
            commentCache.Dispose();
            postCache.Dispose();
        }
    }
}