using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Editors;

public class BackOfficePreviewModel
{
    private readonly UmbracoFeatures _features;

    public BackOfficePreviewModel(UmbracoFeatures features, IEnumerable<ILanguage> languages)
    {
        _features = features;
        Languages = languages;
    }

    public IEnumerable<ILanguage> Languages { get; }

    public bool DisableDevicePreview => _features.Disabled.DisableDevicePreview;

    public string? PreviewExtendedHeaderView => _features.Enabled.PreviewExtendedView;
}
