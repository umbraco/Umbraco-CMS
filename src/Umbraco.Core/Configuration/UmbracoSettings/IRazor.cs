using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRazor
    {
        IEnumerable<INotDynamicXmlDocument> NotDynamicXmlDocumentElements { get; }

        IEnumerable<IRazorStaticMapping> DataTypeModelStaticMappings { get; }
    }
}