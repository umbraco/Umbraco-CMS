using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Trees;

/// <inheritdoc />
/// <summary>
///     Represents the view page node item
/// </summary>
public sealed class View : ActionMenuItem
{
    private const string icon = "icon-application-window-alt";

    public View(string name, bool separatorBefore = false) : base("view", name)
    {
        Icon = icon;
        SeparatorBefore = separatorBefore;
        UseLegacyIcon = false;
    }

    public View(ILocalizedTextService textService, bool separatorBefore = false)
        : base("view", textService)
    {
        Icon = icon;
        SeparatorBefore = separatorBefore;
        UseLegacyIcon = false;
    }

    public override string AngularServiceName => "umbracoMenuActions";
}
