namespace Umbraco.Cms.Core.Actions
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
