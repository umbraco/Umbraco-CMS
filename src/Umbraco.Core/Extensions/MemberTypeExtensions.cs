using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Extensions;

public static class MemberTypeExtensions
{
    public static IEnumerable<IPropertyType> GetSensitivePropertyTypes(this IMemberType memberType)
        => memberType.CompositionPropertyTypes.Where(p => memberType.IsSensitiveProperty(p.Alias));

    public static IEnumerable<string> GetSensitivePropertyTypeAliases(this IMemberType memberType)
        => memberType.GetSensitivePropertyTypes().Select(p => p.Alias);
}
