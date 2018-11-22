using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when a document, media, member is deleted
    /// </summary>
    public class ActionDelete : IAction
    {
        public const string ActionAlias = "delete";
        public const char ActionLetter = 'D';

        public char Letter => ActionLetter;
        public string Alias => ActionAlias;
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
        public string Icon => "delete";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
