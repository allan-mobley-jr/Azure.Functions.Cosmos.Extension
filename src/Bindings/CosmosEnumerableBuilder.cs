// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosEnumerableBuilder<T> : IAsyncConverter<CosmosAttribute, IEnumerable<T>>
        where T : class
    {
        private readonly CosmosExtensionConfigProvider configProvider;

        public CosmosEnumerableBuilder(CosmosExtensionConfigProvider configProvider)
        {
            this.configProvider = configProvider;
        }

        public async Task<IEnumerable<T>> ConvertAsync(CosmosAttribute attribute, CancellationToken cancellationToken)
        {
            CosmosContext context = configProvider.CreateContext(attribute);

            List<T> finalResults = new List<T>();

            string continuation = null;

            QueryDefinition query = new QueryDefinition(context.ResolvedAttribute.SqlQuery);

            if ((context.ResolvedAttribute.SqlQueryParameters?.Count ?? 0) > 0)
            {
                foreach (var parameter in context.ResolvedAttribute.SqlQueryParameters)
                {
                    query = query.WithParameter(parameter.Key, parameter.Value);
                }
            }

            var response = context.Service.GetItemQueryIterator<T>(context.ResolvedAttribute.DatabaseName, context.ResolvedAttribute.ContainerName, query);

            do
            {
                await foreach (var page in response.AsPages(continuation))
                {
                    
                    finalResults.AddRange(page.Values);
                    continuation = page.ContinuationToken;
                }
            }
            while (!string.IsNullOrEmpty(continuation));

            return finalResults;
        }
    }
}