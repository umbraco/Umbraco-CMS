namespace Umbraco.Cms.Api.Common.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class MapToApiAttribute : Attribute
{
    public MapToApiAttribute(string apiName) => ApiName = apiName;

    public string ApiName { get; }
}
