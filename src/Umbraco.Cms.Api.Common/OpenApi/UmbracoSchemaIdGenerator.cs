using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.OpenApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Static utility for generating OpenAPI schema IDs following Umbraco's naming conventions.
/// </summary>
public static class UmbracoSchemaIdGenerator
{
    /// <summary>
    /// Creates a schema reference ID for the given JSON type info, applying Umbraco's naming conventions to
    /// types in the <c>Umbraco.Cms</c> namespace and falling back to the framework default for other types.
    /// </summary>
    /// <param name="jsonTypeInfo">The JSON type info to create a schema reference ID for.</param>
    /// <returns>The schema reference ID, or <c>null</c> if the type should be inlined.</returns>
    internal static string? CreateSchemaReferenceId(JsonTypeInfo jsonTypeInfo)
    {
        // Ensure that only types that would normally be included in the schema generation are given a schema reference ID.
        // Otherwise, we should return null to inline them.
        var defaultSchemaReferenceId = OpenApiOptions.CreateDefaultSchemaReferenceId(jsonTypeInfo);
        if (defaultSchemaReferenceId is null)
        {
            return null;
        }

        Type targetType = Nullable.GetUnderlyingType(jsonTypeInfo.Type) ?? jsonTypeInfo.Type;

        if (targetType.Namespace?.StartsWith("Umbraco.Cms") is not true)
        {
            return defaultSchemaReferenceId;
        }

        return Generate(targetType);
    }

    /// <summary>
    /// Generates a schema ID for the specified type following Umbraco's naming conventions.
    /// </summary>
    /// <param name="type">The type to generate a schema ID for.</param>
    /// <returns>The generated schema ID.</returns>
    public static string Generate(Type type)
    {
        var name = SanitizedTypeName(type);
        name = HandleGenerics(name, type);

        if (name.EndsWith("Model") == false)
        {
            // because some models names clash with common classes in TypeScript (i.e. Document),
            // we need to add a "Model" postfix to all models
            name = $"{name}Model";
        }

        // make absolutely sure we don't pass any invalid named by removing all non-word chars
        return Regex.Replace(name, @"[^\w]", string.Empty);
    }

    private static string SanitizedTypeName(Type t) => t.Name
        // first grab the "non-generic" part of any generic type name (i.e. "PagedViewModel`1" becomes "PagedViewModel")
        .Split('`').First()
        // then remove the "ViewModel" postfix from type names
        .TrimEnd("ViewModel");

    private static string HandleGenerics(string name, Type type)
    {
        if (!type.IsGenericType)
        {
            return name;
        }

        // use attribute custom name or append the generic type names, ultimately turning i.e. "PagedViewModel<RelationItemViewModel>" into "PagedRelationItem"
        return $"{name}{string.Join(string.Empty, type.GenericTypeArguments.Select(SanitizedTypeName))}";
    }
}
