using System;
using System.Collections.Generic;
using System.Text;
using AspNetCore.Identity.DocumentDb.Tools;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace AspNetCore.Identity.DocumentDb
{
    public static class DocumentClientExtensions
    {
        public static DocumentClient AddDefaultDocumentClientForIdentity(this IServiceCollection services, Uri serviceEndpoint, string authKeyOrResourceToken, JsonSerializerSettings serializerSettings = null, ConnectionPolicy connectionPolicy = null, ConsistencyLevel? consistencyLevel = null, Action<DocumentClient> afterCreation = null)
        {
            var documentClient = DocumentClientFactory.CreateClient(serviceEndpoint, authKeyOrResourceToken, serializerSettings, connectionPolicy, consistencyLevel);
            afterCreation?.Invoke(documentClient);
            services.AddSingleton<IDocumentClient>(documentClient);
            return documentClient;
        }
    }
}
