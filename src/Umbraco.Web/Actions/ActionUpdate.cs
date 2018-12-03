using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when copying a document or media
    /// </summary>
    public class ActionUpdate : IAction
    {
        public const char ActionLetter = 'A';

        public char Letter => ActionLetter;
        public string Alias => "update";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
        public string Icon => "save";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
