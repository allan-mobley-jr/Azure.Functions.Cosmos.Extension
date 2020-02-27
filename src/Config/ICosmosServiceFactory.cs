// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure.Cosmos;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal interface ICosmosServiceFactory
    {
        ICosmosService CreateService(string connectionString, CosmosClientOptions cosmosClientOptions);
    }
}