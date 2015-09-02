using System;
using System.Threading.Tasks;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Utility;

namespace Samples.Core.WorkItemHandlers {
    public class DeleteValueWorkItemHandler : IWorkItemHandler {
        private readonly ICacheClient _cacheClient;

        public DeleteValueWorkItemHandler(ICacheClient cacheClient) {
            _cacheClient = cacheClient;
        }

        public IDisposable GetWorkItemLock(WorkItemContext context) {
            return Disposable.Empty;
        }

        public async Task HandleItem(WorkItemContext context) {
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

            context.ReportProgress(100);
        }
    }

    public class DeleteValueWorkItem {
        public string Id { get; set; }
    }
}