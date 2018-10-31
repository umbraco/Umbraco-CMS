using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when copying a document, media, member
    /// </summary>
    public class ActionCopy : IAction
    {
        public char Letter => 'O';
        public string Alias => "copy";
        public string Category => Constants.Conventions.PermissionCategories.StructureCategory;
        public string Icon => "documents";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
