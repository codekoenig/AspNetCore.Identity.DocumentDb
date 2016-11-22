using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb
{
    public class DocumentDbIdentityUser : DocumentDbIdentityDocument
    {
        public DocumentDbIdentityUser()
        {
            this.Roles = new List<DocumentDbIdentityRole>();
            this.Logins = new List<UserLoginInfo>();
            this.Claims = new List<Claim>();
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "normalizedUserName")]
        public string NormalizedUserName { get; set; }

        [JsonProperty(PropertyName = "normalizedEmail")]
        public string NormalizedEmail { get; set; }

        [JsonProperty(PropertyName = "isEmailConfirmed")]
        public bool IsEmailConfirmed { get; set; }

        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; internal set; }

        [JsonProperty(PropertyName = "isPhoneNumberConfirmed")]
        public bool IsPhoneNumberConfirmed { get; internal set; }

        [JsonProperty(PropertyName = "passwordHash")]
        public string PasswordHash { get; set; }

        [JsonProperty(PropertyName = "securityStamp")]
        public string SecurityStamp { get; set; }

        [JsonProperty(PropertyName = "isTwoFactorAuthEnabled")]
        public bool IsTwoFactorAuthEnabled { get; set; }

        [JsonProperty(PropertyName = "logins")]
        public IList<UserLoginInfo> Logins { get; set; }

        [JsonProperty(PropertyName = "roles")]
        public IList<DocumentDbIdentityRole> Roles { get; set; }

        [JsonProperty(PropertyName = "claims")]
        public IList<Claim> Claims { get; set; }

        [JsonProperty(PropertyName = "lockoutEnabled")]
        public bool LockoutEnabled { get; set; }

        [JsonProperty(PropertyName = "lockoutEndDate")]
        public DateTimeOffset? LockoutEndDate { get; set; }

        [JsonProperty(PropertyName = "accessFailedCount")]
        public int AccessFailedCount { get; set; }
    }
}
