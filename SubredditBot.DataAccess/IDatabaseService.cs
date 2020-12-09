using SubredditBot.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SubredditBot.DataAccess
{
    public interface IDatabaseService<T> where T : RedditThing
    {
        Task<T> GetAny(string propertyName, string propertyValue);
        void Delete(string id);
        void Dispose();
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetByFilter(string propertyName, string propertyValue);
        Task<IEnumerable<T>> GetByQueryable(string propertyName, string propertyValue);
        T GetById(string id);
        IEnumerable<T> Upsert(IEnumerable<T> things);
        T Upsert(T thing);
    }
}