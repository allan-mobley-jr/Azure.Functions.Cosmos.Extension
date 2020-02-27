// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosAsyncCollector<T> : IAsyncCollector<T>
    {
        private readonly CosmosContext cosmosContext;

        public CosmosAsyncCollector(CosmosContext cosmosContext)
        {
            this.cosmosContext = cosmosContext;
        }

        public async Task AddAsync(T item, CancellationToken cancellationToken = default)
        {
            bool create = false;
            try
            {
                await UpsertDocument(cosmosContext, item);
            }
            catch (Exception ex)
            {
                if (CosmosUtility.TryGetCosmosException(ex, out CosmosException ce) &&
                    (HttpStatusCode)ce.Status == HttpStatusCode.NotFound)
                {
                    if (cosmosContext.ResolvedAttribute.CreateIfNotExists)
                    {
                        create = true;
                    }
                    else
                    {
                        // Throw a custom error so that it's easier to decipher.
                        string message = $"The container '{cosmosContext.ResolvedAttribute.ContainerName}' (in database '{cosmosContext.ResolvedAttribute.DatabaseName}') does not exist. To automatically create the collection, set '{nameof(CosmosAttribute.CreateIfNotExists)}' to 'true'.";
                        throw new InvalidOperationException(message, ex);
                    }
                }
                else
                {
                    throw;
                }
            }

            if (create)
            {
                await CosmosUtility.CreateDatabaseAndContainerNameIfNotExistAsync(cosmosContext);

                await UpsertDocument(cosmosContext, item);
            }
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            // no-op
            return Task.FromResult(0);
        }

        internal static async Task UpsertDocument(CosmosContext context, T item)
        {
            // CosmosClient does not accept strings directly.
            object convertedItem = item;
            if (item is string)
            {
                convertedItem = JObject.Parse(item.ToString());
            }

            await context.Service.UpsertItemAsync(context.ResolvedAttribute.DatabaseName, context.ResolvedAttribute.ContainerName, convertedItem);
        }
    }
}