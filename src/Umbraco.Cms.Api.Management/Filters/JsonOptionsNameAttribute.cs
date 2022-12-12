namespace Umbraco.Cms.Api.Management.Filters;

[AttributeUsage(AttributeTargets.Class)]
public class JsonOptionsNameAttribute : Attribute
{
    public JsonOptionsNameAttribute(string jsonOptionsName) => JsonOptionsName = jsonOptionsName;

    public string JsonOptionsName { get; }
}
