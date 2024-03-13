using System.Text.RegularExpressions;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.OpenApi;

public class SchemaIdSelector : ISchemaIdSelector
{
    public virtual string SchemaId(Type type)
        => type.Namespace?.StartsWith("Umbraco.Cms") is true ? UmbracoSchemaId(type) : type.Name;

    protected string UmbracoSchemaId(Type type)
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

    private string SanitizedTypeName(Type t) => t.Name
        // first grab the "non generic" part of any generic type name (i.e. "PagedViewModel`1" becomes "PagedViewModel")
        .Split('`').First()
        // then remove the "ViewModel" postfix from type names
        .TrimEnd("ViewModel");

    private string HandleGenerics(string name, Type type)
    {
        if (!type.IsGenericType)
        {
            return name;
        }

        // find all types that implement this type and have an matching attribute
        var assignableTypesWithAttributeInfo = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.FullName?.StartsWith("Umbraco") == true)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(t => t.IsAssignableTo(type))
            .Select(t =>
            {
                var attribute = System.Attribute.GetCustomAttributes(t)
                        .FirstOrDefault(attribute => attribute is ShortGenericSchemaNameAttribute) as
                    ShortGenericSchemaNameAttribute;
                return attribute == null
                    ? new ShortSchemaNameAttributeInfo(t)
                    : new ShortSchemaNameAttributeInfo(t, attribute.GenericTypes, attribute.SchemaName);
            })
            .Where(info => info.GenericTypes != null);

        var matchingType = assignableTypesWithAttributeInfo
            .SingleOrDefault(t => t.GenericTypes!.Length == type.GenericTypeArguments.Length
                                  && t.GenericTypes.Intersect(type.GenericTypeArguments).Count() ==
                                  type.GenericTypeArguments.Length && t.SchemaName.IsNullOrWhiteSpace() == false);

        // use attribute custom name or append the generic type names, ultimately turning i.e. "PagedViewModel<RelationItemViewModel>" into "PagedRelationItem"
        return matchingType != null
            ? matchingType.SchemaName!
            : $"{name}{string.Join(string.Empty, type.GenericTypeArguments.Select(SanitizedTypeName))}";
    }

    private class ShortSchemaNameAttributeInfo
    {
        public Type Type { get; set; }
        public Type[]? GenericTypes { get; set; }
        public string? SchemaName { get; set; }

        public ShortSchemaNameAttributeInfo(Type type, Type[]? genericTypes = null, string? schemaName = null)
        {
            Type = type;
            GenericTypes = genericTypes;
            SchemaName = schemaName;
        }
    }
}
