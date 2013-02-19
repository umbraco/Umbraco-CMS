using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core
{
    [UmbracoWillObsolete("This enumeration should be removed. Use Umbraco.Core.Strings.CleanStringType instead.")]
    public enum StringAliasCaseType
    {
        PascalCase,
        CamelCase,
        Unchanged
    }
}