using Umbraco.Core;

namespace Umbraco.Web.Actions
{
    public class ActionRestrictBlueprintToUserGroups : IAction
    {
        public char Letter => 'ü';
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
        public string Icon => "users";
        public string Alias => "restrictToUserGroups";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
    }
}
