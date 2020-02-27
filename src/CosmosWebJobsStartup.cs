// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Mobsites.Azure.Functions.Cosmos.Extension;

[assembly: WebJobsStartup(typeof(CosmosWebJobsStartup))]

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    public class CosmosWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddCosmos();
        }
    }
}