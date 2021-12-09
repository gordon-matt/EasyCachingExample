using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyCachingExample
{
    public interface ICachedRepository<T>
    {
        T Get(string key);

        Task<T> GetAsync(string key);

        T GetOrSet(string key, Func<T> func);

        Task<T> GetOrSetAsync(string key, Func<Task<T>> func);

        void Set(string key, T value, TimeSpan? ttl = null);

        void Set(IDictionary<string, T> values);

        Task SetAsync(string key, T value);

        Task SetAsync(IDictionary<string, T> values);

        void Remove(string key);

        Task RemoveAllAsync();

        Task RemoveAsync(string key);

        Task RemoveByPrefixAsync(string prefix);
    }
}