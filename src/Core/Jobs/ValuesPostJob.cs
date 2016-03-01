using System;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Logging;
using Foundatio.Messaging;
using Foundatio.Metrics;
using Foundatio.Queues;
using Foundatio.Storage;
using Foundatio.Utility;
using Samples.Core.Models;

namespace Samples.Core.Jobs {
    public class ValuesPostJob : QueueProcessorJobBase<ValuesPost> {
        private readonly ICacheClient _cacheClient;
        private readonly IMetricsClient _metricsClient;
        private readonly IMessagePublisher _publisher;
        private readonly IFileStorage _storage;

        public ValuesPostJob(ICacheClient cacheClient, IQueue<ValuesPost> queue, IMetricsClient metricsClient, IMessagePublisher publisher, IFileStorage storage, ILoggerFactory loggerFactory = null) : base(queue, loggerFactory) {
            _cacheClient = cacheClient;
            _metricsClient = metricsClient;
            _publisher = publisher;
            _storage = storage;
        }

        protected override async Task<JobResult> ProcessQueueEntryAsync(JobQueueEntryContext<ValuesPost> context) {
            var result = await _storage.GetFileContentsAsync(context.QueueEntry.Value.FilePath);

            Guid guid;
            if (!Guid.TryParse(result, out guid)) {
                await _metricsClient.CounterAsync("values.errors");
                await context.QueueEntry.AbandonAsync();
                await _storage.DeleteFileAsync(context.QueueEntry.Value.FilePath, context.CancellationToken);
                return JobResult.FailedWithMessage($"Unable to retrieve values data '{context.QueueEntry.Value.FilePath}'.");
            }

            await _metricsClient.CounterAsync("values.dequeued");
            await _cacheClient.SetAsync(context.QueueEntry.Value.FilePath, guid);
            await _metricsClient.CounterAsync("values.processed");
            _logger.Info().Message("Processing post: id={0} path={1}", context.QueueEntry.Id, context.QueueEntry.Value.FilePath).Write();

            await _publisher.PublishAsync(new EntityChanged {
                ChangeType = ChangeType.Added,
                Id = context.QueueEntry.Value.FilePath,
                Data = new DataDictionary { { "Value", guid } }
            });
            await context.QueueEntry.CompleteAsync();
            await _storage.DeleteFileAsync(context.QueueEntry.Value.FilePath, context.CancellationToken);

            return JobResult.Success;
        }
    }
}