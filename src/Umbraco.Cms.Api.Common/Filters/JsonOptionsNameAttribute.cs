namespace Umbraco.Cms.Api.Common.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class JsonOptionsNameAttribute : Attribute
{
    public JsonOptionsNameAttribute(string jsonOptionsName) => JsonOptionsName = jsonOptionsName;

    public string JsonOptionsName { get; }
}
