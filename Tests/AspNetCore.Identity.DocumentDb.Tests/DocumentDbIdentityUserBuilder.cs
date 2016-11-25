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

        public static DocumentDbIdentityUserBuilder Create(string userName)
        {
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
    }
}
