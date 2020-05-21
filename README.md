# AzureSchedulingService
A domain agnostic scheduling service using azure cloud technologies and quartz.net

## Disclaimer
This is a POC that has only been minimally tested in a local environment, rigourous testing and searching for edge cases is required to be production ready. Also logging and health checks have not been implemented yet.

## High Level Overview
The service recieves HTTP requests to: add/update, delete, or retrieve a scheduled job. When a job is ready to run, a message is published to a topic / subscription. The subscription name is set when the job is initially created. Each job that is scheduled has a `JobIdentifier` that is unique to the subscription name for the job. This allows the service to be domain agnostic because it is the reponsibility of the consumers to use that job identitier to track the data needed to actually do work.

## Solution Breakdown

### Scheduling.Engine
This houses the logic for doing the actual scheduling, currently it uses the [quartz.net engine](https://github.com/quartznet/quartznet). It also handles tasks like converting a Job's schedule (`Job` from the shared package) to a cron epxression, building quartz.net triggers, etc. Aside from the unit tests, the rest of the application only interacts with the engine via ISchedulingActions. This provides the ability to start the scheduling engine, and to upsert or delete a quartz.net job.

### Scheduling.DataAccess
A requirement for this project was using Entity Framework to create the required database tables and to provide CRUD access to non-quartz releated data that requires persisting. EF is overkill and Dapper or some other micro-ORM would be fine. Quartz.net requires a DB to persist jobs past the lifetime of a process. However, it handles all the interactions with those tables behind the scenes. So as far as quartz goes, this project only creates the tables the library needs. 

The other responsibility of this project is to persist the meta data associated with a scheduled job--currently this is only the data in the shared package's `Job` class. When it's time to execute a job, this is the data that is published for the consumers to use. The application interacts with this logic via `IJobMetaDataRepository`.

### Scheduling.Orchestrator
Starts the scheduling engine and listens to messages for upserting and deleting jobs. Calls the logic in the scheduling engine when it's time to do those tasks. This project also implements the `IScheduledJobExecutor` interface from Scheduling.Engine, which is the logic that gets invoked when the scheduling engine determines that it's time to run a job. Currently the `IScheduledJobExecutor` logic will query the meta data for the scheduled job and push that data as a message to the consumers via azure service bus.

This project requires an appsettings.json with the following shape. Change the values as needed, especially the quartz related config:
```javascript
{
  "TopicName": "scheduling-execute",
  "AzureWebJobsServiceBus": "service bus conn string",
  "ConnectionStrings": {
    "SchedulingConnString": "DB conn string that the database that the data access logic uses"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*",
    "CORSCredentials": false
  },
  "Quartz": {
    "quartz.scheduler.instanceName": "SchedulingService",
    "quartz.threadPool.type": "Quartz.Simpl.SimpleThreadPool, Quartz",
    "quartz.threadPool.threadCount": "10",
    "quartz.threadPool.threadPriority": "Normal",
    "quartz.jobStore.misfireThreshold": "60000",
    "quartz.jobStore.type": "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
    "quartz.jobStore.useProperties": "true",
    "quartz.jobStore.dataSource": "default",
    "quartz.jobStore.lockHandler.type": "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz",
    "quartz.dataSource.default.connectionString": "DB conn string that the database that the data access logic uses",
    "quartz.dataSource.default.provider": "SqlServer",
    "quartz.serializer.type": "json",
    "quartz.jobStore.tablePrefix": "scheduling.quartz_"
  }
}
```
### Scheduling.Api
A group of HTTP-triggered azure functions that will push messages to the Orchestrator when a request is made to AddOrUpdate or delete a job. When a GET is requested for a job, the api queries the DataAccess project and returns back the job meta data.

A local.settings.json file is required with the following shape:
```javascript
{
  "IsEncrypted": false,
  "Values": {
    "ServiceBusConnectionString": "Azure service bus connection string",
    "StorageConnectionString": "Azure storage connection string",
    "SchedulingConnString": "DB conn string that the database that the data access logic uses",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "Environment": "Local"
  }
}
```

### Scheduling.SharedPacakge
Holds models, constants, etc shared throughout the service. Also provides a way for the outside world to interact with this service via the logic in `ISchedulingApiService`. This project can be deployed as a nuget package. That nuget package can be pulled in and used in other code bases in order to easily facilitate communication with the Scheduling.Api

### Scheduling.LocalTester
This project acts as an integration test for all the components that make up the scheduling service. It does this by providing a console app to schedule / delete / fetch jobs. It also can be used an an example of how to wire up the logic in the shared package.

It requires an appsettings.json file with the following shape, again values can be changed as needed:

```javascript
{
  "SchedulingBaseUrl": "http://localhost:7071",
  "AddOrUpdateFuncKey": null, 
  "DeleteFuncKey": null,
  "GetFuncKey": null,
  "Azure": {
    "AzureWebJobsServiceBus": "Service bus connection string",
    "SubscriptionName": "testsubscription",
    "TopicName": "scheduling-execute"
  }
}
```

### Scheduling.UnitTests
Tests the Scheduling.Engine related logic

## Usage
To run locally, start Scheduling.Orchestrator, Scheduling.Api, and Scheduling.LocalTester. Use the console app in LocalTester to verify everything is working correctly. To add/update migrations from visual studio, select the Orchestrator as the project to run, and from within the Package Manager Console window, select the DataAccess project.
