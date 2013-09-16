using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IScripting
    {
        IEnumerable<INotDynamicXmlDocument> NotDynamicXmlDocumentElements { get; }

        IEnumerable<IRazorStaticMapping> DataTypeModelStaticMappings { get; }
    }
}