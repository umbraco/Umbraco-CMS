using System.Collections.Generic;

namespace Umbraco.Web.Models.ContentEditing
{
    public class ContentDomainsAndCulture
    {
        public IEnumerable<DomainDisplay> Domains { get; set; }

        public string Language { get; internal set; }
    }
}
