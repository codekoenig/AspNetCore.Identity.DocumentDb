using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb.Models
{
    public class UserEmail
    {
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public bool Confirmed { get; set; }
    }
}
