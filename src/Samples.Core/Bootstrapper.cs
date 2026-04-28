using Foundatio.Caching;
using Foundatio.Extensions.Hosting.Jobs;
using Foundatio.Jobs;
using Foundatio.Lock;
using Foundatio.Messaging;
using Foundatio.Queues;
using Foundatio.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samples.Core.Jobs;
using Samples.Core.Models;

namespace Samples.Core;

public static class Bootstrapper
{
    public static void RegisterServices(IServiceCollection services, bool runJobsInProcess = true)
    {
        // In-memory defaults — Insulation swaps these for production implementations
        services.AddSingleton<ICacheClient>(sp => new InMemoryCacheClient(o => o.LoggerFactory(sp.GetService<ILoggerFactory>())));
        services.AddSingleton<IMessageBus>(sp => new InMemoryMessageBus(o => o.LoggerFactory(sp.GetService<ILoggerFactory>())));
        services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<IMessageBus>());
        services.AddSingleton<IFileStorage>(sp => new InMemoryFileStorage(o => o.LoggerFactory(sp.GetService<ILoggerFactory>())));
        services.AddSingleton<ILockProvider>(sp => new CacheLockProvider(
            sp.GetRequiredService<ICacheClient>(),
            sp.GetRequiredService<IMessageBus>(),
            sp.GetService<ILoggerFactory>()));

        // Queues
        services.AddSingleton<IQueue<ValuesPost>>(sp => new InMemoryQueue<ValuesPost>(o => o
            .LoggerFactory(sp.GetService<ILoggerFactory>())));
        services.AddSingleton<IQueue<WorkItemData>>(sp => new InMemoryQueue<WorkItemData>(o => o
            .LoggerFactory(sp.GetService<ILoggerFactory>())));

        // Work item handlers
        services.AddTransient<DeleteValueWorkItemHandler>();
        services.AddSingleton(sp =>
        {
            var handlers = new WorkItemHandlers();
            handlers.Register<DeleteValueWorkItem>(sp.GetRequiredService<DeleteValueWorkItemHandler>());
            return handlers;
        });

        // Background jobs — only register hosted services when running in-process
        if (runJobsInProcess)
        {
            services.AddJob<ValuesPostJob>();
            services.AddJob<WorkItemJob>();
        }
    }
}
