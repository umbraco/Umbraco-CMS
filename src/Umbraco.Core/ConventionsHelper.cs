using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core;

public static class ConventionsHelper
{
    public static Dictionary<string, PropertyType> GetStandardPropertyTypeStubs(IShortStringHelper shortStringHelper) =>
        new();
}
