namespace Umbraco.Cms.Core.CodeAnnotations;

[AttributeUsage(AttributeTargets.Field)]
public class UmbracoUdiTypeAttribute : Attribute
{
    public UmbracoUdiTypeAttribute(string udiType) => UdiType = udiType;

    public string UdiType { get; }
}
