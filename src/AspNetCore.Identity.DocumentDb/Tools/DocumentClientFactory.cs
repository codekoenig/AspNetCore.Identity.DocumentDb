using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace AspNetCore.Identity.DocumentDb.Tools
{
    internal static class DocumentClientFactory
    {
        internal static DocumentClient CreateClient(Uri serviceEndpoint, string authKeyOrResourceToken, JsonSerializerSettings serializerSettings = null, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? consistencyLevel = null)
        {
            serializerSettings = serializerSettings ?? new JsonSerializerSettings();
            serializerSettings.Converters.Add(new JsonClaimConverter());
            serializerSettings.Converters.Add(new JsonClaimsPrincipalConverter());
            serializerSettings.Converters.Add(new JsonClaimsIdentityConverter());

#if NETSTANDARD2
            return new DocumentClient(serviceEndpoint, authKeyOrResourceToken, serializerSettings, connectionPolicy, consistencyLevel);
#else
            // DocumentDB SDK only supports setting the JsonSerializerSettings on versions after NetStandard 2.0
            JsonConvert.DefaultSettings = () => serializerSettings;
            return new DocumentClient(serviceEndpoint, authKeyOrResourceToken, connectionPolicy, consistencyLevel);
#endif
        }
    }
}
