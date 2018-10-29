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
        {
            Alias = "refreshNode";
            Icon = "refresh";
            Name = name;
            SeperatorBefore = seperatorBefore;
        }

        public RefreshNode(ILocalizedTextService textService, bool seperatorBefore = false)
        {
            Alias = "refreshNode";
            Icon = "refresh";
            Name = textService.Localize($"actions/{Alias}");
            SeperatorBefore = seperatorBefore;
        }
    }
}
