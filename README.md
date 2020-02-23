# AzureSchedulingService
A domain agnostic azure cloud scheduling service using quartz.net


NOTE: You will need to add appsettings.json to Application project with the following data:
```javascript
{
    "TopicName": "whatever",
    "AzureWebJobsServiceBus": "Service bus config string",
      "Quartz": {
      "quartz.threadPool.type": "Quartz.Simpl.SimpleThreadPool, Quartz",
      "quartz.threadPool.threadCount": "10",
      "quartz.threadPool.threadPriority": "Normal",
      "quartz.jobStore.misfireThreshold": "60000"
      ...
      }
}
```


and an appsettings.json to the testing project that has the following shape:
```javascript
{
  "Azure":  {
    "AzureWebJobsServiceBus": "Service bus config string", 
    "SchedulingQueueName":  "whatever"
  } 
}
```
