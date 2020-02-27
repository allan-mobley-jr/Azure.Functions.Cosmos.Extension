// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosAsyncCollectorBuilder<T> : IConverter<CosmosAttribute, IAsyncCollector<T>>
    {
        private readonly CosmosExtensionConfigProvider configProvider;

        public CosmosAsyncCollectorBuilder(CosmosExtensionConfigProvider configProvider)
        {
            this.configProvider = configProvider;
        }

        public IAsyncCollector<T> Convert(CosmosAttribute attribute)
        {
            CosmosContext context = configProvider.CreateContext(attribute);
            return new CosmosAsyncCollector<T>(context);
        }
    }
}