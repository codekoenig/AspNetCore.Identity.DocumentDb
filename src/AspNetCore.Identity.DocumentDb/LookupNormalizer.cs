using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb
{
    public class LookupNormalizer : ILookupNormalizer
    {
#if NETSTANDARD21
        public string NormalizeName(string key)
        {
            return key.Normalize().ToLowerInvariant();
        }

        public string NormalizeEmail(string key)
        {
            return key.Normalize().ToLowerInvariant();
        }
#endif

        public string Normalize(string key)
        {
            return key.Normalize().ToLowerInvariant();
        }
    }
}
