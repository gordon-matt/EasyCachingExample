using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyCaching.Core;
using Microsoft.Extensions.Options;

namespace EasyCachingExample
{
    public class EasyCachingHybridRepository<T> : ICachedRepository<T>
    {
        private readonly TimeSpan defaultTtl;
        private readonly IHybridCachingProvider provider;

        public EasyCachingHybridRepository(IHybridCachingProvider provider, IOptions<CacheConfiguration> config)
        {
            this.provider = provider;
            defaultTtl = config.Value.Ttl ?? TimeSpan.FromTicks(DateTime.UtcNow.AddYears(1).Ticks);
        }

        protected static string Prefix => typeof(T).FullName;

        public T Get(string key)
        {
            string cacheKey = GetCacheKey(key);
            var cacheValue = provider.Get<T>(cacheKey);
            return cacheValue.HasValue ? cacheValue.Value : default;
        }

        public async Task<T> GetAsync(string key)
        {
            string cacheKey = GetCacheKey(key);
            var cacheValue = await provider.GetAsync<T>(cacheKey);
            return cacheValue.HasValue ? cacheValue.Value : default;
        }

        public T GetOrSet(string key, Func<T> func)
        {
            var value = Get(key);
            if (value == null)
            {
                value = func();
                Set(key, value);
            }
            return value;
        }

        public async Task<T> GetOrSetAsync(string key, Func<Task<T>> func)
        {
            var value = await GetAsync(key);
            if (value == null)
            {
                value = await func();
                await SetAsync(key, value);
            }
            return value;
        }

        public void Set(string key, T value, TimeSpan? ttl = null)
        {
            string cacheKey = GetCacheKey(key);
            provider.Set(cacheKey, value, ttl ?? defaultTtl);
        }

        public async Task SetAsync(string key, T value)
        {
            string cacheKey = GetCacheKey(key);
            await provider.SetAsync(cacheKey, value, defaultTtl);
        }

        public void Set(IDictionary<string, T> values)
        {
            foreach (var keyValue in values)
            {
                string cacheKey = GetCacheKey(keyValue.Key);
                provider.Set(cacheKey, keyValue.Value, defaultTtl);
            }
        }

        public async Task SetAsync(IDictionary<string, T> values)
        {
            foreach (var keyValue in values)
            {
                string cacheKey = GetCacheKey(keyValue.Key);
                await provider.SetAsync(cacheKey, keyValue.Value, defaultTtl);
            }
        }

        public void Remove(string key)
        {
            string cacheKey = GetCacheKey(key);
            provider.Remove(cacheKey);
        }

        public async Task RemoveAllAsync()
        {
            await provider.RemoveByPrefixAsync(Prefix);
        }

        public async Task RemoveAsync(string key)
        {
            string cacheKey = GetCacheKey(key);
            await provider.RemoveAsync(cacheKey);
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            string cachePrefix = $"{Prefix}:{prefix}";
            await provider.RemoveByPrefixAsync(cachePrefix);
        }

        private static string GetCacheKey(string key)
        {
            return $"{Prefix}:{key}";
        }
    }
}