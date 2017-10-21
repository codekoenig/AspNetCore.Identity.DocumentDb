using AspNetCore.Identity.DocumentDb.Stores;
using AspNetCore.Identity.DocumentDb.Tests.Builder;
using AspNetCore.Identity.DocumentDb.Tests.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
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
            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();

            string normalizedUserName = Guid.NewGuid().ToString();
            await store.SetNormalizedUserNameAsync(user, normalizedUserName, CancellationToken.None);

            Assert.Equal(normalizedUserName, user.NormalizedUserName);
        }

        [Fact]
        public async Task ShouldReturnAllUsersWithAdminRoleClaim()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();

            string adminRoleValue = Guid.NewGuid().ToString();

            DocumentDbIdentityUser<DocumentDbIdentityRole> firstAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddClaim(ClaimTypes.Role, adminRoleValue).AddClaim();
            DocumentDbIdentityUser<DocumentDbIdentityRole> secondAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddClaim(ClaimTypes.Role, adminRoleValue).AddClaim().AddClaim();
            DocumentDbIdentityUser<DocumentDbIdentityRole> thirdAdmin = DocumentDbIdentityUserBuilder.Create().WithId().AddClaim(ClaimTypes.Role, adminRoleValue);

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
            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();
            DocumentDbIdentityUser<DocumentDbIdentityRole> targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 3);
            UserLoginInfo targetLogin = targetUser.Logins[1];

            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 2));
            CreateDocument(targetUser);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithUserLoginInfo(amount: 2));

            DocumentDbIdentityUser<DocumentDbIdentityRole> foundUser = await store.FindByLoginAsync(targetLogin.LoginProvider, targetLogin.ProviderKey, CancellationToken.None);

            Assert.Equal(targetUser.Id, foundUser.Id);
        }

        [Fact]
        public async Task ShouldReturnAllUsersWithAdminRole()
        {
            DocumentDbIdentityRole adminRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();

            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();

            DocumentDbIdentityUser<DocumentDbIdentityRole> firstAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName().AddRole(adminRole).AddRole();
            DocumentDbIdentityUser<DocumentDbIdentityRole> secondAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName().AddRole(adminRole).AddRole().AddRole();
            DocumentDbIdentityUser<DocumentDbIdentityRole> thirdAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName().AddRole(adminRole);

            CreateDocument(firstAdmin);
            CreateDocument(secondAdmin);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddRole().AddRole());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddRole().AddRole().AddRole());
            CreateDocument(thirdAdmin);
            CreateDocument(DocumentDbIdentityUserBuilder.Create());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddRole());

            IList<DocumentDbIdentityUser> adminUsers = await store.GetUsersInRoleAsync(adminRole.NormalizedName, CancellationToken.None);

            Assert.Collection(
                adminUsers,
                u => u.Id.Equals(firstAdmin.Id),
                u => u.Id.Equals(secondAdmin.Id),
                u => u.Id.Equals(thirdAdmin.Id));
        }

        [Fact]
        public async Task ShouldReturnNoUsersWithAdminRoleWhenPassingNotNormalizedRoleNameToGetUsersInRole()
        {
            DocumentDbIdentityRole adminRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();
            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();
            DocumentDbIdentityUser<DocumentDbIdentityRole> firstAdmin = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName().AddRole(adminRole).AddRole();

            CreateDocument(firstAdmin);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().AddRole().AddRole());

            IList<DocumentDbIdentityUser> adminUsers = await store.GetUsersInRoleAsync(adminRole.Name, CancellationToken.None);

            Assert.Empty(adminUsers);
        }

        [Fact]
        public async Task ShouldReturnUserBySpecificEmail()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();
            DocumentDbIdentityUser<DocumentDbIdentityRole> targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedEmail();

            CreateDocument(DocumentDbIdentityUserBuilder.Create());
            CreateDocument(targetUser);
            CreateDocument(DocumentDbIdentityUserBuilder.Create());
            CreateDocument(DocumentDbIdentityUserBuilder.Create());

            DocumentDbIdentityUser<DocumentDbIdentityRole> foundUser = await store.FindByEmailAsync(targetUser.NormalizedEmail, CancellationToken.None);

            Assert.Equal(targetUser.Id, foundUser.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public async Task ShouldIncreaseAccessFailedCountBy1(int accessFailedCount)
        {
            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();
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
            DocumentDbUserStore<DocumentDbIdentityUser> store = CreateUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithAccessFailedCountOf(accessFailedCount);

            await store.ResetAccessFailedCountAsync(targetUser, CancellationToken.None);

            Assert.Equal(0, targetUser.AccessFailedCount);
        }

        [Fact]
        public async Task ShouldAddUserToRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbRoleStore<DocumentDbIdentityRole> roleStore = CreateRoleStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create();
            DocumentDbIdentityRole targetRole = DocumentDbIdentityRoleBuilder.Create("RoleName").WithId().WithNormalizedRoleName();

            // Create sample data role
            await roleStore.CreateAsync(targetRole, CancellationToken.None);

            // Add the created sample data role to the user
            await userStore.AddToRoleAsync(targetUser, targetRole.NormalizedName, CancellationToken.None);

            Assert.Contains(targetUser.Roles, r => r.NormalizedName.Equals(targetRole.NormalizedName));
        }

        [Fact]
        public async Task ShouldThrowExceptionOnAddingUserToNonexistantRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbRoleStore<DocumentDbIdentityRole> roleStore = CreateRoleStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create();
            DocumentDbIdentityRole someNotTargetedRole = DocumentDbIdentityRoleBuilder.Create().WithId().WithNormalizedRoleName();

            // Create a role so there is a differently named role in the store
            await roleStore.CreateAsync(someNotTargetedRole, CancellationToken.None);
            
            // Add the user to a role name different than the role created before, expecting an exception
            await Assert.ThrowsAsync(typeof(ArgumentException), async () => await userStore.AddToRoleAsync(targetUser, "NotExistantRole", CancellationToken.None));
        }

        [Fact]
        public async Task ShouldThrowExceptionWhenPassingNotNormalizedNameToAddToRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbRoleStore<DocumentDbIdentityRole> roleStore = CreateRoleStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create();
            DocumentDbIdentityRole targetRole = DocumentDbIdentityRoleBuilder.Create().WithId().WithNormalizedRoleName();

            // Create sample data role
            await roleStore.CreateAsync(targetRole, CancellationToken.None);

            // Add the user to the created role, but pass the not normalized name, expecting an exception
            await Assert.ThrowsAsync(typeof(ArgumentException), async () => await userStore.AddToRoleAsync(targetUser, targetRole.Name, CancellationToken.None));
        }

        [Fact]
        public async Task ShouldRemoveRoleFromUser()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbIdentityRole firstRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();
            DocumentDbIdentityRole secondRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().AddRole(firstRole).AddRole(secondRole);

            // Remove the second role
            await userStore.RemoveFromRoleAsync(targetUser, secondRole.NormalizedName, CancellationToken.None);

            // Assert second role has been removed while first one is still there
            Assert.DoesNotContain(targetUser.Roles, r => r.NormalizedName.Equals(secondRole.NormalizedName));
            Assert.Contains(targetUser.Roles, r => r.NormalizedName.Equals(firstRole.NormalizedName));
        }

        [Fact]
        public async Task ShouldNotRemoveRoleFromUserWhenPassingNotNormalizedRoleNameToRemoveFromRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbIdentityRole firstRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();
            DocumentDbIdentityRole secondRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().AddRole(firstRole).AddRole(secondRole);

            // Try remove the second role with a not normalized role name
            await userStore.RemoveFromRoleAsync(targetUser, secondRole.Name, CancellationToken.None);

            // Assert both roles are still here, as lookup without normalized name should have failed
            Assert.Collection(targetUser.Roles, 
                r => r.NormalizedName.Equals(firstRole.NormalizedName), 
                r => r.NormalizedName.Equals(secondRole.NormalizedName));
        }

        [Fact]
        public async Task ShouldReturnUserIsInRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbIdentityRole targetRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().AddRole(targetRole).AddRole();

            bool isInRole = await userStore.IsInRoleAsync(targetUser, targetRole.NormalizedName, CancellationToken.None);

            Assert.True(isInRole);
        }

        [Fact]
        public async Task ShouldReturnUserIsNotInRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().AddRole().AddRole();

            bool isInRole = await userStore.IsInRoleAsync(targetUser, "NonExistantRoleName", CancellationToken.None);

            Assert.False(isInRole);
        }

        [Fact]
        public async Task ShouldReturnUserIsNotInRoleWhenPassingNotNormalizedRoleNameToIsInRole()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbIdentityRole targetRole = DocumentDbIdentityRoleBuilder.Create().WithNormalizedRoleName();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().AddRole(targetRole).AddRole();

            // Pass not normalized name which should lead to not locating the target role
            bool isInRole = await userStore.IsInRoleAsync(targetUser, targetRole.Name, CancellationToken.None);

            Assert.False(isInRole);
        }

        [Fact]
        public async Task ShouldReturnUserByUserName()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbIdentityUser targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName();

            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName());
            CreateDocument(targetUser);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedUserName());

            DocumentDbIdentityUser<DocumentDbIdentityRole> foundUser = await userStore.FindByNameAsync(targetUser.NormalizedUserName, CancellationToken.None);

            Assert.Equal(targetUser.Id, foundUser.Id);
        }

        [Fact]
        public async Task ShouldReturnUserByEmail()
        {
            DocumentDbUserStore<DocumentDbIdentityUser> userStore = CreateUserStore();
            DocumentDbIdentityUser<DocumentDbIdentityRole> targetUser = DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedEmail();

            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedEmail());
            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedEmail());
            CreateDocument(targetUser);
            CreateDocument(DocumentDbIdentityUserBuilder.Create().WithId().WithNormalizedEmail());

            DocumentDbIdentityUser<DocumentDbIdentityRole> foundUser = await userStore.FindByEmailAsync(targetUser.NormalizedEmail, CancellationToken.None);

            Assert.Equal(targetUser.Id, foundUser.Id);
        }
