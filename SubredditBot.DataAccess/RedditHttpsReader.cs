using SubredditBot.Data;
using SubredditBot.DataAccess.DataExtensions;
using SubredditBot.DataAccess.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace ChefKnivesCommentsDatabase
{
    public class RedditHttpsReader
    {
        private readonly string _subreddit;
        private const string _redditUrlPrefix = "https://reddit.com/r/";
        private const string _redditUrlCommentsRequest = "/comments.json";
        private const string _redditUrlPostsRequest = "/new/.json";
        private const string _redditUrlLimitString = "?limit=";
        private readonly HttpClient _httpClient = new HttpClient();

        public RedditHttpsReader(string subreddit)
        {
            _subreddit = subreddit;
        }

        private string GetRecentPostsRequestUrl(int numPosts)
        {
            return $"{ _redditUrlPrefix}{_subreddit}{_redditUrlPostsRequest}{_redditUrlLimitString}{numPosts}";
        }

        private string GetRecentCommentsRequestUrl(int numComments)
        {
            return $"{ _redditUrlPrefix}{_subreddit}{_redditUrlCommentsRequest}{_redditUrlLimitString}{numComments}";
        }

        public IEnumerable<SubredditBot.Data.Post> GetRecentPosts(int numPosts)
        {
            var output = new List<SubredditBot.Data.Post>();
            using (var httpResponse = _httpClient.GetAsync(GetRecentPostsRequestUrl(numPosts)).Result)
            {
                var content = httpResponse.Content;
                if (httpResponse.StatusCode != HttpStatusCode.OK || content == null)
                {
                    return output;
                }

                var parsedContent = JsonConvert.DeserializeObject<RedditPostQueryResponse>(content.ReadAsStringAsync().Result);
                foreach (SubredditBot.DataAccess.Serialization.Post post in parsedContent?.data?.children)
                {
                    output.Add(post.data.ToPost(post.kind));
                }
            }

            return output;
        }

        public IEnumerable<SubredditBot.Data.Comment> GetRecentComments(int numComments)
        {
            var output = new List<SubredditBot.Data.Comment>();
            using (var httpResponse = _httpClient.GetAsync(GetRecentCommentsRequestUrl(numComments)).Result)
            {
                var content = httpResponse.Content;
                if (httpResponse.StatusCode != HttpStatusCode.OK || content == null)
                {
                    return output;
                }

                var parsedContent = JsonConvert.DeserializeObject<RedditCommentQueryResponse>(content.ReadAsStringAsync().Result);
                foreach(var comment in parsedContent?.data?.children)
                {
                    output.Add(comment.data.ToComment(comment.kind));
                }
            }

            return output;
        }
    }
}