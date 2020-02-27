// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Azure;
using Azure.Cosmos;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal sealed class CosmosService : ICosmosService, IDisposable
    {
        private bool isDisposed;
        private CosmosClient client;

        public CosmosService(string connectionString, CosmosClientOptions cosmosClientOptions)
        {
            client = new CosmosClient(connectionString, cosmosClientOptions);
        }

        public CosmosClient GetClient()
        {
            return client;
        }

        public CosmosContainer GetContainer(string databaseName, string containerName)
        {
            return client.GetContainer(databaseName, containerName);
        }

        public async Task<CosmosDatabase> CreateDatabaseIfNotExistsAsync(
            string databaseName,
            int? throughput = null,
            RequestOptions requestOptions = null)
        {
            return await client.CreateDatabaseIfNotExistsAsync(databaseName, throughput, requestOptions);
        }

        public async Task<CosmosContainer> CreateContainerIfNotExistsAsync(
            CosmosDatabase database,
            ContainerProperties containerProperties,
            int? throughput = null,
            RequestOptions requestOptions = null)
        {
            return await database.CreateContainerIfNotExistsAsync(containerProperties, throughput, requestOptions);
        }

        public async Task<T> UpsertItemAsync<T>(
            string databaseName,
            string containerName,
            T item,
            PartitionKey? partitionKey = null,
            ItemRequestOptions requestOptions = null)
        {
            return await GetContainer(databaseName, containerName).UpsertItemAsync(item, partitionKey, requestOptions);
        }

        public async Task<T> ReadItemAsync<T>(
            string databaseName,
            string containerName,
            string id,
            PartitionKey partitionKey,
            ItemRequestOptions requestOptions = null)
        {
            return await GetContainer(databaseName, containerName).ReadItemAsync<T>(id, partitionKey, requestOptions);
        }

        public async Task<T> ReplaceItemAsync<T>(
            string databaseName,
            string containerName,
            T item,
            string id,
            PartitionKey? partitionKey = null,
            ItemRequestOptions requestOptions = null)
        {
            return await GetContainer(databaseName, containerName).ReplaceItemAsync<T>(item, id, partitionKey, requestOptions);
        }

        public AsyncPageable<T> GetItemQueryIterator<T>(
            string databaseName,
            string containerName,
            QueryDefinition query)
        {
            return GetContainer(databaseName, containerName).GetItemQueryIterator<T>(query);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }

                isDisposed = true;
            }
        }
    }
}