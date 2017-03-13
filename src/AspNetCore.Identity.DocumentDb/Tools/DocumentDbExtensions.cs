using Microsoft.Azure.Documents.Client;
using AspNetCore.Identity.DocumentDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System.Net;

namespace AspNetCore.Identity.DocumentDb.Tools
{
    /// <summary>
    /// Various extension methods to be used with DocumentDb SDK
    /// </summary>
    internal static class DocumentDbExtensions
    {
        /// <summary>
        /// Generic version of ReadDocumentAsync that returns the queried document deserialized into the given generic type
        /// </summary>
        /// <typeparam name="T">The Type of the Document to read from DocumentDb</typeparam>
        /// <param name="client">The <see cref="IDocumentClient"/> this extension method is executed on</param>
        /// <param name="documentUri">The <see cref="Uri"/> of the document to read from DocumentDB</param>
        /// <returns>The Document read from DocumentDB, deserialized into the given generic Type</returns>
        internal static async Task<T> ReadDocumentAsync<T>(this IDocumentClient client, Uri documentUri)
        {
            return await client.ReadDocumentAsync<T>(documentUri, null);
        }

        /// <summary>
        /// Generic version of ReadDocumentAsync that returns the queried document deserialized into the given generic type
        /// </summary>
        /// <typeparam name="T">The Type of the Document to read from DocumentDb</typeparam>
        /// <param name="client">The <see cref="IDocumentClient"/> this extension method is executed on</param>
        /// <param name="documentUri">The <see cref="Uri"/> of the document to read from DocumentDB</param>
        /// <param name="requestOptions">The <see cref="RequestOptions"/> used for calling DocumentDB</param>
        /// <returns>The Document read from DocumentDB, deserialized into the given generic Type</returns>
        internal static async Task<T> ReadDocumentAsync<T>(this IDocumentClient client, Uri documentUri, RequestOptions requestOptions)
        {
            ResourceResponse<Document> response;

            if (documentUri == null)
            {
                throw new ArgumentNullException(nameof(documentUri));
            }

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
