using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when children to a document, media, member is being sorted
    /// </summary>
    public class ActionSort : IAction
    {
        public char Letter => 'S';
        public string Alias => "sort";
        public string Category => Constants.Conventions.PermissionCategories.StructureCategory;
        public string Icon => "navigation-vertical";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
