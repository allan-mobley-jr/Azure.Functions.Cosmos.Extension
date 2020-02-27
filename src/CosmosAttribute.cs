// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    /// <summary>
    /// Attribute used to bind to an Azure Cosmos collection.
    /// </summary>
    /// <remarks>
    /// The method parameter type can be one of the following:
    /// <list type="bullet">
    /// <item><description><see cref="ICollector{T}"/></description></item>
    /// <item><description><see cref="IAsyncCollector{T}"/></description></item>
    /// <item><description>out T</description></item>
    /// <item><description>out T[]</description></item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    [Binding]
    public sealed class CosmosAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public CosmosAttribute()
        {
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="databaseName">The Cosmos database name.</param>
        /// <param name="containerName">The Cosmos container name.</param>
        public CosmosAttribute(string databaseName, string containerName)
        {
            DatabaseName = databaseName;
            ContainerName = containerName;
        }

        /// <summary>
        /// The name of the database to which the parameter applies.        
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string DatabaseName { get; private set; }

        /// <summary>
        /// The name of the container to which the parameter applies. 
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string ContainerName { get; private set; }

        /// <summary>
        /// Optional.
        /// Only applies to output bindings.
        /// If true, the database and container will be automatically created if they do not exist.
        /// </summary>
        public bool CreateIfNotExists { get; set; }

        /// <summary>
        /// Optional. A string value indicating the app setting to use as the Cosmos connection string, if different
        /// than the one specified in the <see cref="CosmosOptions"/>.
        /// </summary>
        [ConnectionString]
        public string ConnectionStringSetting { get; set; }

        /// <summary>
        /// Optional. The Id of the document to retrieve from the collection.
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string Id { get; set; }

        /// <summary>
        /// Optional.
        /// When specified on an output binding and <see cref="CreateIfNotExists"/> is true, defines the partition key 
        /// path for the created container.
        /// When specified on an input binding, specifies the partition key value for the lookup.
        /// May include binding parameters.
        /// </summary>
        [AutoResolve]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Optional.
        /// When specified on an output binding and <see cref="CreateIfNotExists"/> is true, defines the throughput of the created
        /// database.
        /// </summary>
        public int? DatabaseThroughput { get; set; }

        /// <summary>
        /// Optional.
        /// When specified on an output binding and <see cref="CreateIfNotExists"/> is true, defines the throughput of the created
        /// container.
        /// </summary>
        public int? ContainerThroughput { get; set; }

        /// <summary>
        /// Optional.
        /// When specified on an input binding using an <see cref="IEnumerable{T}"/>, defines the query to run against the container. 
        /// May include binding parameters.
        /// </summary>
        [AutoResolve(ResolutionPolicyType = typeof(CosmosSqlResolutionPolicy))]
        public string SqlQuery { get; set; }

        /// <summary>
        /// Optional.
        /// Sets user-agent suffix to include with every Azure Cosmos service interaction.
        /// May include binding parameters.
        /// </summary>
        /// <remarks>
        /// Setting this property after sending any request won't have any effect.
        /// </remarks>
        [AutoResolve]
        public string ApplicationName { get; set; }

        /// <summary>
        /// Optional.
        /// Sets the preferred geo-replicated region to be used for Azure Cosmos service interaction.
        /// May include binding parameters.
        /// </summary>
        /// <remarks>
        /// When this property is specified, the SDK prefers the region to perform operations. Also SDK auto-selects fallback geo-replicated regions for high availability.
        /// When this property is not specified, the SDK uses the write region as the preferred region for all operations.
        /// </remarks>
        /// <example>
        /// ApplicationRegion = "East US"
        /// </example>
        [AutoResolve]
        public string ApplicationRegion { get; set; }

        internal IReadOnlyDictionary<string, object> SqlQueryParameters { get; set; }
    }
}