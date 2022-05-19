using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Trees;

/// <inheritdoc />
/// <summary>
///     Represents the refresh node menu item
/// </summary>
public sealed class RefreshNode : ActionMenuItem
{
    public RefreshNode(string name, bool separatorBefore = false)
        : base("refreshNode", name)
    {
        Icon = "refresh";
        SeparatorBefore = separatorBefore;
    }

    public RefreshNode(ILocalizedTextService textService, bool separatorBefore = false)
        : base("refreshNode", textService)
    {
        Icon = "refresh";
        SeparatorBefore = separatorBefore;
    }

    public override string AngularServiceName => "umbracoMenuActions";
}
