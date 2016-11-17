using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb
{
    public class DocumentDbIdentityUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }
}
