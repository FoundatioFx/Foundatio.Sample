using System;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.Jobs;
using Foundatio.Messaging;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Samples.Core.Models;

namespace Samples.Web.Hubs {
    public interface IMessageBusHubClientMethods {
        void entityChanged(EntityChanged entityChanged);
        void workItemStatus(WorkItemStatus workItemStatus);
    }

    [HubName("messages")]
    public class MessageBusHub : Hub<IMessageBusHubClientMethods> {
        public MessageBusHub(IMessageSubscriber subscriber) {
            subscriber.Subscribe<EntityChanged>(OnEntityChangedAsync);
            subscriber.Subscribe<WorkItemStatus>(OnWorkItemStatusAsync);
        }

        private Task OnEntityChangedAsync(EntityChanged entityChanged, CancellationToken cancellationToken = default(CancellationToken)) {
            if (entityChanged != null)
                Clients.All.entityChanged(entityChanged);

            return Task.FromResult(0);
        }
        
        private Task OnWorkItemStatusAsync(WorkItemStatus workItemStatus, CancellationToken cancellationToken = default(CancellationToken)) {
            if (workItemStatus != null)
                Clients.All.workItemStatus(workItemStatus);

            return Task.FromResult(0);
        }
    }
}