using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCore.Identity.DocumentDb.Tests
{
    [CollectionDefinition("DocumentDbCollection")]
    public class DocumentDbFixtureCollection : ICollectionFixture<DocumentDbFixture>
    {
    }
}
