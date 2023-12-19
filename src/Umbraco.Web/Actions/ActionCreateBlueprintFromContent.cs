using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;


namespace Umbraco.Web.Actions
{
    public class ActionCreateBlueprintFromContent : IAction
    {
        public const char ActionLetter = 'ï';
        public char Letter => ActionLetter;
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
        public string Icon => "blueprint";
        public string Alias => "createblueprint";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
    }
}
