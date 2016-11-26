using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    public class DocumentDbIdentityUserBuilder
    {
        protected DocumentDbIdentityUser identityUser;

        public DocumentDbIdentityUserBuilder(DocumentDbIdentityUser identityUser)
        {
            this.identityUser = identityUser;
        }

        public static implicit operator DocumentDbIdentityUser(DocumentDbIdentityUserBuilder builder)
        {
            return builder.identityUser;
        }

        public static DocumentDbIdentityUserBuilder Create(string userName = null)
        {
            string email = userName;

            if (userName == null)
            {
                userName = Guid.NewGuid().ToString();
                email = userName + "@test.at";
            }

            return new DocumentDbIdentityUserBuilder(new DocumentDbIdentityUser()
            {
                UserName = userName,
                Email = userName
            });
        }

        public DocumentDbIdentityUserBuilder WithId(string id = null)
        {
            identityUser.Id = id ?? Guid.NewGuid().ToString();
            return this;
        }

        public DocumentDbIdentityUserBuilder WithNormalizedUserName(string normalizedUserName = null)
        {
            identityUser.NormalizedUserName = normalizedUserName ?? "unittestuser@test.at";
            return this;
        }

        public DocumentDbIdentityUserBuilder IsLockedOut(DateTime? until = null)
        {
            identityUser.LockoutEnabled = true;
            identityUser.LockoutEndDate = until.HasValue ? until.Value : DateTime.Now.AddMinutes(10);
            return this;
        }

        public DocumentDbIdentityUserBuilder IsNotLockedOut()
        {
            identityUser.LockoutEnabled = false;
            identityUser.LockoutEndDate = null;
            return this;
        }

        public DocumentDbIdentityUserBuilder WithClaims(List<Claim> claims = null)
        {
            if (claims == null || !claims.Any())
            {
                claims = new List<Claim>();

                claims.Add(new Claim(ClaimTypes.Email, identityUser.Email));
                claims.Add(new Claim(ClaimTypes.Gender, "Female"));
                claims.Add(new Claim(ClaimTypes.Country, "USA"));
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            identityUser.Claims = claims;
            return this;
        }

        public DocumentDbIdentityUserBuilder WithUserRoleClaim()
        {
            return WithClaims(null);
        }

        public DocumentDbIdentityUserBuilder WithAdminRoleClaim()
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Email, identityUser.Email));
            claims.Add(new Claim(ClaimTypes.Gender, "Male"));
            claims.Add(new Claim(ClaimTypes.Country, "Austria"));
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            identityUser.Claims = claims;
            return this;
        }

        public DocumentDbIdentityUserBuilder WithUserRole()
        {
            List<DocumentDbIdentityRole> roles = new List<DocumentDbIdentityRole>();

            roles.Add(new DocumentDbIdentityRole() { Name = "User" });
            roles.Add(new DocumentDbIdentityRole() { Name = "Generic" });

            identityUser.Roles = roles;
            return this;
        }

        public DocumentDbIdentityUserBuilder WithAdminRole()
        {
            List<DocumentDbIdentityRole> roles = new List<DocumentDbIdentityRole>();

            roles.Add(new DocumentDbIdentityRole() { Name = "Admin" });
            roles.Add(new DocumentDbIdentityRole() { Name = "Generic" });

            identityUser.Roles = roles;
            return this;
        }

        public DocumentDbIdentityUserBuilder WithNormalizedEmail()
        {
            LookupNormalizer normalizer = new LookupNormalizer();

            identityUser.NormalizedEmail =  normalizer.Normalize(identityUser.Email);
            return this;
        }

        public DocumentDbIdentityUserBuilder WithUserLoginInfo(int amount = 1)
        {
            List<UserLoginInfo> logins = new List<UserLoginInfo>();

            for (int i = 0; i < amount; i++)
            {
                logins.Add(new UserLoginInfo(
                    Guid.NewGuid().ToString(), 
                    Guid.NewGuid().ToString(), 
                    Guid.NewGuid().ToString()));
            }

            this.identityUser.Logins = logins;
            return this;
        }

        public DocumentDbIdentityUserBuilder WithAccessFailedCountOf(int count)
        {
            this.identityUser.AccessFailedCount = count;
            return this;
        }
    }
}
