using ChefKnivesBot.Data;
using ChefKnivesCommentsDatabase.Utility;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace ChefKnivesCommentsDatabase
{
    public class RedditContentService : IDisposable
    {
        private readonly string _subreddit;
        private readonly MongoClient _mongoClient;
        private const string _commentsCollectionName = "comments";
        private const string _postsCollectionName = "posts";
        private readonly DatabaseCache<RedditComment> _commentCache = new DatabaseCache<RedditComment>(10000);
        private readonly DatabaseCache<RedditPost> _postCache = new DatabaseCache<RedditPost>(1000);

        public RedditContentService(IConfiguration configuration, string subreddit)
        {
            _mongoClient = new MongoClient(configuration["ConnectionString"]);
            _subreddit = subreddit;
            if (!_mongoClient.GetDatabase(subreddit).ListCollections(new ListCollectionsOptions { Filter = new BsonDocument("name", _commentsCollectionName) }).Any())
            {
                GetMongoCommentCollection().InsertOne(new BsonDocument()); // completely empty, but ensures the database and collection is set up the first time
            }
        }

        /// <summary>
        /// Ensures a post is in is in the database
        /// </summary>
        /// <param name="post"></param>
        public void InsertPost(RedditPost post)
        {
            InsertPosts(new List<RedditPost> { post });
        }

        /// <summary>
        /// Ensures each post in the collection is in the database
        /// </summary>
        /// <param name="posts"></param>
        public void InsertPosts(IEnumerable<RedditPost> posts)
        {
            foreach (RedditPost post in posts)
            {
                if (_postCache.Contains(post))
                {
                    continue;
                }

                _postCache.Add(post);
                UpsertIntoCollection(post);
                Console.WriteLine($"Wrote post id {post.Id} by {post.Author} to database");
            }
        }

        /// <summary>
        /// Ensures a commnet is in is in the database
        /// </summary>
        /// <param name="post"></param>
        public void InsertComment(RedditComment comment)
        {
            InsertComments(new List<RedditComment> { comment });
        }

        /// <summary>
        /// Ensures each comment in the collection is in the database
        /// </summary>
        public void InsertComments(IEnumerable<RedditComment> comments)
        {
            foreach (RedditComment comment in comments)
            {
                if (_commentCache.Contains(comment))
                {
                    continue;
                }

                _commentCache.Add(comment);
                UpsertIntoCollection(comment);
                Console.WriteLine($"Wrote comment id {comment.Id} by {comment.Author} to database");
            }
        }

        public RedditComment GetComment(uint id)
        {
            throw new NotImplementedException();
        }

        public RedditComment GetCommentsByAuthor(string author)
        {
            throw new NotImplementedException();
        }

        public RedditComment GetPost(uint id)
        {
            throw new NotImplementedException();
        }

        public RedditComment GetPostsByAuthor(string author)
        {
            throw new NotImplementedException();
        }

        public void DeleteComment(uint id)
        {

        }

        public void DeletePost(uint id)
        {

        }

        private IMongoCollection<BsonDocument> GetMongoCommentCollection()
        {
            return _mongoClient.GetDatabase(_subreddit).GetCollection<BsonDocument>(_commentsCollectionName);
        }

        private IMongoCollection<BsonDocument> GetMongoPostCollection()
        {
            return _mongoClient.GetDatabase(_subreddit).GetCollection<BsonDocument>(_postsCollectionName);
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
            _commentCache.Dispose();
            _postCache.Dispose();
        }
    }
}