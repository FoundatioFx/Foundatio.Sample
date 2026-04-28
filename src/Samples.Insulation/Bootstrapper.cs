using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Lock;
using Foundatio.Messaging;
using Foundatio.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Samples.Core.Models;
using StackExchange.Redis;

namespace Samples.Insulation;

public static class Bootstrapper
{
    /// <summary>
    /// When a Redis connection is available, swap in Redis implementations
    /// for caching, messaging, queues, and locks — replacing the in-memory
    /// defaults registered by Core.Bootstrapper.
    /// </summary>
    public static void RegisterServices(IServiceCollection services, string? redisConnectionString)
    {
        if (string.IsNullOrEmpty(redisConnectionString))
            return;

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.Replace(ServiceDescriptor.Singleton<ICacheClient>(sp =>
            new RedisCacheClient(o => o
                .ConnectionMultiplexer(sp.GetRequiredService<IConnectionMultiplexer>())
                .LoggerFactory(sp.GetService<ILoggerFactory>()))));

        services.Replace(ServiceDescriptor.Singleton<IMessageBus>(sp =>
            new RedisMessageBus(o => o
                .Subscriber(sp.GetRequiredService<IConnectionMultiplexer>().GetSubscriber())
                .LoggerFactory(sp.GetService<ILoggerFactory>()))));

        services.Replace(ServiceDescriptor.Singleton<IQueue<ValuesPost>>(sp =>
            new RedisQueue<ValuesPost>(o => o
                .ConnectionMultiplexer(sp.GetRequiredService<IConnectionMultiplexer>())
                .LoggerFactory(sp.GetService<ILoggerFactory>()))));

        services.Replace(ServiceDescriptor.Singleton<IQueue<WorkItemData>>(sp =>
            new RedisQueue<WorkItemData>(o => o
                .ConnectionMultiplexer(sp.GetRequiredService<IConnectionMultiplexer>())
                .LoggerFactory(sp.GetService<ILoggerFactory>()))));

        services.Replace(ServiceDescriptor.Singleton<ILockProvider>(sp =>
            new CacheLockProvider(
                sp.GetRequiredService<ICacheClient>(),
                sp.GetRequiredService<IMessageBus>(),
                sp.GetService<ILoggerFactory>())));
    }
}
