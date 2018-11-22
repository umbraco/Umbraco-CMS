using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked upon creation of a document
    /// </summary>
    public class ActionNew : IAction
    {
        public const string ActionAlias = "create";
        public const char ActionLetter = 'C';

        public char Letter => ActionLetter;
        public string Alias => ActionAlias;
        public string Icon => "add";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
    }
}
