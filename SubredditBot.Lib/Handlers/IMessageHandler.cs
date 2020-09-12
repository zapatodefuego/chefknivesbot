namespace SubredditBot.Lib.Handlers
{
    public interface IMessageHandler
    {
        bool Process(object thing);
    }
}
