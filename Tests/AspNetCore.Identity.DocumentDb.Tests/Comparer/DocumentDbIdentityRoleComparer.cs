using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb.Tests.Comparer
{
    public class DocumentDbIdentityRoleComparer : IEqualityComparer<DocumentDbIdentityRole>
    {
        public bool Equals(DocumentDbIdentityRole x, DocumentDbIdentityRole y)
        {
            return x.Id.Equals(y.Id)
                && string.Equals(x.Name, y.Name)
                && string.Equals(x.NormalizedName, y.NormalizedName)
                && x.DocumentType.Equals(y.DocumentType);
        }

        public int GetHashCode(DocumentDbIdentityRole obj)
        {
            return 1;
        }
    }
}
