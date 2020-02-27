using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Mobsites.Azure.Functions.Cosmos.Extension;

namespace Azure.Functions.Cosmos.Extension.Sample
{
    public static class ReadData
    {
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
        {
            try
            {
                string message = $"Read {sampleData.Count()} items.";

                if (sampleData.Count() == 10)
                {
                    log.LogInformation(message);
                    return new OkObjectResult(message);
                }
                else 
                {
                    log.LogInformation(message);
                    return new BadRequestObjectResult(message);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }
    }
}
