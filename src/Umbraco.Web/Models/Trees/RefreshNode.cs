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

        public RefreshNode(string name, bool seperatorBefore = false)
            : base("refreshNode", name)
        {
            Icon = "refresh";
            SeperatorBefore = seperatorBefore;
        }

        public RefreshNode(ILocalizedTextService textService, bool seperatorBefore = false)
            : base("refreshNode", textService)
        {
            Icon = "refresh";
            SeperatorBefore = seperatorBefore;
        }
    }
}
