using SubredditBot.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubredditBot.DataAccess
{
    public interface IDatabaseService<T> where T : RedditThing
    {
        Task<bool> ContainsAny(string propertyName, string propertyValue);
        void Delete(string id);
        void Dispose();
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetBy(string propertyName, string propertyValue);
        T GetById(string id);
        void Upsert(IEnumerable<T> things);
        void Upsert(T thing);
    }
}