using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb
{
    public abstract class DocumentDbIdentityDocument
    {
        public virtual string DocumentType
        {
            get
            {
                return this.GetType().ToString();
            }
        }
    }
}
