namespace Umbraco.Cms.Api.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SupportedDerivedTypesAttribute(params Type[] types) : Attribute
{
    public Type[] Types { get; } = types;
}
