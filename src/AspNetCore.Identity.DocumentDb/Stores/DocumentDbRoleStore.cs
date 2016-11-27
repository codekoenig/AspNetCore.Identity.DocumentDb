using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Documents;
using System.Security.Claims;
using System.Net;
using AspNetCore.Identity.DocumentDb.Tools;

namespace AspNetCore.Identity.DocumentDb.Stores
{
    public class DocumentDbRoleStore<TRole> : StoreBase, IRoleClaimStore<TRole>
        where TRole : DocumentDbIdentityRole
    {
        public DocumentDbRoleStore(DocumentClient documentClient, IOptions<DocumentDbOptions> options, ILookupNormalizer normalizer) 
            : base(documentClient, options, normalizer, options.Value.RoleStoreDocumentCollection)
        {
            collectionUri = UriFactory.CreateDocumentCollectionUri(this.options.Database, this.options.RoleStoreDocumentCollection);
        }

        public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            // If no RoleId was specified, generate one
            if (role.Id == null)
            {
                role.Id = Guid.NewGuid().ToString();
            }

            ResourceResponse<Document> result = await documentClient.CreateDocumentAsync(collectionUri, role);

            return result.StatusCode == HttpStatusCode.Created
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError() { Code = result.StatusCode.ToString() });
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            try
            {
                ResourceResponse<Document> result = await documentClient.ReplaceDocumentAsync(GenerateDocumentUri(role.Id), document: role);
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    return IdentityResult.Failed();
                }

                throw;
            }

            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (roleId == null)
            {
                throw new ArgumentNullException(nameof(roleId));
            }

            TRole role = await documentClient.ReadDocumentAsync<TRole>(GenerateDocumentUri(roleId));

            return role;
        }

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (normalizedRoleName == null)
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            TRole role = documentClient.CreateDocumentQuery<TRole>(collectionUri)
                .Where(r => r.NormalizedName == normalizedRoleName)
                .AsEnumerable()
                .FirstOrDefault();

            return Task.FromResult(role);
        }

        #region IDisposable Support

        public void Dispose()
        {
            // TODO: Workaround, gets disposed too early currently
            disposed = false;
        }

        #endregion
    }
}
