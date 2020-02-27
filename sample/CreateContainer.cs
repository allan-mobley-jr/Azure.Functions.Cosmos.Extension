using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Cosmos;
using Mobsites.Azure.Functions.Cosmos.Extension;

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