// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Azure.Cosmos;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal static class CosmosUtility
    {
        internal static bool TryGetCosmosException(Exception originalEx, out CosmosException cosmosClientEx)
        {
            cosmosClientEx = originalEx as CosmosException;

            if (cosmosClientEx != null)
            {
                return true;
            }

            if (!(originalEx is AggregateException ae))
            {
                return false;
            }

            cosmosClientEx = ae.InnerException as CosmosException;

            return cosmosClientEx != null;
        }

        internal static async Task CreateDatabaseAndContainerNameIfNotExistAsync(CosmosContext context)
        {
            await CreateDatabaseAndContainerNameIfNotExistAsync(
                context.Service, 
                context.ResolvedAttribute.DatabaseName, 
                context.ResolvedAttribute.ContainerName,
                context.ResolvedAttribute.PartitionKey, 
                context.ResolvedAttribute.DatabaseThroughput,
                context.ResolvedAttribute.ContainerThroughput);
        }

        internal static async Task CreateDatabaseAndContainerNameIfNotExistAsync(
            ICosmosService service, 
            string databaseName, 
            string containerName, 
            string partitionKey, 
            int? databaseThroughput = null,
            int? containerThroughput = null)
        {
            var database = await service.CreateDatabaseIfNotExistsAsync(databaseName, databaseThroughput);
            await CreateContainerIfNotExistsAsync(service, database, containerName, partitionKey, containerThroughput);
        }

        internal static CosmosClientOptions BuildClientOptions(ConnectionMode? connectionMode, string applicationName, string applicationRegion)
        {
            CosmosClientOptions clientOptions = new CosmosClientOptions();
            if (connectionMode.HasValue)
            {
                // Default is Direct
                clientOptions.ConnectionMode = connectionMode.Value;
            }

            if (!string.IsNullOrEmpty(applicationName))
            {
                clientOptions.ApplicationName = applicationName;
            }

            if (!string.IsNullOrEmpty(applicationRegion))
            {
                clientOptions.ApplicationRegion = applicationRegion;
            }

            return clientOptions;
        }

        private static async Task<CosmosContainer> CreateContainerIfNotExistsAsync(
            ICosmosService service, 
            CosmosDatabase database, 
            string containerName,
            string partitionKey, 
            int? throughput)
        {
            string partitionKeyPath = null;

            if (!string.IsNullOrEmpty(partitionKey))
            {
                if (!partitionKey.StartsWith("/"))
                {
                    partitionKeyPath = $"/{partitionKey}";
                }
            }
            
            if (throughput == 0)
            {
                throughput = null;
            }

            return await service.CreateContainerIfNotExistsAsync(
                database, 
                new ContainerProperties { Id = containerName, PartitionKeyPath = partitionKeyPath }, 
                throughput);
        }
    }
}