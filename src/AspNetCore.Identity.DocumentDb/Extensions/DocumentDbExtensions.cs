using Microsoft.Azure.Documents.Client;
using AspNetCore.Identity.DocumentDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System.Net;

namespace AspNetCore.Identity.DocumentDb.Extensions
{
    /// <summary>
    /// Various extension methods to be used with DocumentDb SDK
    /// </summary>
    internal static class DocumentDbExtensions
    {
        internal static async Task<T> ReadDocumentAsync<T>(this DocumentClient client, string documentId, DocumentDbOptions options)
        {
            ResourceResponse<Document> response;

            if (documentId == null)
            {
                throw new ArgumentNullException(nameof(documentId));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            RequestOptions requestOptions = options.PartitionKey != null
                ? new RequestOptions() { PartitionKey = new PartitionKey(options.PartitionKey) }
                : null;

            Uri documentUri = UriFactory.CreateDocumentUri(options.Database, options.DocumentCollection, documentId);

            try
            {
                response = await client.ReadDocumentAsync(documentUri, requestOptions);
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    return default(T);
                }

                throw;
            }

            return JsonConvert.DeserializeObject<T>(response.Resource.ToString());
        }
    }
}
