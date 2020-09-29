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
        private const string _redditUrlAfterString = "&after=";
        private readonly HttpClient _httpClient = new HttpClient();

        public RedditHttpsReader(string subreddit)
        {
            _subreddit = subreddit;
        }

        private string GetRecentPostsRequestUrl(int numPosts, string after = null)
        {
            var url = $"{ _redditUrlPrefix}{_subreddit}{_redditUrlPostsRequest}{_redditUrlLimitString}{numPosts}";
            if (!string.IsNullOrEmpty(after))
            {
                url = $"{url}{_redditUrlAfterString}{after}";
            }

            return url;
        }

        private string GetRecentCommentsRequestUrl(int numComments, string after = null)
        {
            var url = $"{ _redditUrlPrefix}{_subreddit}{_redditUrlCommentsRequest}{_redditUrlLimitString}{numComments}";
            if (!string.IsNullOrEmpty(after))
            {
                url = $"{url}{_redditUrlAfterString}{after}";
            }

            return url;
        }

        public IEnumerable<SubredditBot.Data.Post> GetRecentPosts(int numPosts, string after = null)
        {
            var output = new List<SubredditBot.Data.Post>();
            using (var httpResponse = _httpClient.GetAsync(GetRecentPostsRequestUrl(numPosts, after)).Result)
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

        public IEnumerable<SubredditBot.Data.Comment> GetRecentComments(int numComments, string after = null)
        {
            var output = new List<SubredditBot.Data.Comment>();
            using (var httpResponse = _httpClient.GetAsync(GetRecentCommentsRequestUrl(numComments, after)).Result)
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