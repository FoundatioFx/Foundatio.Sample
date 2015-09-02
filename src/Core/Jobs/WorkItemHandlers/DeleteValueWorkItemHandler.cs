using System;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Messaging;
using Foundatio.Utility;
using Samples.Core.Models;

namespace Samples.Core.Jobs.WorkItemHandlers {
    public class DeleteValueWorkItemHandler : WorkItemHandlerBase {
        private readonly ICacheClient _cacheClient;
        private readonly IMessagePublisher _publisher;

        public DeleteValueWorkItemHandler(ICacheClient cacheClient, IMessagePublisher publisher) {
            _cacheClient = cacheClient;
            _publisher = publisher;
        }
        
        public override async Task HandleItem(WorkItemContext context) {
            var workItem = context.GetData<DeleteValueWorkItem>();
            
            context.ReportProgress(0, String.Format("Starting to delete item: {0}", workItem.Id));
            await Task.Delay(TimeSpan.FromSeconds(2.5));
            context.ReportProgress(50, String.Format("Deleting"));
            await Task.Delay(TimeSpan.FromSeconds(.5));
            context.ReportProgress(70, String.Format("Deleting."));
            await Task.Delay(TimeSpan.FromSeconds(.5));
            context.ReportProgress(90, String.Format("Deleting.."));
            await Task.Delay(TimeSpan.FromSeconds(.5));

            _cacheClient.Remove(workItem.Id);
            _publisher.Publish(new EntityChanged {
                ChangeType = ChangeType.Removed,
                Id = workItem.Id
            });

            context.ReportProgress(100);
        }
    }

    public class DeleteValueWorkItem {
        public string Id { get; set; }
    }
}