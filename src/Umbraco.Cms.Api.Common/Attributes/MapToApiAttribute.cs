namespace Umbraco.Cms.Api.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MapToApiAttribute : Attribute
{
    public MapToApiAttribute(string apiName) => ApiName = apiName;

    public string ApiName { get; }
}
