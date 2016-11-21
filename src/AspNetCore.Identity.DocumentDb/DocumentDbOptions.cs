using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace AspNetCore.Identity.DocumentDb
{
    public class DocumentDbOptions
    {
        public string Database { get; set; }
        public string DocumentCollection { get; set; }
        public string PartitionKey { get; set; }
    }
}
