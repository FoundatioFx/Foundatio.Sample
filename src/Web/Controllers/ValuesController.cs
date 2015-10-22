using System;
using System.Threading.Tasks;
using System.Web.Http;
using Foundatio.Caching;
using Foundatio.Jobs;
using Foundatio.Queues;
using Foundatio.Storage;
using Samples.Core.Jobs.WorkItemHandlers;
using Samples.Core.Models;

namespace Samples.Web.Controllers {
    public class ValuesController : ApiController {
        private readonly ICacheClient _cacheClient;
        private readonly IQueue<ValuesPost> _valuesPostQueue;
        private readonly IQueue<WorkItemData> _workItemQueue;
        private readonly IFileStorage _storage;

        public ValuesController(ICacheClient cacheClient, IQueue<ValuesPost> valuesPostQueue, IQueue<WorkItemData> workItemQueue, IFileStorage storage) {
            _cacheClient = cacheClient;
            _valuesPostQueue = valuesPostQueue;
            _workItemQueue = workItemQueue;
            _storage = storage;
        }

        // GET api/values/xyz
        public async Task<Guid?> GetAsync(string id) {
            if (String.IsNullOrEmpty(id))
                return null;

            var value = (await _cacheClient.GetAsync<Guid?>(id)).Value;
            if (value.HasValue)
                return value.Value;

            // Simulate work
            await Task.Delay(TimeSpan.FromSeconds(5));

            value = Guid.NewGuid();
            await _cacheClient.SetAsync(id, value);
            return value;
        }

        // POST api/values
        public async Task PostAsync([FromBody] string id) {
            if (String.IsNullOrEmpty(id))
                return;

            await _storage.SaveFileAsync(id, Guid.NewGuid().ToString());
            await _valuesPostQueue.EnqueueAsync(new ValuesPost {
                FilePath = id
            });
        }

        // DELETE api/values/5
        public Task DeleteAsync(string id) {
            return _workItemQueue.EnqueueAsync(new DeleteValueWorkItem { Id = id });
        }
    }
}