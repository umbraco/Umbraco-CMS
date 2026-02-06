namespace Umbraco.Cms.Api.Common.Filters;

/// <summary>
///     Attribute used to specify the named JSON serialization options for a controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class JsonOptionsNameAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonOptionsNameAttribute"/> class.
    /// </summary>
    /// <param name="jsonOptionsName">The name of the JSON options configuration to use.</param>
    public JsonOptionsNameAttribute(string jsonOptionsName) => JsonOptionsName = jsonOptionsName;

    /// <summary>
    ///     Gets the name of the JSON options configuration.
    /// </summary>
    public string JsonOptionsName { get; }
}
