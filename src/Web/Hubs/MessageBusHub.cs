using System;
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
            subscriber.Subscribe<EntityChanged>(OnEntityChanged);
            subscriber.Subscribe<WorkItemStatus>(OnWorkItemStatus);
        }

        private void OnEntityChanged(EntityChanged entityChanged) {
            if (entityChanged == null)
                return;

            try {
                Clients.All.entityChanged(entityChanged);
            } catch (NullReferenceException) {} // TODO: Remove this when SignalR bug is fixed.
        }
        
        private void OnWorkItemStatus(WorkItemStatus workItemStatus) {
            if (workItemStatus == null)
                return;

            try {
                Clients.All.workItemStatus(workItemStatus);
            } catch (NullReferenceException) {} // TODO: Remove this when SignalR bug is fixed.
        }
    }
}