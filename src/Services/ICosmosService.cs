// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Azure;
using Azure.Cosmos;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    /// <summary>
    /// An abstraction layer for communicating with a Cosmos account.
    /// </summary>
    internal interface ICosmosService
    {
        /// <summary>
        /// Returns the underlying <see cref="CosmosClient"/>.
        /// </summary>
        /// <returns></returns>
        CosmosClient GetClient();

        /// <summary>
        /// Returns the underlying <see cref="CosmosContainer"/>.
        /// </summary>
        /// <returns></returns>
        CosmosContainer GetContainer(string databaseName, string containerName);

        /// <summary>
        /// Creates the specified <see cref="CosmosDatabase"/> if it doesn't exists or returns the existing one.
        /// </summary>
        /// <param name="databaseName">The id of the database to create.</param>
        /// <param name="throughput">The (optional) throughput to provision for a database in measurement of Request Units per second in the Azure Cosmos service.</param>
        /// <param name="requestOptions">The (optional) <see cref="RequestOptions"/> for the request.</param>
        /// <returns>The task object representing the service response for the asynchronous operation.</returns>
        Task<CosmosDatabase> CreateDatabaseIfNotExistsAsync(
            string databaseName,
            int? throughput = null,
            RequestOptions requestOptions = null);

        /// <summary>
        /// Creates the specified <see cref="CosmosContainer"/> if it doesn't exist or returns the existing one.
        /// </summary>
        /// <param name="database">The <see cref="CosmosDatabase"/> to create container in.</param>
        /// <param name="containerProperties">The <see cref="ContainerProperties"/> of the container to create.</param>
        /// <param name="throughput">The (optional) throughput to provision for a container in measurement of Request Units per second in the Azure Cosmos service.</param>
        /// <param name="requestOptions">The (optional) <see cref="RequestOptions"/> for the request.</param>
        /// <returns>The task object representing the service response for the asynchronous operation.</returns>
        Task<CosmosContainer> CreateContainerIfNotExistsAsync(
            CosmosDatabase database,
            ContainerProperties containerProperties,
            int? throughput = null,
            RequestOptions requestOptions = null);

        /// <summary>
        /// Inserts or replaces an item.
        /// </summary>
        /// <param name="container">The <see cref="CosmosContainer"/> to insert or replace item in.</param>
        /// <param name="item">The item to insert or replace.</param>
        /// <param name="partitionKey">The (optional) <see cref="PartitionKey"/> of the item to insert or replace.</param>
        /// <param name="requestOptions">The (optional) <see cref="ItemRequestOptions"/> for the request.</param>
        /// <returns>The task object representing the service response for the asynchronous operation.</returns>
        Task<T> UpsertItemAsync<T>(
            string databaseName,
            string containerId,
            T item,
            PartitionKey? partitionKey = null,
            ItemRequestOptions requestOptions = null);

        /// <summary>
        /// Reads an item.
        /// </summary>
        /// <param name="container">The <see cref="CosmosContainer"/> to read the item from.</param>
        /// <param name="id">The id of the item to read.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/> of the item to read.</param>
        /// <param name="requestOptions">The (optional) <see cref="ItemRequestOptions"/> for the request.</param>
        /// <returns>The task object representing the service response for the asynchronous operation.</returns>
        Task<T> ReadItemAsync<T>(
            string databaseName,
            string containerId,
            string id,
            PartitionKey partitionKey,
            ItemRequestOptions requestOptions = null);

        /// <summary>
        /// Replaces an item.
        /// </summary>
        /// <param name="container">The <see cref="CosmosContainer"/> to replace the item in.</param>
        /// <param name="item">The item to insert or replace.</param>
        /// <param name="id">The id of the item to replace.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/> of the item to replace.</param>
        /// <param name="requestOptions">The (optional) <see cref="ItemRequestOptions"/> for the request.</param>
        /// <returns></returns>
        Task<T> ReplaceItemAsync<T>(
            string databaseName,
            string containerId,
            T item,
            string id,
            PartitionKey? partitionKey = null,
            ItemRequestOptions requestOptions = null);

        /// <summary>
        /// Queries a collection.
        /// </summary>
        /// <param name="container">The <see cref="CosmosContainer"/> to query.</param>
        /// <param name="query">The <see cref="QueryDefinition"/> containing query and any query parameters.</param>
        /// <param name="continuation">The continuation token.</param>
        /// <returns>The response from the call to Cosmos</returns>
        AsyncPageable<T> GetItemQueryIterator<T>(
            string databaseName,
            string containerId,
            QueryDefinition query);
    }
}