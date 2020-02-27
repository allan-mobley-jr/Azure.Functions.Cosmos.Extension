// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Cosmos;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosItemValueBinder<T> : IValueBinder
        where T : class
    {
        private readonly CosmosContext context;
        private JObject originalItem;

        public CosmosItemValueBinder(CosmosContext context)
        {
            this.context = context;
        }

        public Type Type
        {
            get { return typeof(T); }
        }

        public async Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            if (value == null || originalItem == null)
            {
                return;
            }

            await SetValueInternalAsync(originalItem, value as T, context);
        }

        public async Task<object> GetValueAsync()
        {
            PartitionKey partitionKey = PartitionKey.Null;
            if (!string.IsNullOrEmpty(context.ResolvedAttribute.PartitionKey))
            {
                partitionKey = new PartitionKey(context.ResolvedAttribute.PartitionKey);
            }

            T item = null;

            try
            {
                item = await context.Service.ReadItemAsync<T>(
                    context.ResolvedAttribute.DatabaseName,
                    context.ResolvedAttribute.ContainerName,
                    context.ResolvedAttribute.Id,
                    partitionKey);
            }
            catch (CosmosException ex) when ((HttpStatusCode)ex.Status == HttpStatusCode.NotFound)
            {
                // ignore not found; we'll return null below
            }

            if (item == null)
            {
                return item;
            }
            
            // Strings need to be handled differently.
            if (typeof(T) == typeof(string))
            {
                originalItem = JObject.FromObject(item);
                item = originalItem.ToString(Formatting.None) as T;
            }
            else
            {
                originalItem = JObject.FromObject(item);
            }

            return item;
        }

        public string ToInvokeString()
        {
            return string.Empty;
        }

        internal static async Task SetValueInternalAsync(JObject originalItem, T newItem, CosmosContext context)
        {
            // We can short-circuit here as strings are immutable.
            if (newItem is string)
            {
                return;
            }

            JObject currentValue = JObject.FromObject(newItem);

            if (HasChanged(originalItem, currentValue))
            {
                // make sure it's not the id that has changed
                if (TryGetId(currentValue, out string currentId) &&
                    !string.IsNullOrEmpty(currentId) &&
                    TryGetId(originalItem, out string originalId) &&
                    !string.IsNullOrEmpty(originalId))
                {
                    // make sure it's not the Id that has changed
                    if (!string.Equals(originalId, currentId, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("Cannot update the 'Id' property.");
                    }
                }
                else
                {
                    // If the serialized object does not have a lowercase 'id' property, DocDB will reject it.
                    // We'll just short-circuit here since we validate that the 'id' hasn't changed.
                    throw new InvalidOperationException(string.Format("The document must have an 'id' property."));
                }

                PartitionKey partitionKey = PartitionKey.Null;
                if (!string.IsNullOrEmpty(context.ResolvedAttribute.PartitionKey))
                {
                    partitionKey = new PartitionKey(context.ResolvedAttribute.PartitionKey);
                }

                await context.Service.ReplaceItemAsync(
                    context.ResolvedAttribute.DatabaseName,
                    context.ResolvedAttribute.ContainerName,
                    newItem,
                    context.ResolvedAttribute.Id,
                    partitionKey);
            }
        }

        internal static bool TryGetId(JObject item, out string id)
        {
            id = null;

            // 'id' must be lowercase
            if (item.TryGetValue("id", StringComparison.Ordinal, out JToken idToken))
            {
                id = idToken.ToString();
                return true;
            }

            return false;
        }

        internal static bool HasChanged(JToken original, JToken current)
        {
            return !JToken.DeepEquals(original, current);
        }

        internal static JObject CloneItem(object item)
        {
            string serializedItem = JsonConvert.SerializeObject(item);
            return JObject.Parse(serializedItem);
        }
    }
}