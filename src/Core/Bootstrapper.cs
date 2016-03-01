using System;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Lock;
using Foundatio.Logging;
using Foundatio.Messaging;
using Foundatio.Metrics;
using Foundatio.Queues;
using Foundatio.Serializer;
using Foundatio.ServiceProviders;
using Foundatio.Storage;
using Samples.Core.Jobs.WorkItemHandlers;
using Samples.Core.Models;
using SimpleInjector;

namespace Samples.Core {
    public class Bootstrapper {
        public static void RegisterServices(Container container, ILoggerFactory loggerFactory) {
            container.RegisterSingleton<ILoggerFactory>(loggerFactory);
            container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

            ServiceProvider.Current = container;
            container.RegisterSingleton<ISerializer>(() => new JsonNetSerializer());

            var metricsClient = new InMemoryMetricsClient();
            container.RegisterSingleton<IMetricsClient>(metricsClient);
            container.RegisterSingleton<ICacheClient, InMemoryCacheClient>();

            container.RegisterCollection(typeof(IQueueBehavior<ValuesPost>), new[] {
                Lifestyle.Singleton.CreateRegistration(
                    () => new MetricsQueueBehavior<ValuesPost>(metricsClient), container)
            });
            container.RegisterSingleton<IQueue<ValuesPost>>(() => new InMemoryQueue<ValuesPost>(behaviors: container.GetAllInstances<IQueueBehavior<ValuesPost>>()));
            
            container.RegisterCollection(typeof(IQueueBehavior<WorkItemData>), new[] {
                Lifestyle.Singleton.CreateRegistration(
                    () => new MetricsQueueBehavior<WorkItemData>(metricsClient), container)
            });

            var handlers = new WorkItemHandlers();
            handlers.Register<DeleteValueWorkItem, DeleteValueWorkItemHandler>();
            container.RegisterSingleton(handlers);

            container.RegisterSingleton<IQueue<WorkItemData>>(() => new InMemoryQueue<WorkItemData>(behaviors: container.GetAllInstances<IQueueBehavior<WorkItemData>>(), workItemTimeout: TimeSpan.FromHours(1)));
            
            container.RegisterSingleton<IMessageBus, InMemoryMessageBus>();
            container.RegisterSingleton<IMessagePublisher>(container.GetInstance<IMessageBus>);
            container.RegisterSingleton<IMessageSubscriber>(container.GetInstance<IMessageBus>);

            container.RegisterSingleton<ILockProvider, CacheLockProvider>();
            container.RegisterSingleton<IFileStorage>(new InMemoryFileStorage());
        }
    }
}