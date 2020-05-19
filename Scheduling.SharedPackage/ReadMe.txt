This is the "api" for the scheduling service. It will be setup to be created as a nuget package on deployment and it will hold the logic for other code to use to interact with the service.
This works because the WebApi service in this package will http trigger the azure functions for scheduling (in application). Those functions take the payload and drop messages on to the queues that the 
other functions listen to actually do the scheduling work.  This allows the code that wants to schedule messages to call a method in this api, w/o having to worry about setting up 
serivce bus and sending messages to queues. Sending messages from the http trigged functions keeps the http requests short and it allows for the service bus to distribute the work better if under heavy load.

The only time consumers have to deal with the service bus directly is to listen to the topic / subscription that they're interested in when it's time to execute jobs.

This package also has some models the whole application requires.
