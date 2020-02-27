// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosContext
    {
        public CosmosAttribute ResolvedAttribute { get; set; }

        public ICosmosService Service { get; set; }
    }
}