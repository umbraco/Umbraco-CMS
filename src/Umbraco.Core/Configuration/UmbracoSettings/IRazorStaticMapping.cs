using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRazorStaticMapping
    {
        Guid DataTypeGuid { get; }
        string NodeTypeAlias { get; }
        string PropertyTypeAlias { get; }
        string MappingName { get; }
    }
}