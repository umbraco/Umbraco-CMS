using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides helper methods for working with Umbraco conventions.
/// </summary>
public static class ConventionsHelper
{
    /// <summary>
    ///     Gets the standard property type stubs used by the system.
    /// </summary>
    /// <param name="shortStringHelper">The short string helper for string operations.</param>
    /// <returns>A dictionary of standard property types, currently empty.</returns>
    public static Dictionary<string, PropertyType> GetStandardPropertyTypeStubs(IShortStringHelper shortStringHelper) =>
        new();
}
