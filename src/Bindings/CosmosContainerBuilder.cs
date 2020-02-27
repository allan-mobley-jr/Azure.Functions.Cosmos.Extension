// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Azure.Cosmos;
using Microsoft.Azure.WebJobs;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosContainerBuilder : IConverter<CosmosAttribute, CosmosContainer>
    {
        private readonly CosmosExtensionConfigProvider configProvider;

        public CosmosContainerBuilder(CosmosExtensionConfigProvider configProvider)
        {
            this.configProvider = configProvider;
        }

        public CosmosContainer Convert(CosmosAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            ICosmosService service = configProvider.GetService(
                configProvider.ResolveConnectionString(attribute.ConnectionStringSetting),
                attribute.ApplicationName,
                attribute.ApplicationRegion);

            if (attribute.CreateIfNotExists)
            {
                var context = new CosmosContext 
                {
                    ResolvedAttribute = attribute,
                    Service = service
                };

                CosmosUtility.CreateDatabaseAndContainerNameIfNotExistAsync(context).Wait();
            }

            return service.GetContainer(attribute.DatabaseName, attribute.ContainerName);
        }
    }
}