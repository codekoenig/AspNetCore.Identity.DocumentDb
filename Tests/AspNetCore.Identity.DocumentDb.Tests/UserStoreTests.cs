using AspNetCore.Identity.DocumentDb.Stores;
using AspNetCore.Identity.DocumentDb.Tests.Builder;
using AspNetCore.Identity.DocumentDb.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
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
    [Collection("DocumentDbCollection")]
    public class UserStoreTests : StoreTestsBase
    {
        public UserStoreTests(DocumentDbFixture documentDbFixture) 
            : base(documentDbFixture)
        {
            this.collectionUri = UriFactory.CreateDocumentCollectionUri(this.documentDbFixture.Database, this.documentDbFixture.UserStoreDocumentCollection);
        }

        [Fact]
        public async Task ShouldSetNormalizedUserName()
        {
            DocumentDbIdentityUser user = DocumentDbIdentityUserBuilder.Create().WithNormalizedUserName();
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();

            string normalizedUserName = Guid.NewGuid().ToString();
            await store.SetNormalizedUserNameAsync(user, normalizedUserName, CancellationToken.None);

            Assert.Equal(normalizedUserName, user.NormalizedUserName);
        }

        [Fact]
        public async Task ShouldReturnAllUsersWithAdminRoleClaim()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();

            string adminRoleValue = Guid.NewGuid().ToString();

            DocumentDbIdentityUser firstAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddClaim(ClaimTypes.Role, adminRoleValue).AddClaim();
            DocumentDbIdentityUser secondAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddClaim(ClaimTypes.Role, adminRoleValue).AddClaim().AddClaim();
            DocumentDbIdentityUser thirdAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddClaim(ClaimTypes.Role, adminRoleValue);

            CreateDocument(firstAdmin);
            CreateDocument(secondAdmin);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddClaim().AddClaim());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddClaim().AddClaim().AddClaim());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddClaim().AddClaim());
            CreateDocument(thirdAdmin);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddClaim().AddClaim().AddClaim().AddClaim());

            IList<DocumentDbIdentityUser> adminUsers = await store.GetUsersForClaimAsync(new Claim(ClaimTypes.Role, adminRoleValue), CancellationToken.None);
            
            Assert.Collection(
                adminUsers,
                u => u.Id.Equals(firstAdmin.Id),
                u => u.Id.Equals(secondAdmin.Id),
                u => u.Id.Equals(thirdAdmin.Id));
        }

        [Fact]
        public async Task ShouldReturnUserByLoginProvider()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 3);
            UserLoginInfo targetLogin = targetUser.Logins[1];

            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 2));
            CreateDocument(targetUser);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 2));

            DocumentDbIdentityUser foundUser = await store.FindByLoginAsync(targetLogin.LoginProvider, targetLogin.ProviderKey, CancellationToken.None);

            Assert.Equal(targetUser.Id, foundUser.Id);
        }

        [Fact]
        public async Task ShouldReturnUserIsInRole()
        {
            DocumentDbIdentityRole role = DocumentDbIdentityRoleBuilder.Create();

            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser user = DocumentDbIdentityUserBuilder.Create().WithId().AddRole(role).AddRole().AddRole();

            bool result = await store.IsInRoleAsync(user, role.Name, CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task ShouldReturnUserIsNotInRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser user = DocumentDbIdentityUserBuilder.Create().WithId().AddRole().AddRole();

            bool result = await store.IsInRoleAsync(user, Guid.NewGuid().ToString(), CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task ShouldReturnAllUsersWithAdminRole()
        {
            DocumentDbIdentityRole role = DocumentDbIdentityRoleBuilder.Create();

            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();

            DocumentDbIdentityUser firstAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddRole(role).AddRole();
            DocumentDbIdentityUser secondAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddRole(role).AddRole().AddRole();
            DocumentDbIdentityUser thirdAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddRole(role);

            CreateDocument(firstAdmin);
            CreateDocument(secondAdmin);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddRole().AddRole());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddRole().AddRole().AddRole());
            CreateDocument(thirdAdmin);
            CreateDocument(DocumentDbIdentityUserBuilder.Create());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddRole());

            IList<DocumentDbIdentityUser> adminUsers = await store.GetUsersInRoleAsync(role.Name, CancellationToken.None);

            Assert.Collection(
                adminUsers,
                u => u.Id.Equals(firstAdmin.Id),
                u => u.Id.Equals(secondAdmin.Id),
                u => u.Id.Equals(thirdAdmin.Id));
        }

        [Fact]
        public async Task ShouldReturnUserBySpecificEmail()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedEmail();

            CreateDocument(DocumentDbIdentityUserBuilder.Create());
            CreateDocument(targetUser);
            CreateDocument(DocumentDbIdentityUserBuilder.Create());
            CreateDocument(DocumentDbIdentityUserBuilder.Create());

            DocumentDbIdentityUser foundUser = await store.FindByEmailAsync(targetUser.NormalizedEmail, CancellationToken.None);

            Assert.Equal(targetUser.Id, foundUser.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public async Task ShouldIncreaseAccessFailedCountBy1(int accessFailedCount)
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithAccessFailedCountOf(accessFailedCount);

            await store.IncrementAccessFailedCountAsync(targetUser, CancellationToken.None);

            Assert.Equal(++accessFailedCount, targetUser.AccessFailedCount);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(1)]
        [InlineData(0)]
        public async Task ShouldResetAccessFailedCountToZero(int accessFailedCount)
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = InitializeDocumentDbUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithAccessFailedCountOf(accessFailedCount);

            await store.ResetAccessFailedCountAsync(targetUser, CancellationToken.None);

            Assert.Equal(0, targetUser.AccessFailedCount);
        }

        private  DocumentDbUserStore<DocumentDbIdentityUser> InitializeDocumentDbUserStore()
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
    }
}
