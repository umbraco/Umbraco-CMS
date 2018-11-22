using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "ContentDomainsAndCulture")]
    public class ContentDomainsAndCulture
    {
        [DataMember(Name = "domains")]
        public IEnumerable<DomainDisplay> Domains { get; set; }

        [DataMember(Name = "language")]
        public string Language { get; internal set; }
    }
}
