using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Trees;

/// <inheritdoc />
/// <summary>
///     Represents the refresh node menu item
/// </summary>
public sealed class RefreshNode : ActionMenuItem
{
    private const string icon = "icon-refresh";

    public RefreshNode(string name, bool separatorBefore = false)
        : base("refreshNode", name)
    {
        Icon = icon;
        SeparatorBefore = separatorBefore;
        UseLegacyIcon = false;
    }

    public RefreshNode(ILocalizedTextService textService, bool separatorBefore = false)
        : base("refreshNode", textService)
    {
        Icon = icon;
        SeparatorBefore = separatorBefore;
        UseLegacyIcon = false;
    }

    public override string AngularServiceName => "umbracoMenuActions";
}
