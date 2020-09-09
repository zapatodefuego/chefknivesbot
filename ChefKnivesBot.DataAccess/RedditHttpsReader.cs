using ChefKnivesBot.Data;
using ChefKnivesBot.DataAccess.DataExtensions;
using ChefKnivesBot.DataAccess.Serialization;
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

        internal IEnumerable<RedditPost> GetRecentPosts(int numPosts)
        {
            var output = new List<RedditPost>();
            using (var httpResponse = _httpClient.GetAsync(GetRecentPostsRequestUrl(numPosts)).Result)
            {
                var content = httpResponse.Content;
                if (httpResponse.StatusCode != HttpStatusCode.OK || content == null)
                {
                    return output;
                }

                var parsedContent = JsonConvert.DeserializeObject<RedditPostQueryResponse>(content.ReadAsStringAsync().Result);
                foreach (Post post in parsedContent?.data?.children)
                {
                    output.Add(post.data.ToRedditPost());
                }
            }

            return output;
        }

        public IEnumerable<RedditComment> GetRecentComments(int numComments)
        {
            var output = new List<RedditComment>();
            using (var httpResponse = _httpClient.GetAsync(GetRecentCommentsRequestUrl(numComments)).Result)
            {
                var content = httpResponse.Content;
                if (httpResponse.StatusCode != HttpStatusCode.OK || content == null)
                {
                    return output;
                }

                var parsedContent = JsonConvert.DeserializeObject<RedditCommentQueryResponse>(content.ReadAsStringAsync().Result);
                foreach(var redditComment in parsedContent?.data?.children)
                {
                    output.Add(redditComment.data.ToRedditComment());
                }
            }

            return output;
        }
    }
}