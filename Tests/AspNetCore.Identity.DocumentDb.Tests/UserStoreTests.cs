using AspNetCore.Identity.DocumentDb.Stores;
using Microsoft.AspNetCore.Identity;
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

            DocumentDbIdentityUser firstAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithAdminRoleClaim();
            DocumentDbIdentityUser secondAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithAdminRoleClaim();
            DocumentDbIdentityUser thirdAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithAdminRoleClaim();

            CreateUser(firstAdmin);
            CreateUser(secondAdmin);
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRoleClaim());
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRoleClaim());
            CreateUser(thirdAdmin);
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRoleClaim());

            IList<DocumentDbIdentityUser> adminUsers = store.GetUsersForClaimAsync(new Claim(ClaimTypes.Role, "Admin"), CancellationToken.None).Result;
            
            Assert.Collection(
                adminUsers,
                u => u.Id.Equals(firstAdmin.Id),
                u => u.Id.Equals(secondAdmin.Id),
                u => u.Id.Equals(thirdAdmin.Id));

            CleanupUsers();
        }

        [Fact]
        public void ShouldReturnUserByLoginProvider()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 3);
            UserLoginInfo targetLogin = targetUser.Logins[1];

            CreateUser(DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 2));
            CreateUser(targetUser);
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 2));

            DocumentDbIdentityUser foundUser = store.FindByLoginAsync(targetLogin.LoginProvider, targetLogin.ProviderKey, CancellationToken.None).Result;

            Assert.Equal(targetUser.Id, foundUser.Id);

            CleanupUsers();
        }

        [Fact]
        public void ShouldReturnUserIsInRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser user = DocumentDbIdentityUserBuilder.Create().WithId().WithAdminRole();

            bool result = store.IsInRoleAsync(user, "Admin", CancellationToken.None).Result;

            Assert.True(result);
        }

        [Fact]
        public void ShouldReturnUserIsNotInRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser user = DocumentDbIdentityUserBuilder.Create().WithId().WithUserRole();

            bool result = store.IsInRoleAsync(user, "Admin", CancellationToken.None).Result;

            Assert.False(result);
        }

        [Fact]
        public void ShouldReturnAllUsersWithAdminRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();

            DocumentDbIdentityUser firstAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithAdminRole();
            DocumentDbIdentityUser secondAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithAdminRole();
            DocumentDbIdentityUser thirdAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithAdminRole();

            CreateUser(firstAdmin);
            CreateUser(secondAdmin);
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRole());
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRole());
            CreateUser(thirdAdmin);
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRole());
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRole());

            IList<DocumentDbIdentityUser> adminUsers = store.GetUsersInRoleAsync("Admin", CancellationToken.None).Result;

            Assert.Collection(
                adminUsers,
                u => u.Id.Equals(firstAdmin.Id),
                u => u.Id.Equals(secondAdmin.Id),
                u => u.Id.Equals(thirdAdmin.Id));

            CleanupUsers();
        }

        [Fact]
        public void ShouldReturnUserBySpecificEmail()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedEmail();

            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRole());
            CreateUser(targetUser);
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRole());
            CreateUser(DocumentDbIdentityUserBuilder.Create().WithUserRole());

            DocumentDbIdentityUser foundUser = store.FindByEmailAsync(targetUser.NormalizedEmail, CancellationToken.None).Result;

            Assert.Equal(targetUser.Id, foundUser.Id);

            CleanupUsers();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void ShouldIncreaseAccessFailedCountBy1(int accessFailedCount)
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithAccessFailedCountOf(accessFailedCount);

            store.IncrementAccessFailedCountAsync(targetUser, CancellationToken.None);

            Assert.Equal(++accessFailedCount, targetUser.AccessFailedCount);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(1)]
        [InlineData(0)]
        public void ShouldResetAccessFailedCountToZero(int accessFailedCount)
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithAccessFailedCountOf(accessFailedCount);

            store.ResetAccessFailedCountAsync(targetUser, CancellationToken.None);

            Assert.Equal(0, targetUser.AccessFailedCount);
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
