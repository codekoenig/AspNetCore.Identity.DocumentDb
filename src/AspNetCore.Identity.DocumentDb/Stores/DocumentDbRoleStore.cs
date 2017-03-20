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
    /// <summary>
    /// Represents a DocumentDb-based persistence store for ASP.NET Core Identity roles
    /// </summary>
    /// <typeparam name="TRole">The type representing a role</typeparam>
    public class DocumentDbRoleStore<TRole> : StoreBase, IRoleClaimStore<TRole>
        where TRole : DocumentDbIdentityRole
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDbRoleStore{TRole}"/>
        /// </summary>
        /// <param name="documentClient">The DocumentDb client to be used</param>
        /// <param name="options">The configuraiton options for the <see cref="IDocumentClient"/></param>
        public DocumentDbRoleStore(IDocumentClient documentClient, IOptions<DocumentDbOptions> options)
            : base(documentClient, options, options.Value.RoleStoreDocumentCollection ?? options.Value.UserStoreDocumentCollection)
        {
            collectionUri = UriFactory.CreateDocumentCollectionUri(
                this.options.Database, 
                this.options.RoleStoreDocumentCollection ?? this.options.UserStoreDocumentCollection);
        }

        public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Claims);
        }

        public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            role.Claims.Add(claim);

            return Task.CompletedTask;
        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            role.Claims.Remove(claim);

            return Task.CompletedTask;
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

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            ResourceResponse<Document> result;

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            try
            {
                result = await documentClient.DeleteDocumentAsync(GenerateDocumentUri(role.Id));
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

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (roleName == null)
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            role.Name = roleName;

            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (normalizedName == null)
            {
                throw new ArgumentNullException(nameof(normalizedName));
            }

            role.NormalizedName = normalizedName;

            return Task.CompletedTask;
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
                .Where(r => r.NormalizedName == normalizedRoleName && r.DocumentType == typeof(TRole).Name)
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
