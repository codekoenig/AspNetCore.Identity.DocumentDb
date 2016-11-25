using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using AspNetCore.Identity.DocumentDb.Tools;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    public class DocumentDbFixture : IDisposable
    {
        public DocumentClient Client { get; private set; }
        public ILookupNormalizer Normalizer { get; private set; }
        public string Database { get; } = "AspNetCore.Identity.DocumentDb.Tests";
        public string UserStoreDocumentCollection { get; private set; } = "AspNetCore.Identity";
        public string RoleStoreDocumentCollection { get; private set; } = "AspNetCore.Identity";
        public Uri DatabaseLink { get; private set; }
        public Uri UserStoreCollectionLink { get; private set; }
        public Uri RoleStoreCollectionLink { get; private set; }

        public DocumentDbFixture()
        {
            // TODO: Until DocumentDB SDK exposes it's JSON.NET settings, we need to hijack the global settings to serialize claims
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() { new JsonClaimConverter() }
                };
            };

            Client = new DocumentClient(
                serviceEndpoint: new Uri("https://localhost:8081", UriKind.Absolute),
                authKeyOrResourceToken: "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                connectionPolicy: new ConnectionPolicy() { EnableEndpointDiscovery = false });

            Normalizer = new LookupNormalizer();

            DatabaseLink = UriFactory.CreateDatabaseUri(this.Database);
            UserStoreCollectionLink = UriFactory.CreateDocumentCollectionUri(this.Database, this.UserStoreDocumentCollection);
            RoleStoreCollectionLink = UriFactory.CreateDocumentCollectionUri(this.Database, this.RoleStoreDocumentCollection);

            CreateTestDatabase();
        }

        private void CreateTestDatabase()
        {
            Database db = this.Client.CreateDatabaseAsync(new Database() { Id = this.Database }).Result;

            DocumentCollection userCollection = this.Client.CreateDocumentCollectionAsync(
                DatabaseLink,
                new DocumentCollection() { Id = this.UserStoreDocumentCollection }).Result;

            if (this.UserStoreDocumentCollection != this.RoleStoreDocumentCollection)
            {
                DocumentCollection roleCollection = this.Client.CreateDocumentCollectionAsync(
                    DatabaseLink,
                    new DocumentCollection() { Id = this.RoleStoreDocumentCollection }).Result;
            }
        }

        private void CleanupTestDatabase()
        {
            this.Client.DeleteDatabaseAsync(DatabaseLink);
        }

        public void Dispose()
        {
            CleanupTestDatabase();
        }
    }
}
