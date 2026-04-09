namespace Umbraco.Cms.Api.Common.Attributes;

/// <summary>
///     Attribute used to map a class to a specific API for OpenAPI documentation generation.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MapToApiAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MapToApiAttribute"/> class.
    /// </summary>
    /// <param name="apiName">The name of the API to map to.</param>
    public MapToApiAttribute(string apiName) => ApiName = apiName;

    /// <summary>
    ///     Gets the name of the API this class is mapped to.
    /// </summary>
    public string ApiName { get; }
}
