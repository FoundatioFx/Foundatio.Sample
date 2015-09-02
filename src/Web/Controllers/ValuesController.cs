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
        public async Task<Guid?> Get(string id) {
            if (String.IsNullOrEmpty(id))
                return null;

            var value = _cacheClient.Get<Guid?>(id);
            if (!value.HasValue) {
                // Simulate work
                await Task.Delay(TimeSpan.FromSeconds(5));

                value = Guid.NewGuid();
                _cacheClient.Set(id, value);
                return value;
            }

            return null;
        }

        // POST api/values
        public void Post([FromBody] string value) {
            _storage.SaveFile(value, Guid.NewGuid().ToString());
            _valuesPostQueue.Enqueue(new ValuesPost {
                FilePath = value
            });
        }

        // DELETE api/values/5
        public void Delete(string id) {
            _workItemQueue.Enqueue(new DeleteValueWorkItem { Id = id });
        }
    }
}