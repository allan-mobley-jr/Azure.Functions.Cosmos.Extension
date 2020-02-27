// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Bindings.Path;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class CosmosSqlResolutionPolicy : IResolutionPolicy
    {
        public string TemplateBind(PropertyInfo propInfo, Attribute resolvedAttribute, BindingTemplate bindingTemplate, IReadOnlyDictionary<string, object> bindingData)
        {
            if (bindingTemplate == null)
            {
                throw new ArgumentNullException(nameof(bindingTemplate));
            }

            if (bindingData == null)
            {
                throw new ArgumentNullException(nameof(bindingData));
            }

            if (!(resolvedAttribute is CosmosAttribute cosmosDbAttribute))
            {
                throw new NotSupportedException($"This policy is only supported for {nameof(CosmosAttribute)}.");
            }

            // build a SqlParameterCollection for each parameter            
            IDictionary<string, object> paramCollection = new Dictionary<string, object>();

            // also build up a dictionary replacing '{token}' with '@token' 
            IDictionary<string, object> replacements = new Dictionary<string, object>();
            
            foreach (var token in bindingTemplate.ParameterNames.Distinct())
            {
                string sqlToken = $"@{token}";
                paramCollection.Add(sqlToken, bindingData[token]);
                replacements.Add(token, sqlToken);
            }

            cosmosDbAttribute.SqlQueryParameters = new ReadOnlyDictionary<string, object>(paramCollection);

            string replacement = bindingTemplate.Bind(new ReadOnlyDictionary<string, object>(replacements));
            return replacement;
        }
    }
}