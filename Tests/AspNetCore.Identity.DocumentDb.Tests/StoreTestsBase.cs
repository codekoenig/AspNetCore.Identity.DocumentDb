using AspNetCore.Identity.DocumentDb.Stores;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    public abstract class StoreTestsBase : IClassFixture<DocumentDbFixture>
    {
        protected DocumentDbFixture documentDbFixture;
        protected Uri collectionUri;

        protected StoreTestsBase(DocumentDbFixture documentDbFixture)
        {
            this.documentDbFixture = documentDbFixture;
        }

        protected void CreateDocument(object document)
        {
            Document doc = this.documentDbFixture.Client.CreateDocumentAsync(collectionUri, document).Result;
        }
    }
}
