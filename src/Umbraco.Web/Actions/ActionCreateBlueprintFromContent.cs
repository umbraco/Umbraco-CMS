using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;


namespace Umbraco.Web.Actions
{
    public class ActionCreateBlueprintFromContent : IAction
    {
        public char Letter => 'ï';
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
        public string Icon => "blueprint";
        public string Alias => "createblueprint";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
    }
}
