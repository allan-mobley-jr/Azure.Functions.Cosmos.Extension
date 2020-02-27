// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    /// <summary>
    /// Extension methods for Cosmos integration.
    /// </summary>
    public static class CosmosWebJobsBuilderExtensions
    {
        /// <summary>
        /// Adds the Cosmos extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        public static IWebJobsBuilder AddCosmos(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<CosmosExtensionConfigProvider>() 
                .ConfigureOptions<CosmosOptions>((config, path, options) =>
                {
                    options.ConnectionString = config.GetConnectionString(Constants.DefaultConnectionStringName);

                    IConfigurationSection section = config.GetSection(path);
                    section.Bind(options);
                });

            builder.Services.AddSingleton<ICosmosServiceFactory, DefaultCosmosServiceFactory>();

            return builder;
        }

        /// <summary>
        /// Adds the Cosmos extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <param name="configure">An <see cref="Action{CosmosOptions}"/> to configure the provided <see cref="CosmosOptions"/>.</param>
        public static IWebJobsBuilder AddCosmos(this IWebJobsBuilder builder, Action<CosmosOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddCosmos();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}