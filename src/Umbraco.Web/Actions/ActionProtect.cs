using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when a document is protected or unprotected
    /// </summary>
    public class ActionProtect : IAction
    {
        public char Letter => 'P';
        public string Alias => "protect";
        public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;
        public string Icon => "lock";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
