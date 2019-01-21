using Umbraco.Core.Services;
using Umbraco.Web.Actions;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// Represents the refresh node menu item
    /// </summary>
    public sealed class CreateChildEntity : ActionMenuItem
    {
        public override string AngularServiceName => "umbracoMenuActions";

        public CreateChildEntity(string name, bool separatorBefore = false)
            : base(ActionNew.ActionAlias, name)
        {
            Icon = "add"; Name = name;
            SeparatorBefore = separatorBefore;
        }

        public CreateChildEntity(ILocalizedTextService textService, bool separatorBefore = false)
            : base(ActionNew.ActionAlias, textService)
        {
            Icon = "add";
            SeparatorBefore = separatorBefore;
        }
    }
}
