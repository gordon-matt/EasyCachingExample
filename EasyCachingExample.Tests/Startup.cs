using System;
using System.Collections.Generic;
using Autofac;
using EasyCaching.CSRedis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCachingExample.Tests
{
    internal class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CacheConfiguration>(Configuration.GetSection("CacheSettings"));

            services.AddEasyCaching(option =>
            {
                option.UseInMemory("Memory");

                option.WithJson("JsonSerialization"); // Store as JSON, so we don't need to mark everything with [Serializable]

                option.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new List<string> { "127.0.0.1:6388,defaultDatabase=12,poolsize=10" }
                    };
                    config.SerializerName = "JsonSerialization";
                }, "Redis");

                // combine local and distributed
                option.UseHybrid(config =>
                {
                    config.TopicName = "test-topic";
                    config.EnableLogging = true;
                    config.LocalCacheProviderName = "Memory";
                    config.DistributedCacheProviderName = "Redis";
                })
                .WithCSRedisBus(busConf =>
                {
                    busConf.ConnectionStrings = new List<string> { "127.0.0.1:6379,defaultDatabase=13,poolsize=10" };
                });
            });

            #region StackExchange Redis (Slower than CS Redis)

            //services.AddEasyCaching(option =>
            //{
            //    option.UseInMemory("Memory");

            //    //option.WithJson("JsonSerialization"); // Store as JSON, so we don't need to mark everything with [Serializable]

            //    // distributed
            //    option.UseRedis(config =>
            //    {
            //        config.DBConfig.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
            //        //config.SerializerName = "JsonSerialization";
            //        //config.DBConfig.Database = 5;
            //    }, "Redis");

            //    // combine local and distributed
            //    option.UseHybrid(config =>
            //    {
            //        config.TopicName = "test-topic";
            //        config.EnableLogging = true;

            //        // specify the local cache provider name after v0.5.4
            //        config.LocalCacheProviderName = "Memory";
            //        // specify the distributed cache provider name after v0.5.4
            //        config.DistributedCacheProviderName = "Redis";
            //    }, "Hybrid")
            //    .WithRedisBus(busConf =>
            //    {
            //        busConf.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6380));
            //    });
            //});

            #endregion StackExchange Redis (Slower than CS Redis)
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup.cs")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ignore for Startup.cs")]
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(EasyCachingHybridRepository<>)).AsImplementedInterfaces().SingleInstance();
        }
    }
}