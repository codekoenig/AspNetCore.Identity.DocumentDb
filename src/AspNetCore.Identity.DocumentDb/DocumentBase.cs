using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Identity.DocumentDb
{
    public abstract class DocumentBase
    {
        [JsonProperty(PropertyName = "documentType")]
        public virtual string DocumentType
        {
            get
            {
                return this.GetType().Name;
            }
        }
    }
}
