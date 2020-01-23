using Umbraco.Core.Services;

namespace Umbraco.Web.Models.Trees
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the refresh node menu item
    /// </summary>
    public sealed class RefreshNode : ActionMenuItem
    {
        public override string AngularServiceName => "umbracoMenuActions";

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
    }
}
