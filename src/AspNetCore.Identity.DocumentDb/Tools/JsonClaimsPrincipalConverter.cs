using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetCore.Identity.DocumentDb.Tools 
{
    public class JsonClaimsPrincipalConverter : JsonConverter 
    {
        public override bool CanConvert(Type objectType) 
        {
            return typeof(ClaimsPrincipal) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) 
        {
            var jObject = JObject.Load(reader);
            var identities = jObject[nameof(ClaimsPrincipal.Identities)].ToObject<IEnumerable<ClaimsIdentity>>(serializer);
            return new ClaimsPrincipal(identities);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) 
        {
            var claimsPrincipal = (ClaimsPrincipal)value;
            var jObject = new JObject 
            {
                { nameof(ClaimsPrincipal.Identities), new JArray(claimsPrincipal.Identities.Select(x => JObject.FromObject(x, serializer))) },
            };

            jObject.WriteTo(writer);
        }
    }
}
