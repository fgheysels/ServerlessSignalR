# Serverless SignalR

## Introduction

This is a sample project to demonstrate that there's an issue with the `SignalRTrigger` trigger in Azure Functions when using Azure SignalR in serverless mode.
As explained in [this issue](Azure/azure-functions-signalrservice-extension#207), the `SignalRTrigger` does not seem to work when using a custom settingname for the `ConnectionStringSetting` property.

This behavior is reproducable using `Microsoft.Azure.WebJobs.Extensions.SignalRService 1.4.0`.

> Note that this is a sample project, where shortcuts have been taken regarding security:  in a real-life project, secrets like the FunctionApp app-key that is used in SignalR's upstream setting must be retrieved from KeyVault, just like connectionstrings which should also be retrieved from KeyVault.

## Getting started

1. Deploy the necessary Azure resources using the `serverlesssignalr_resources.json` ARM template that can be found in the `deploy` folder.  This ARM template deploys the following resources:
   - Azure Function App
   - Consumption plan based serverfarm 
   - Storage account for the Function App
   - Azure SignalR 
   - Application Insights

2. Publish the `ServerlessSignalR` project that can be found in the `ServerlessSignalR` VS.NET solution.  The solution can be found in the `src` folder.

3. Since we're using serverless SignalR, the SignalR upstream must be configured to communicate with the Azure Function.  This can be done by running the `configure_signalr_upstream.ps1` script that can be found in the `deploy` folder.  

## How to reproduce

1. Open the `Program.cs` file in `ServerlessSignalR.Client`.  On line 12, initialize the `url` variable to point to the Azure Function that has been deployed earlier.  This must be something like `https://myfunction.azurewebsites.net/api`.
2. Note that the `SubscribeToDevice` function in `TelemetryGenerator.cs` in the `ServerlessSignalR` Azure Functions project uses the `SignalRTrigger` and uses a custom settingname (instead of the default `AzureSignalRConnetionstring` setting) using the `ConnectionStringSetting` property.  The other 2 functions also specify a custom setting using the `ConnectionStringSetting` property.
3. Run the `ServerlessSignalR.Client` program.
4. When the `SubscribeToDevice` SignalR endpoint is invoked on line 39, the following error is given:
   > Microsoft.AspNetCore.SignalR.HubException: 'The SignalR Service connection string or endpoints are not set.'
5. Open the Azure Portal and go to the Azure FunctionApp.  Add a setting with the name `AzureSignalRConnectionString`.  The value must be the same value as the value of the `TelemetrySignalR_Connectionstring` setting.
6. Run the `ServerlessSignalR.Client` program again.  Invoking the `SubscribeToDevice` endpoint no longer throws an exception.  The `ConnectionStringSetting` on the `SignalRTrigger` seems to be ignored, as it is still applied on the  `SubscribeToDevice` function, and the function is now working.
