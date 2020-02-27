// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosJArrayBuilder : IAsyncConverter<CosmosAttribute, JArray>
    {
        private readonly CosmosEnumerableBuilder<JObject> builder;

        public CosmosJArrayBuilder(CosmosExtensionConfigProvider configProvider)
        {
            builder = new CosmosEnumerableBuilder<JObject>(configProvider);
        }

        public async Task<JArray> ConvertAsync(CosmosAttribute attribute, CancellationToken cancellationToken)
        {
            IEnumerable<JObject> results = await builder.ConvertAsync(attribute, cancellationToken);
            return JArray.FromObject(results);
        }
    }
}