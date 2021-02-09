using Reddit;
using Reddit.Controllers;
using SubredditBot.Data;
using SubredditBot.DataAccess;
using System.Collections.Generic;

namespace SubredditBot.Lib
{
    public interface ISubredditService
    {
        Account Account { get; }
        List<ICommentHandler> CommentHandlers { get; }
        List<IMessageHandler> MessageHandlers { get; }
        List<IPostHandler> PostHandlers { get; }
        RedditClient RedditClient { get; }
        IDatabaseService<SubredditBot.Data.Comment> RedditCommentDatabase { get; }
        IDatabaseService<SubredditBot.Data.Post> RedditPostDatabase { get; }
        IDatabaseService<SelfComment> SelfCommentDatabase { get; }
        Subreddit Subreddit { get; }

        void Dispose();
        bool IsSubredditModerator(string username);
        void PullCommentsAndPosts(string subredditName, int postCount = 100, int commentCount = 100);
        void RegisterRepeatForCommentAndPostDataPull();
        void SubscribeToCommentFeed();
        void SubscribeToMessageFeed();
        void SubscribeToPostFeed();
        void UnsubscribeAllEvents();
    }
}