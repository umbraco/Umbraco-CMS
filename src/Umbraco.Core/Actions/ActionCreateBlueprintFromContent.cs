namespace Umbraco.Cms.Core.Actions
{
    public class ActionCreateBlueprintFromContent : IAction
    {
        /// <inheritdoc/>
        public char Letter => 'ï';

        /// <inheritdoc/>
        public bool ShowInNotifier => false;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => true;

        /// <inheritdoc/>
        public string Icon => "blueprint";

        /// <inheritdoc/>
        public string Alias => "createblueprint";

        /// <inheritdoc/>
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
    }
}
