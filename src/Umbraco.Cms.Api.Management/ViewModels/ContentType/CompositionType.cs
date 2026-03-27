namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Specifies the type of composition used for content types in the management system, such as whether a content type is composed of other types or acts as a composition itself.
/// </summary>
public enum CompositionType
{
    Composition,
    Inheritance
}
