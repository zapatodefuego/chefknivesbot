namespace SubredditBot.Lib
{
    public interface IMessageHandler
    {
        bool Process(object thing);
    }
}
