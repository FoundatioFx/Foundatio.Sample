![Foundatio](https://raw.githubusercontent.com/FoundatioFx/Foundatio/master/media/foundatio.png "Foundatio")

[![Slack Status](https://slack.exceptionless.com/badge.svg)](https://slack.exceptionless.com)

A sample application that shows off some of the features of [Foundatio](https://github.com/exceptionless/Foundatio). For all of the examples below, please take a look at the JavaScript console (Press F12 now) for client side messages via [SignalR](http://www.asp.net/signalr).

By default this example will use the in memory versions of Foundatio. In memory versions do not scale across processes or persist information across application restarts. To scale out out this example, please uncomment the RedisConnectionString in the web.config and ensure [redis](http://redis.io/download) is installed.

# Examples

To get started with the examples, please clone this repository and set the `Web` project as the startup project. Then press `F5` to start debugging!

## 1. Caching

Attempt to get a guid from the cache. If the value is not cached the value will be populated after 5 seconds. This code makes a call to the `ValuesController` `Get` controller action

## 2. Jobs, Message Bus, Queues, Caching
Posts a guid to the `ValuesController`, this will save a file to storage with a new guid and enqueue a work item to a dedicated job (`ValuesPostJob`). This job will then dequeue the item and read the guid from storage and parse it. If it can be parsed it will be persisted to the cache and publish a notification on the message bus.

When you call Post Value you can then Get Value button to get the updated guid that was processed from the `ValuesPostJob`. Clicking this button multiple times will trigger the jobs to run multiple times and will also increment the metrics (visible from the debug window when running in memory versions).

This example shows off how jobs, queues, and the message bus work together.

## 3. Shared Jobs, Message Bus, Queues, Caching
Deletes an id from the cache by calling the `ValuesController` `Delete` controller action. Which enqueues a shared job work item (`DeleteValueWorkItemHandler`). Unlike dedicated jobs which are processing one job type, shared jobs can handle different job types and process them at anytime. This is great when you have long running tasks that don't happen very often, such as deleting an entity with many children.

The nice thing about shared jobs is that you can easily report the progress via the messagebus by calling the `context.ReportProgress(percent, message);`

Please note that if you are seeing duplicated messages reported to the console, this is because there are two instances of the shared jobs running. To fix this, you'll need to open the `DeleteValueWorkItemHandler` in the `Core` project and uncomment `GetWorkItemLock` to add locking to this job.
