using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Trees;

/// <summary>
    /// Represents the refresh node menu item
/// </summary>
public sealed class CreateChildEntity : ActionMenuItem
{
    private const string icon = "icon-add";

    public CreateChildEntity(string name, bool separatorBefore = false)
        : base(ActionNew.ActionAlias, name)
    {
        Icon = icon;
        Name = name;
        SeparatorBefore = separatorBefore;
        UseLegacyIcon = false;
    }

    public CreateChildEntity(ILocalizedTextService textService, bool separatorBefore = false)
        : base(ActionNew.ActionAlias, textService)
    {
        Icon = icon;
        SeparatorBefore = separatorBefore;
        UseLegacyIcon = false;
    }

    public override string AngularServiceName => "umbracoMenuActions";
}
