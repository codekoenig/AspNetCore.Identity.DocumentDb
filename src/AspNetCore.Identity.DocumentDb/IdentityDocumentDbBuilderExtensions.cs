using AspNetCore.Identity.DocumentDb.Stores;
using AspNetCore.Identity.DocumentDb.Tools;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb
{
    public static class IdentityDocumentDbBuilderExtensions
    {
        public static IdentityBuilder AddDocumentDbStores(this IdentityBuilder builder)
        {
            return builder.AddDocumentDbStores(setupAction: null);
        }

        public static IdentityBuilder AddDocumentDbStores(this IdentityBuilder builder, Action<DocumentDbOptions> setupAction)
        {
            // TODO: Until DocumentDB SDK exposes it's JSON.NET settings, we need to hijack the global settings to serialize claims
            JsonConvert.DefaultSettings = () =>
            {
                return new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() { new JsonClaimConverter(), new JsonClaimsPrincipalConverter(), new JsonClaimsIdentityConverter() }
                };
            };

            if (setupAction != null)
            {
                builder.Services.Configure(setupAction);
            }

            builder.Services.AddSingleton(
                typeof(IRoleStore<>).MakeGenericType(builder.RoleType),
                typeof(DocumentDbRoleStore<>).MakeGenericType(builder.RoleType));

            builder.Services.AddSingleton(
                typeof(IUserStore<>).MakeGenericType(builder.UserType),
                typeof(DocumentDbUserStore<,>).MakeGenericType(builder.UserType, builder.RoleType));

            builder.Services.AddTransient<ILookupNormalizer, LookupNormalizer>();

            return builder;
        }
    }
}
