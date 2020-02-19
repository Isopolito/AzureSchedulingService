# AzureSchedulingService
Generic azure cloud scheduling service


NOTE: You will need to add appsettings.json to Application project with the following daa:

{
    "AzureWebJobsServiceBus": "Service bus config string",
}

and an appsettings.json to the testing project that has the following shape:
{
  "Azure":  {
    "AzureWebJobsServiceBus": "Service bus config string", 
    "SchedulingQueueName":  "scheduling-inbound"
  } 
}
