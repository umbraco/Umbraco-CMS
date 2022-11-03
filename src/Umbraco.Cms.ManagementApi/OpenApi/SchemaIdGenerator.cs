using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.OpenApi;

internal static class SchemaIdGenerator
{
    public static string Generate(Type type)
    {
        string SanitizedTypeName(Type t) => t.Name
            // first grab the "non generic" part of any generic type name (i.e. "PagedViewModel`1" becomes "PagedViewModel")
            .Split('`').First()
            // then remove the "ViewModel" postfix from type names
            .TrimEnd("ViewModel");

        var name = SanitizedTypeName(type);
        if (type.IsGenericType)
        {
            // append the generic type names, ultimately turning i.e. "PagedViewModel<RelationItemViewModel>" into "PagedRelationItem"
            name += string.Join(string.Empty, type.GenericTypeArguments.Select(SanitizedTypeName));
        }

        // make absolutely sure we don't pass any invalid named by removing all non-word chars
        return Regex.Replace(name, @"[^\w]", string.Empty);
    }
}
