using System;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Logging;
using Foundatio.Messaging;
using Foundatio.Queues;
using Foundatio.Serializer;
using Samples.Core.Models;
using SimpleInjector;
using SimpleInjector.Packaging;
using StackExchange.Redis;

namespace Insulation {
    public class Bootstrapper : IPackage {
        public void RegisterServices(Container container) {
            // use nlog logging implementation
            //Logger.RegisterWriter(NLogWriter.WriteLog);

            bool enableRedis = false;
            if (enableRedis) {
                var muxer = ConnectionMultiplexer.Connect("localhost");
                container.RegisterSingleton(muxer);

                container.RegisterSingleton<ICacheClient, RedisHybridCacheClient>();
                container.RegisterSingleton<IQueue<ValuesPost>>(() => new RedisQueue<ValuesPost>(muxer, behaviours: container.GetAllInstances<IQueueBehavior<ValuesPost>>()));
                container.RegisterSingleton<IQueue<WorkItemData>>(() => new RedisQueue<WorkItemData>(muxer, behaviours: container.GetAllInstances<IQueueBehavior<WorkItemData>>(), workItemTimeout: TimeSpan.FromHours(1)));
              
                container.RegisterSingleton<IMessageBus>(() => new RedisMessageBus(muxer.GetSubscriber(), serializer: container.GetInstance<ISerializer>()));
            } else {
                Logger.Warn().Message("Redis is NOT enabled.").Write();
            }
        }
    }
}