using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Extensions;

namespace Hqv.Thermostat.Api.Infrastructure.Ecobee.Shared
{
    /// <summary>
    /// Helper class. Should be moved to Shared at some point.
    /// </summary>
    public static class UriHelper
    {
        public static string Create(string baseUri, string relativeUri, 
            ICollection<KeyValuePair<string, string>> queryParameters)
        {
            var queryBuilder = new QueryBuilder(queryParameters);
            return Create(baseUri, relativeUri, queryBuilder.ToQueryString().Value);
        }

        public static string Create(string baseUri, string relativeUri, string queryString = null)
        {
            var uriBuilder = new UriBuilder(baseUri);
            uriBuilder.Path += relativeUri;
            uriBuilder.Query = queryString;
            return uriBuilder.ToString();
        }
    }
}