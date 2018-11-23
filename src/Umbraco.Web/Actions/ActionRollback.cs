using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when copying a document is being rolled back
    /// </summary>
    public class ActionRollback : IAction
    {
        public const char ActionLetter = 'K';

        public char Letter => ActionLetter;
        public string Alias => "rollback";
        public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;
        public string Icon => "undo";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
