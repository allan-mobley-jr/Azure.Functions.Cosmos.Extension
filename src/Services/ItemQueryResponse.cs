// Copyright (c) 2020 Allan Mobley. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Mobsites.Azure.Functions.Cosmos.Extension
{
    internal class ItemQueryResponse<T>
    {
        public ItemQueryResponse()
        {
            Results = Enumerable.Empty<T>();
        }

        public IEnumerable<T> Results { get; set; }

        public string ResponseContinuation { get; set; }
    }
}