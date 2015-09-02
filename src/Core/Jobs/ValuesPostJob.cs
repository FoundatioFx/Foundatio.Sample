using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Messaging;
using Foundatio.Metrics;
using Foundatio.Queues;
using Foundatio.Storage;
using Foundatio.Utility;
using Samples.Core.Models;

namespace Samples.Core.Jobs {
    public class ValuesPostJob : JobBase {
        private readonly ICacheClient _cacheClient;
        private readonly IQueue<ValuesPost> _queue;
        private readonly IMetricsClient _metricsClient;
        private readonly IMessagePublisher _publisher;
        private readonly IFileStorage _storage;

        public ValuesPostJob(ICacheClient cacheClient, IQueue<ValuesPost> queue, IMetricsClient metricsClient, IMessagePublisher publisher, IFileStorage storage) {
            _cacheClient = cacheClient;
            _queue = queue;
            _metricsClient = metricsClient;
            _publisher = publisher;
            _storage = storage;
        }

        public void RunUntilEmpty() {
            while (_queue.GetQueueStats().Queued > 0)
                Run();
        }

        protected override async Task<JobResult> RunInternalAsync(CancellationToken token) {
            QueueEntry<ValuesPost> queueEntry = null;
            try {
                queueEntry = _queue.Dequeue(TimeSpan.FromSeconds(1));
            } catch (Exception ex) {
                if (!(ex is TimeoutException)) {
                    //Log.Error().Exception(ex).Message("An error occurred while trying to dequeue the next ValuesPost: {0}", ex.Message).Write();
                    return JobResult.FromException(ex);
                }
            }

            if (queueEntry == null)
                return JobResult.Success;

            if (token.IsCancellationRequested) {
                queueEntry.Abandon();
                return JobResult.Cancelled;
            }

            var result = _storage.GetFileContents(queueEntry.Value.FilePath);
            Guid guid;
            if (Guid.TryParse(result, out guid)) {
                await _metricsClient.CounterAsync("values.errors");
                queueEntry.Abandon();
                await _storage.DeleteFileAsync(queueEntry.Value.FilePath, token);
                return JobResult.FailedWithMessage(String.Format("Unable to retrieve values data '{0}'.", queueEntry.Value.FilePath));
            }

            await _metricsClient.CounterAsync("values.dequeued");
            _cacheClient.Set(queueEntry.Value.FilePath, guid);
            await _metricsClient.CounterAsync("values.processsed");
            //Log.Info().Message("Processing post: id={0} path={1} project={2} ip={3} v={4} agent={5}", queueEntry.Id, queueEntry.Value.FilePath, eventPostInfo.ProjectId, eventPostInfo.IpAddress, eventPostInfo.ApiVersion, eventPostInfo.UserAgent).WriteIf(!isInternalProject);

            _publisher.Publish(new EntityChanged {
                ChangeType = ChangeType.Added,
                Id = queueEntry.Value.FilePath,
                Data = new DataDictionary { { "Value", guid } }
            });
            queueEntry.Complete();
            await _storage.DeleteFileAsync(queueEntry.Value.FilePath, token);

            return JobResult.Success;
        }
    }
}