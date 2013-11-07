using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IScriptingSection : IUmbracoConfigurationSection
    {
        IEnumerable<INotDynamicXmlDocument> NotDynamicXmlDocumentElements { get; }

        IEnumerable<IRazorStaticMapping> DataTypeModelStaticMappings { get; }
    }
}