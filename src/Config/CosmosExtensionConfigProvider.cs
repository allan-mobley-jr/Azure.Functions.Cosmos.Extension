// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    /// <summary>
    /// Defines the configuration options for the Cosmos binding.
    /// </summary>
    [Extension("Cosmos")]
    internal class CosmosExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration configuration;
        private readonly ICosmosServiceFactory cosmosServiceFactory;
        private readonly INameResolver nameResolver;
        private readonly CosmosOptions options;
        private readonly ILoggerFactory loggerFactory;

        public CosmosExtensionConfigProvider(
            IOptions<CosmosOptions> options,
            ICosmosServiceFactory cosmosServiceFactory,
            IConfiguration configuration,
            INameResolver nameResolver,
            ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.cosmosServiceFactory = cosmosServiceFactory;
            this.nameResolver = nameResolver;
            this.options = options.Value;
            this.loggerFactory = loggerFactory;
        }

        internal ConcurrentDictionary<string, ICosmosService> ClientCache { get; } = new ConcurrentDictionary<string, ICosmosService>();

        /// <inheritdoc />
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Apply ValidateConnection to all on this rule. 
            var rule = context.AddBindingRule<CosmosAttribute>();
            rule.AddValidator(ValidateConnection);

            rule.BindToCollector<DocumentOpenType>(typeof(CosmosAsyncCollectorBuilder<>), this);

            rule.BindToInput(new CosmosClientBuilder(this));

            rule.BindToInput(new CosmosContainerBuilder(this));

            // Enumerable inputs
            rule.WhenIsNull(nameof(CosmosAttribute.Id))
                .BindToInput<JArray>(typeof(CosmosJArrayBuilder), this);

            rule.WhenIsNull(nameof(CosmosAttribute.Id))
                .BindToInput<IEnumerable<DocumentOpenType>>(typeof(CosmosEnumerableBuilder<>), this);

            // Single input
            rule.WhenIsNotNull(nameof(CosmosAttribute.Id))
                .WhenIsNull(nameof(CosmosAttribute.SqlQuery))
                .BindToValueProvider<DocumentOpenType>((attr, t) => BindForItemAsync(attr, t));
        }

        internal void ValidateConnection(CosmosAttribute attribute, Type paramType)
        {
            if (string.IsNullOrEmpty(options.ConnectionString) &&
                string.IsNullOrEmpty(attribute.ConnectionStringSetting))
            {
                string attributeProperty = $"{nameof(CosmosAttribute)}.{nameof(CosmosAttribute.ConnectionStringSetting)}";
                string optionsProperty = $"{nameof(CosmosOptions)}.{nameof(CosmosOptions.ConnectionString)}";
                throw new InvalidOperationException(
                    $"The Cosmos connection string must be set either via the '{Constants.DefaultConnectionStringName}' IConfiguration connection string, via the {attributeProperty} property or via {optionsProperty}.");
            }
        }

        internal CosmosClient BindForClient(CosmosAttribute attribute)
        {
            string resolvedConnectionString = ResolveConnectionString(attribute.ConnectionStringSetting);
            ICosmosService service = GetService(resolvedConnectionString, attribute.ApplicationName, attribute.ApplicationRegion);

            return service.GetClient();
        }

        internal Task<IValueBinder> BindForItemAsync(CosmosAttribute attribute, Type type)
        {
            if (string.IsNullOrEmpty(attribute.Id))
            {
                throw new InvalidOperationException("The 'Id' property of a Cosmos single-item input binding cannot be null or empty.");
            }

            CosmosContext context = CreateContext(attribute);

            Type genericType = typeof(CosmosItemValueBinder<>).MakeGenericType(type);
            IValueBinder binder = (IValueBinder)Activator.CreateInstance(genericType, context);

            return Task.FromResult(binder);
        }

        internal string ResolveConnectionString(string attributeConnectionString)
        {
            // First, try the Attribute's string.
            if (!string.IsNullOrEmpty(attributeConnectionString))
            {
                return attributeConnectionString;
            }

            // Then use the options.
            return options.ConnectionString;
        }

        internal ICosmosService GetService(string connectionString, string applicationName = "", string applicationRegion = "")
        {
            string cacheKey = BuildCacheKey(connectionString, applicationName, applicationRegion);
            CosmosClientOptions clientOptions = CosmosUtility.BuildClientOptions(options.ConnectionMode, applicationName, applicationRegion);
            return ClientCache.GetOrAdd(cacheKey, (c) => cosmosServiceFactory.CreateService(connectionString, clientOptions));
        }

        internal CosmosContext CreateContext(CosmosAttribute attribute)
        {
            string resolvedConnectionString = ResolveConnectionString(attribute.ConnectionStringSetting);

            ICosmosService service = GetService(resolvedConnectionString, attribute.ApplicationName, attribute.ApplicationRegion);

            return new CosmosContext
            {
                Service = service,
                ResolvedAttribute = attribute,
            };
        }

        internal static bool IsSupportedEnumerable(Type type)
        {
            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return true;
            }

            return false;
        }

        internal static string BuildCacheKey(string connectionString, string applicationName, string applicationRegion) => $"{connectionString}|{applicationName}|{applicationRegion}";

        private class DocumentOpenType : OpenType.Poco
        {
            public override bool IsMatch(Type type, OpenTypeMatchContext context)
            {
                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return false;
                }

                if (type.FullName == "System.Object")
                {
                    return true;
                }

                return base.IsMatch(type, context);
            }
        }
    }
}