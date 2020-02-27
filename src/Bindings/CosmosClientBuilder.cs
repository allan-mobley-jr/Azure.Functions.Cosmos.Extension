// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosClientBuilder : IConverter<CosmosAttribute, CosmosClient>
    {
        private readonly CosmosExtensionConfigProvider configProvider;

        public CosmosClientBuilder(CosmosExtensionConfigProvider configProvider)
        {
            this.configProvider = configProvider;
        }

        public CosmosClient Convert(CosmosAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            ICosmosService service = configProvider.GetService(
                configProvider.ResolveConnectionString(attribute.ConnectionStringSetting),
                attribute.ApplicationName,
                attribute.ApplicationRegion);

            return service.GetClient();
        }
    }
}