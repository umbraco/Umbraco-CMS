namespace Umbraco.Cms.Core.Models;

public static class PropertyGroupExtensions
{
    private const char AliasSeparator = '/';

    /// <summary>
    ///     Gets the local alias.
    /// </summary>
    /// <param name="propertyGroup">The property group.</param>
    /// <returns>
    ///     The local alias.
    /// </returns>
    public static string? GetLocalAlias(this PropertyGroup propertyGroup) => GetLocalAlias(propertyGroup.Alias);

    internal static string? GetLocalAlias(string alias)
    {
        var lastIndex = alias?.LastIndexOf(AliasSeparator) ?? -1;
        if (lastIndex != -1)
        {
            return alias?.Substring(lastIndex + 1);
        }

        return alias;
    }

    internal static string? GetParentAlias(string? alias)
    {
        var lastIndex = alias?.LastIndexOf(AliasSeparator) ?? -1;
        if (lastIndex == -1)
        {
            return null;
        }

        return alias?.Substring(0, lastIndex);
    }

    /// <summary>
    ///     Updates the local alias.
    /// </summary>
    /// <param name="propertyGroup">The property group.</param>
    /// <param name="localAlias">The local alias.</param>
    public static void UpdateLocalAlias(this PropertyGroup propertyGroup, string localAlias)
    {
        var parentAlias = propertyGroup.GetParentAlias();
        if (string.IsNullOrEmpty(parentAlias))
        {
            propertyGroup.Alias = localAlias;
        }
        else
        {
            propertyGroup.Alias = parentAlias + AliasSeparator + localAlias;
        }
    }

    /// <summary>
    ///     Gets the parent alias.
    /// </summary>
    /// <param name="propertyGroup">The property group.</param>
    /// <returns>
    ///     The parent alias.
    /// </returns>
    public static string? GetParentAlias(this PropertyGroup propertyGroup) => GetParentAlias(propertyGroup.Alias);

    /// <summary>
    ///     Updates the parent alias.
    /// </summary>
    /// <param name="propertyGroup">The property group.</param>
    /// <param name="parentAlias">The parent alias.</param>
    public static void UpdateParentAlias(this PropertyGroup propertyGroup, string parentAlias)
    {
        var localAlias = propertyGroup.GetLocalAlias();
        if (string.IsNullOrEmpty(parentAlias))
        {
            propertyGroup.Alias = localAlias!;
        }
        else
        {
            propertyGroup.Alias = parentAlias + AliasSeparator + localAlias;
        }
    }
}
