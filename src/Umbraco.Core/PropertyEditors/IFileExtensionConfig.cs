namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Marker interface for any editor configuration that supports defining file extensions
/// </summary>
public interface IFileExtensionsConfig
{
    IEnumerable<string> FileExtensions { get; set; }
}
