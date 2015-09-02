using System;
using System.Threading.Tasks;
using System.Web.Http;
using Foundatio.Caching;

namespace Samples.Web.Controllers {
    public class ValuesController : ApiController {
        private readonly ICacheClient _cacheClient;

        public ValuesController(ICacheClient cacheClient) {
            _cacheClient = cacheClient;
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
            
        }

        // PUT api/values/5
        public void Put(string id, [FromBody] string value) {}

        // DELETE api/values/5
        public void Delete(string id) {
            
        }
    }
}