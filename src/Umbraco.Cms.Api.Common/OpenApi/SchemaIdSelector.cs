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

        // use attribute custom name or append the generic type names, ultimately turning i.e. "PagedViewModel<RelationItemViewModel>" into "PagedRelationItem"
        return $"{name}{string.Join(string.Empty, type.GenericTypeArguments.Select(SanitizedTypeName))}";
    }
}
