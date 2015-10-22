using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Lock;
using Foundatio.Messaging;
using Samples.Core.Models;

namespace Samples.Core.Jobs.WorkItemHandlers {
    public class DeleteValueWorkItemHandler : WorkItemHandlerBase {
        private readonly ICacheClient _cacheClient;
        private readonly IMessagePublisher _publisher;
        private readonly ILockProvider _lockProvider;


        public DeleteValueWorkItemHandler(ICacheClient cacheClient, IMessagePublisher publisher, ILockProvider lockProvider) {
            _cacheClient = cacheClient;
            _publisher = publisher;
            _lockProvider = lockProvider;
        }

        // NOTE: Uncomment to ensure only one work item handler is called at a time.
        //public override Task<ILock> GetWorkItemLockAsync(object workItem, CancellationToken cancellationToken = new CancellationToken()) {
        //    return _lockProvider.AcquireAsync(nameof(DeleteValueWorkItemHandler), cancellationToken: cancellationToken);
        //}

        public override async Task HandleItemAsync(WorkItemContext context) {
            var workItem = context.GetData<DeleteValueWorkItem>();
            await context.ReportProgressAsync(0, $"Starting to delete item: {workItem.Id}.");
            await context.ReportProgressAsync(1, "If you are seeing multiple progresses. Please uncomment the lock in the DeleteValueWorkItemHandler.");
            await Task.Delay(TimeSpan.FromSeconds(2.5));
            await context.ReportProgressAsync(50, "Deleting");
            await Task.Delay(TimeSpan.FromSeconds(.5));
            await context.ReportProgressAsync(70, "Deleting.");
            await Task.Delay(TimeSpan.FromSeconds(.5));
            await context.ReportProgressAsync(90, "Deleting..");
            await Task.Delay(TimeSpan.FromSeconds(.5));

            await _cacheClient.RemoveAsync(workItem.Id);
            await _publisher.PublishAsync(new EntityChanged {
                ChangeType = ChangeType.Removed,
                Id = workItem.Id
            });

            await context.ReportProgressAsync(100);
        }
    }

    public class DeleteValueWorkItem {
        public string Id { get; set; }
    }
}