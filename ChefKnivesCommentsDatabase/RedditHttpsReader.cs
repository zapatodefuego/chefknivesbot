using ChefKnivesCommentsDatabase.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace ChefKnivesCommentsDatabase
{
    public class RedditHttpsReader
    {
        private readonly string subreddit;
        private const string RedditUrlPrefix = "https://reddit.com/r/";
        private const string RedditUrlCommentsRequest = "/comments.json";
        private const string RedditUrlPostsRequest = "/new/.json";
        private const string RedditUrlLimitString = "?limit=";
        private readonly HttpClient httpClient = new HttpClient();

        public RedditHttpsReader(string subreddit)
        {
            this.subreddit = subreddit;
        }

        private string GetRecentPostsRequestUrl(int numPosts)
        {
            return $"{ RedditUrlPrefix}{subreddit}{RedditUrlPostsRequest}{RedditUrlLimitString}{numPosts}";
        }

        private string GetRecentCommentsRequestUrl(int numComments)
        {
            return $"{ RedditUrlPrefix}{subreddit}{RedditUrlCommentsRequest}{RedditUrlLimitString}{numComments}";
        }

        internal IEnumerable<RedditPost> GetRecentPosts(int numPosts)
        {
            List<RedditPost> output = new List<RedditPost>();
            using (var httpResponse = httpClient.GetAsync(GetRecentPostsRequestUrl(numPosts)).Result)
            {
                HttpContent content = httpResponse.Content;
                if (httpResponse.StatusCode != HttpStatusCode.OK || content == null)
                {
                    return output;
                }

                RedditPostQueryResponse parsedContent = JsonConvert.DeserializeObject<RedditPostQueryResponse>(content.ReadAsStringAsync().Result);
                foreach (Post redditComment in parsedContent?.data?.children)
                {
                    RedditPost toAdd = new RedditPost(redditComment.data);
                    output.Add(toAdd);
                }
            }

            return output;
        }

        public IEnumerable<RedditComment> GetRecentComments(int numComments)
        {
            List<RedditComment> output = new List<RedditComment>();
            using (var httpResponse = httpClient.GetAsync(GetRecentCommentsRequestUrl(numComments)).Result)
            {
                HttpContent content = httpResponse.Content;
                if (httpResponse.StatusCode != HttpStatusCode.OK || content == null)
                {
                    return output;
                }

                RedditCommentQueryResponse parsedContent = JsonConvert.DeserializeObject<RedditCommentQueryResponse>(content.ReadAsStringAsync().Result);
                foreach(Comment redditComment in parsedContent?.data?.children)
                {
                    RedditComment toAdd = new RedditComment(redditComment.data);
                    output.Add(toAdd);
                }
            }

            return output;
        }
    }
}