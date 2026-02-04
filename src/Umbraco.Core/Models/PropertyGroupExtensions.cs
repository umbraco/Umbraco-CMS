namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Provides extension methods for <see cref="PropertyGroup" />.
/// </summary>
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

    /// <summary>
    ///     Gets the local alias from a full alias string.
    /// </summary>
    /// <param name="alias">The full alias.</param>
    /// <returns>The local alias portion after the last separator.</returns>
    internal static string? GetLocalAlias(string alias)
    {
        var lastIndex = alias?.LastIndexOf(AliasSeparator) ?? -1;
        if (lastIndex != -1)
        {
            return alias?.Substring(lastIndex + 1);
        }

        return alias;
    }

    /// <summary>
    ///     Gets the parent alias from a full alias string.
    /// </summary>
    /// <param name="alias">The full alias.</param>
    /// <returns>The parent alias portion before the last separator, or <c>null</c> if there is no separator.</returns>
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
