using AspNetCore.Identity.DocumentDb.Stores;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    public class UserStoreTests : IClassFixture<DocumentDbFixture>
    {
        private DocumentDbFixture documentDbFixture;
        private List<Document> createdDocuments = new List<Document>();

        public UserStoreTests(DocumentDbFixture documentDbFixture)
        {
            this.documentDbFixture = documentDbFixture;
        }

        [Fact]
        public void ShouldReturnNormalizedUserName()
        {
            DocumentDbIdentityUser user = DocumentDbIdentityUserBuilder.Create("test@test.at").WithNormalizedUserName();
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();

            string normalizedUserName = "normalized@test.at";

            store.SetNormalizedUserNameAsync(user, normalizedUserName, CancellationToken.None);

            Assert.Equal(normalizedUserName, user.NormalizedUserName);
        }

        [Fact]
        public void ShouldReturnAllUsersWithAdminRoleClaim()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();

            string firstAdminEmail = "adminabcdefg@test.at";
            string secondAdminEmail = "admin1234567@test.at";
            string thirdAdminEmail = "admin349jfja@test.at";

            CreateUser(DocumentDbIdentityUserBuilder.Create(firstAdminEmail).WithAdminRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create(secondAdminEmail).WithAdminRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create("userasdfasdf@test.at").WithUserRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create("user435fdfgg@test.at").WithUserRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create("usergfdghdfg@test.at").WithUserRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create("usernggdfs66@test.at").WithUserRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create("userbklasdkj@test.at").WithUserRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create(thirdAdminEmail).WithAdminRoleClaim());

            IList<DocumentDbIdentityUser> adminUsers = store.GetUsersForClaimAsync(new Claim(ClaimTypes.Role, "Admin"), CancellationToken.None).Result;
            
            Assert.Collection(
                adminUsers,
                u => u.Email.Equals(firstAdminEmail),
                u => u.Email.Equals(secondAdminEmail),
                u => u.Email.Equals(thirdAdminEmail));

            CleanupUsers();
        }

        private DocumentDbUserStore<DocumentDbIdentityUser> InitializeDocumentDbUserStore()
        {
            return new DocumentDbUserStore<DocumentDbIdentityUser>(
                documentClient: documentDbFixture.Client,
                options: Options.Create(new DocumentDbOptions()
                {
                    Database = documentDbFixture.Database,
                    UserStoreDocumentCollection = documentDbFixture.UserStoreDocumentCollection,
                    RoleStoreDocumentCollection = documentDbFixture.RoleStoreDocumentCollection
                }),
                normalizer: documentDbFixture.Normalizer);
        }

        private void CreateUser(DocumentDbIdentityUser user)
        {
            Document doc = this.documentDbFixture.Client.CreateDocumentAsync(this.documentDbFixture.UserStoreCollectionLink, user).Result;
            createdDocuments.Add(doc);
        }

        private void CleanupUsers()
        {
            foreach (Document createdDocument in this.createdDocuments)
            {
                var response = this.documentDbFixture.Client.DeleteDocumentAsync(createdDocument.SelfLink);
            }

            createdDocuments.Clear();
        }
    }
}
