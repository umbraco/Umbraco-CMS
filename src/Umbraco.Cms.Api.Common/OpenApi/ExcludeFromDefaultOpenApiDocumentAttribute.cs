namespace Umbraco.Cms.Api.Common.OpenApi;

/// <summary>
/// Excludes the controller from the default OpenAPI document.
/// Use this when you have a custom OpenAPI document for your API.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ExcludeFromDefaultOpenApiDocumentAttribute : Attribute
{
}
