using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Trees;

/// <summary>
///     Represents the export member menu item
/// </summary>
public sealed class ExportMember : ActionMenuItem
{
    public ExportMember(ILocalizedTextService textService)
        : base("export", textService)
    {
        Icon = "icon-download-alt";
        UseLegacyIcon = false;
    }

    public override string AngularServiceName => "umbracoMenuActions";
}
