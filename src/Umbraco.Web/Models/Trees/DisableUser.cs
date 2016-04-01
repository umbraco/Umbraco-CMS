namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// Represents the disable user menu item
    /// </summary>
    [ActionMenuItem("umbracoMenuActions")]
    public sealed class DisableUser : ActionMenuItem
    {
        public DisableUser()
        {
            Alias = "disable";
            Icon = "remove";
        }
    }
}