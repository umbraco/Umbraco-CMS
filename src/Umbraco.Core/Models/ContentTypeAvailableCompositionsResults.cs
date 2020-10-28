using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Used when determining available compositions for a given content type
    /// </summary>
    [UmbracoVolatile]
    public class ContentTypeAvailableCompositionsResults
    {
        public ContentTypeAvailableCompositionsResults()
        {
            Ancestors = Enumerable.Empty<IContentTypeComposition>();
            Results = Enumerable.Empty<ContentTypeAvailableCompositionsResult>();
        }

        public ContentTypeAvailableCompositionsResults(IEnumerable<IContentTypeComposition> ancestors, IEnumerable<ContentTypeAvailableCompositionsResult> results)
        {
            Ancestors = ancestors;
            Results = results;
        }

        public IEnumerable<IContentTypeComposition> Ancestors { get; private set; }
        public IEnumerable<ContentTypeAvailableCompositionsResult> Results { get; private set; }
    }
}
