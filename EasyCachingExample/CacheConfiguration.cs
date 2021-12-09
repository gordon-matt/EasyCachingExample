using System;

namespace EasyCachingExample
{
    public class CacheConfiguration
    {
        public string Prefix;

        public int? RedisDatabase;

        public CacheConfiguration()
        {
        }

        public CacheConfiguration(CacheConfiguration config)
        {
            Prefix = config.Prefix;
            Ttl = config.Ttl;
            MemoryTtl = config.MemoryTtl;
            RedisDatabase = config.RedisDatabase;
            BlackListDuration = config.BlackListDuration;
        }

        public TimeSpan BlackListDuration { get; set; }

        public TimeSpan MemoryTtl { get; set; }

        /// <summary>
        /// When Ttl is null, It means the cache will be persisted permanently
        /// </summary>
        public TimeSpan? Ttl { get; set; }
    }
}