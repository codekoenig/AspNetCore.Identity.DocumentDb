using AspNetCore.Identity.DocumentDb.Stores;
using AspNetCore.Identity.DocumentDb.Tests.Fixtures;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    public abstract class StoreTestsBase : IClassFixture<DocumentDbFixture>
    {
        protected DocumentDbFixture documentDbFixture;
        protected Uri collectionUri;

        protected StoreTestsBase(DocumentDbFixture documentDbFixture)
        {
            this.documentDbFixture = documentDbFixture;
        }

        protected void CreateDocument(object document)
        {
            Document doc = this.documentDbFixture.Client.CreateDocumentAsync(collectionUri, document).Result;
        }

        protected DocumentDbUserStore<DocumentDbIdentityUser<DocumentDbIdentityRole>, DocumentDbIdentityRole> CreateUserStore()
        {
            IOptions<DocumentDbOptions> documentDbOptions = Options.Create(new DocumentDbOptions()
            {
                Database = documentDbFixture.Database,
                UserStoreDocumentCollection = documentDbFixture.UserStoreDocumentCollection,
                RoleStoreDocumentCollection = documentDbFixture.RoleStoreDocumentCollection
            });

            return new DocumentDbUserStore<DocumentDbIdentityUser<DocumentDbIdentityRole>, DocumentDbIdentityRole>(
                documentClient: documentDbFixture.Client,
                options: documentDbOptions,
                normalizer: documentDbFixture.Normalizer,
                roleStore: new DocumentDbRoleStore<DocumentDbIdentityRole>(
                    documentClient: documentDbFixture.Client,
                    options: documentDbOptions,
                    normalizer: documentDbFixture.Normalizer)
                );
        }

        protected DocumentDbRoleStore<DocumentDbIdentityRole> CreateRoleStore()
        {
            return new DocumentDbRoleStore<DocumentDbIdentityRole>(
                documentClient: documentDbFixture.Client,
                options: Options.Create(new DocumentDbOptions()
                {
                    Database = documentDbFixture.Database,
                    UserStoreDocumentCollection = documentDbFixture.UserStoreDocumentCollection,
                    RoleStoreDocumentCollection = documentDbFixture.RoleStoreDocumentCollection
                }),
                normalizer: documentDbFixture.Normalizer);
        }
    }
}