#if NETCORE2
        [Fact]
        public async Task ShouldSetAuthenticatorKeyAsync()
        {
            // Arrange
            var userStore = CreateUserStore();
            var user = (DocumentDbIdentityUser)DocumentDbIdentityUserBuilder.Create();
            var key = Guid.NewGuid().ToString();

            // Act
            await userStore.SetAuthenticatorKeyAsync(user, key, CancellationToken.None);

            // Assert
            Assert.Equal(key, user.AuthenticatorKey);
        }

        [Fact]
        public async Task ShouldGetAuthenticatorKeyAsync()
        {
            // Arrange
            var userStore = CreateUserStore();
            var user = (DocumentDbIdentityUser)DocumentDbIdentityUserBuilder.Create();
            var expectedKey = Guid.NewGuid().ToString();
            user.AuthenticatorKey = expectedKey;

            // Act
            var key = await userStore.GetAuthenticatorKeyAsync(user, CancellationToken.None);

            // Assert
            Assert.Equal(expectedKey, key);
        }

        [Fact]
        public async Task ShouldReplaceRecoveryCodes()
        {
            // Arrange
            var userStore = CreateUserStore();
            var user = (DocumentDbIdentityUser)DocumentDbIdentityUserBuilder.Create();
            var replacementCodes = new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            user.RecoveryCodes = new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            // Act
            await userStore.ReplaceCodesAsync(user, replacementCodes, CancellationToken.None);

            // Assert
            Assert.Equal(replacementCodes, user.RecoveryCodes);
        }

        [Fact]
        public async Task ShouldRemoveTheRedeemedCodeAndReturnTrue()
        {
            // Arrange
            var userStore = CreateUserStore();
            var user = (DocumentDbIdentityUser)DocumentDbIdentityUserBuilder.Create();
            var codeToRedeem = Guid.NewGuid().ToString();
            var expectedCode1 = Guid.NewGuid().ToString();
            var expectedCode2 = Guid.NewGuid().ToString();
            user.RecoveryCodes = new string[] { expectedCode1, codeToRedeem, expectedCode2 };

            // Act
            var result = await userStore.RedeemCodeAsync(user, codeToRedeem, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Collection(user.RecoveryCodes,
                code => Assert.Equal(expectedCode1, code),
                code => Assert.Equal(expectedCode2, code)
            );
        }

        [Fact]
        public async Task ShouldReturnFalseIfRecoveryCodeDoesNotExistsInTheCollection()
        {
            // Arrange
            var userStore = CreateUserStore();
            var user = (DocumentDbIdentityUser)DocumentDbIdentityUserBuilder.Create();
            var codeToRedeem = Guid.NewGuid().ToString();
            var expectedCode1 = Guid.NewGuid().ToString();
            var expectedCode2 = Guid.NewGuid().ToString();
            user.RecoveryCodes = new string[] { expectedCode1, expectedCode2 };

            // Act
            var result = await userStore.RedeemCodeAsync(user, codeToRedeem, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShouldReturnRecoveryCodesCount()
        {
            // Arrange
            var userStore = CreateUserStore();
            var user = (DocumentDbIdentityUser)DocumentDbIdentityUserBuilder.Create();
            user.RecoveryCodes = new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            var expectedCount = 3;

            // Act
            var result = await userStore.CountCodesAsync(user, CancellationToken.None);

            // Assert
            Assert.Equal(expectedCount, result);
        }
#endif
    }
}
