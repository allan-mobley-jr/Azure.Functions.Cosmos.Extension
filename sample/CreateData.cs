using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mobsites.Azure.Functions.Cosmos.Extension;

namespace Azure.Functions.Cosmos.Extension.Sample
{
    public static class CreateData
    {
        [FunctionName("CreateData")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [Cosmos(
                databaseName: "Test", 
                containerName: "Data",
                ConnectionStringSetting = "Cosmos")] IAsyncCollector<SampleData> sampleData,
            ILogger log)
        {
            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    await sampleData.AddAsync(new SampleData
                    {
                        Id = i.ToString()
                    });
                }

                log.LogInformation("Created sample data.");
                return new OkObjectResult("Created sample data.");
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
