using AspNetCore.Identity.DocumentDb.Stores;
using AspNetCore.Identity.DocumentDb.Tests.Comparer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    [Collection("DocumentDbCollection")]
    public class RoleStoreTests : StoreTestsBase
    {
        public RoleStoreTests(DocumentDbFixture documentDbFixture)
            : base(documentDbFixture)
        {
            this.collectionUri = UriFactory.CreateDocumentCollectionUri(this.documentDbFixture.Database, this.documentDbFixture.RoleStoreDocumentCollection);
        }

        [Fact]
        public void ShouldCreateNewRoleInDatabase()
        {
            DocumentDbIdentityRole newRole = DocumentDbIdentityRoleBuilder.Create().WithId();
            DocumentDbRoleStore<DocumentDbIdentityRole> store = InitializeDocumentDbRoleStore();

            // Create the new role
            IdentityResult result = store.CreateAsync(newRole, CancellationToken.None).Result;

            // Get it again from the DB to check if it was created correctly
            DocumentDbIdentityRole queriedRole = store.FindByIdAsync(newRole.Id, CancellationToken.None).Result;

            Assert.True(result.Succeeded);
            Assert.Equal(queriedRole, newRole, new DocumentDbIdentityRoleComparer());
        }

        [Fact]
        public void ShouldUpdateExistingRoleInDatabase()
        {
            DocumentDbRoleStore<DocumentDbIdentityRole> store = InitializeDocumentDbRoleStore();
            DocumentDbIdentityRole existingRole = DocumentDbIdentityRoleBuilder.Create().WithId();

            // Create sample data in DB
            CreateDocument(existingRole);

            // Change property to upate on sample data and call the update mehtod
            existingRole.Name = Guid.NewGuid().ToString();
            IdentityResult result = store.UpdateAsync(existingRole, CancellationToken.None).Result;

            // Get it again from the DB to check if it was created correctly
            DocumentDbIdentityRole queriedRole = store.FindByIdAsync(existingRole.Id, CancellationToken.None).Result;

            Assert.True(result.Succeeded);
            Assert.Equal(existingRole, queriedRole, new DocumentDbIdentityRoleComparer());
        }

        private DocumentDbRoleStore<DocumentDbIdentityRole> InitializeDocumentDbRoleStore()
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
