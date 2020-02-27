# Azure.Functions.Cosmos.Extension
This extension provides functionality for using the latest Azure.Cosmos .Net SDK for Cosmos DB bindings in Azure Functions.

**NOTE: CURRENTLY IN PREVIEW**

## Dependencies
###### .NETStandard 2.0
* Azure.Cosmos (>= 4.0.0-preview3)
* Microsoft.Azure.WebJobs (>= 3.0.14)
* Microsoft.CSharp (>= 4.7.0)

## Getting Started

### Trigger

*This extension only supports input and output bindings. Trigger support is forthcoming.*

### Input & Output Bindings

The Cosmos [input](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-input?tabs=csharp) 
and [output](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-output?tabs=csharp) 
binding examples shown in Microsoft docs are still relevant here.

Just use the new attribute name `Cosmos`:

```csharp
[FunctionName("ReadData")]
public static IActionResult Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
    [Cosmos(
        databaseName: "Test", 
        containerName: "Data",
        PartitionKey = "SampleData",
        SqlQuery = "SELECT * FROM c",
        ConnectionStringSetting = "Cosmos")] IEnumerable<SampleData> sampleData,
    ILogger log)
```

### Container Parameter Support

You can now specify a parameter of type `CosmosContainer`. 
And if you want to make sure it is created if it doesn't exist, just set `CreateIfNotExists` option to true:

```csharp
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Cosmos;

namespace Azure.Functions.Cosmos.Extension.Sample
{
    public static class CreateContainer
    {
        [FunctionName("CreateContainer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Cosmos(
                databaseName: "Test", 
                containerName: "Data",
                PartitionKey = "Discriminator",
                ConnectionStringSetting = "Cosmos",
                CreateIfNotExists = true)] CosmosContainer DataContainer,
            ILogger log)
        {
            try
            {
                CosmosContainer container = await DataContainer.ReadContainerAsync();
                log.LogInformation(container.Id);
                return new OkObjectResult(container.Id);
            }
            catch (CosmosException cex)
            {
                log.LogError(cex, cex.Message);
                return new BadRequestObjectResult(cex.Message);
            }
        }
    }
}
```