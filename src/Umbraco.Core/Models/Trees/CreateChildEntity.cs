using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models.Trees;

/// <summary>
///     Represents the refresh node menu item
/// </summary>
public sealed class CreateChildEntity : ActionMenuItem
{
    public CreateChildEntity(string name, bool separatorBefore = false)
        : base(ActionNew.ActionAlias, name)
    {
        Icon = "add";
        Name = name;
        SeparatorBefore = separatorBefore;
    }

    public CreateChildEntity(ILocalizedTextService textService, bool separatorBefore = false)
        : base(ActionNew.ActionAlias, textService)
    {
        Icon = "add";
        SeparatorBefore = separatorBefore;
    }

    public override string AngularServiceName => "umbracoMenuActions";
}
