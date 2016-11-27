using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace AspNetCore.Identity.DocumentDb
{
    public class DocumentDbIdentityRole : DocumentBase
    {
        public DocumentDbIdentityRole()
        {
            this.Claims = new List<Claim>();
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "normalizedName")]
        public string NormalizedName { get; set; }

        [JsonProperty(PropertyName = "claims")]
        public IList<Claim> Claims { get; set; }
    }
}
