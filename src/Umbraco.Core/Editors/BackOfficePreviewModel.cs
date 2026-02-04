using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Editors;

/// <summary>
/// Represents the model used for the backoffice content preview functionality.
/// </summary>
public class BackOfficePreviewModel
{
    private readonly UmbracoFeatures _features;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficePreviewModel"/> class.
    /// </summary>
    /// <param name="features">The Umbraco features configuration.</param>
    /// <param name="languages">The collection of available languages.</param>
    public BackOfficePreviewModel(UmbracoFeatures features, IEnumerable<ILanguage> languages)
    {
        _features = features;
        Languages = languages;
    }

    /// <summary>
    /// Gets the collection of available languages for preview.
    /// </summary>
    public IEnumerable<ILanguage> Languages { get; }

    /// <summary>
    /// Gets a value indicating whether device preview is disabled.
    /// </summary>
    public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;

    /// <summary>
    /// Gets the path to an extended preview header view, if configured.
    /// </summary>
    public string? PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
}
