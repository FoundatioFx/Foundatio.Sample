using System;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Lock;
using Foundatio.Messaging;
using Foundatio.Metrics;
using Foundatio.Queues;
using Foundatio.Serializer;
using Foundatio.ServiceProviders;
using Foundatio.Storage;
using Samples.Core.Jobs.WorkItemHandlers;
using Samples.Core.Models;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Samples.Core {
    public class Bootstrapper : IPackage {
        public void RegisterServices(Container container) {
            // Foundation service provider
            ServiceProvider.Current = container;

            container.RegisterSingleton<ISerializer>(() => new JsonNetSerializer());

            var metricsClient = new InMemoryMetricsClient();
            metricsClient.StartDisplayingStats();
            container.RegisterSingleton<IMetricsClient>(metricsClient);
            container.RegisterSingleton<ICacheClient, InMemoryCacheClient>();

            container.RegisterSingleton<IQueueBehavior<ValuesPost>>(() => new MetricsQueueBehavior<ValuesPost>(metricsClient));
            container.RegisterSingleton<IQueue<ValuesPost>>(() => new InMemoryQueue<ValuesPost>(behaviours: container.GetAllInstances<IQueueBehavior<ValuesPost>>()));

            container.RegisterSingleton<IQueueBehavior<WorkItemData>>(() => new MetricsQueueBehavior<WorkItemData>(metricsClient));
            var handlers = new WorkItemHandlers();
            handlers.Register<DeleteValueWorkItem, DeleteValueWorkItemHandler>();
            container.RegisterSingleton(handlers);

            container.RegisterSingleton<IQueue<WorkItemData>>(() => new InMemoryQueue<WorkItemData>(behaviours: container.GetAllInstances<IQueueBehavior<WorkItemData>>(), workItemTimeout: TimeSpan.FromHours(1)));
            
            container.RegisterSingleton<IMessageBus, InMemoryMessageBus>();
            container.RegisterSingleton<IMessagePublisher>(container.GetInstance<IMessageBus>);
            container.RegisterSingleton<IMessageSubscriber>(container.GetInstance<IMessageBus>);

            container.RegisterSingleton<ILockProvider, CacheLockProvider>();
            container.RegisterSingleton<IFileStorage>(new InMemoryFileStorage());
        }
    }
}