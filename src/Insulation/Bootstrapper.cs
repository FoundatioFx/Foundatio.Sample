using System;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Logging;
using Foundatio.Logging.NLog;
using Foundatio.Messaging;
using Foundatio.Queues;
using Foundatio.Serializer;
using Samples.Core;
using Samples.Core.Models;
using SimpleInjector;
using StackExchange.Redis;

namespace Insulation {
    public class Bootstrapper {
        public static void RegisterServices(Container container, ILoggerFactory loggerFactory) {
            loggerFactory.AddNLog();
            
            // NOTE: To enable redis, please uncomment the RedisConnectionString string in the web.config
            if (Settings.Current.EnableRedis) {
                var muxer = ConnectionMultiplexer.Connect(Settings.Current.RedisConnectionString);
                container.RegisterSingleton(muxer);

                container.RegisterSingleton<ICacheClient, RedisHybridCacheClient>();
                container.RegisterSingleton<IQueue<ValuesPost>>(() => new RedisQueue<ValuesPost>(muxer, behaviors: container.GetAllInstances<IQueueBehavior<ValuesPost>>()));
                container.RegisterSingleton<IQueue<WorkItemData>>(() => new RedisQueue<WorkItemData>(muxer, behaviors: container.GetAllInstances<IQueueBehavior<WorkItemData>>(), workItemTimeout: TimeSpan.FromHours(1)));
              
                container.RegisterSingleton<IMessageBus>(() => new RedisMessageBus(muxer.GetSubscriber(), serializer: container.GetInstance<ISerializer>()));
            } else {
                var logger = loggerFactory.CreateLogger<Bootstrapper>();
                logger.Warn().Message("Redis is NOT enabled.").Write();
            }
        }
    }
}