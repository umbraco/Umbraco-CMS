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

        public CreateChildEntity(string name, bool seperatorBefore = false)
        {
            Alias = ActionNew.ActionAlias;
            Icon = "add"; Name = name;
            SeperatorBefore = seperatorBefore;
        }

        public CreateChildEntity(ILocalizedTextService textService, bool seperatorBefore = false)
        {
            Alias = ActionNew.ActionAlias;
            Icon = "add";
            Name = textService.Localize($"actions/{Alias}");
            SeperatorBefore = seperatorBefore;
        }
    }
}
