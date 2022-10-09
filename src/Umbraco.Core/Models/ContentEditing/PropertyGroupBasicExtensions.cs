namespace Umbraco.Cms.Core.Models.ContentEditing;

internal static class PropertyGroupBasicExtensions
{
    public static string? GetParentAlias(this PropertyGroupBasic propertyGroup)
        => PropertyGroupExtensions.GetParentAlias(propertyGroup.Alias);
}
