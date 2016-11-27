using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    public class DocumentDbIdentityRoleBuilder
    {
        protected DocumentDbIdentityRole identityRole;

        public DocumentDbIdentityRoleBuilder(DocumentDbIdentityRole identityRole)
        {
            this.identityRole = identityRole;
        }

        public static implicit operator DocumentDbIdentityRole(DocumentDbIdentityRoleBuilder builder)
        {
            return builder.identityRole;
        }

        public static DocumentDbIdentityRoleBuilder Create(string roleName = null)
        {
            if (roleName == null)
            {
                roleName = Guid.NewGuid().ToString();
            }

            return new DocumentDbIdentityRoleBuilder(new DocumentDbIdentityRole()
            {
                Name = roleName
            });
        }

        public DocumentDbIdentityRoleBuilder WithId(string id = null)
        {
            identityRole.Id = id ?? Guid.NewGuid().ToString();
            return this;
        }

        public DocumentDbIdentityRoleBuilder WithNormalizedRoleName(string normalizedRoleName = null)
        {
            LookupNormalizer normalizer = new LookupNormalizer();

            identityRole.NormalizedName = normalizedRoleName ?? normalizer.Normalize(identityRole.Name);
            return this;
        }
    }
}
