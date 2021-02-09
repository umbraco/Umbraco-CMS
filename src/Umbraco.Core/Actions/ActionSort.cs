namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when children to a document, media, member is being sorted
    /// </summary>
    public class ActionSort : IAction
    {
        public const char ActionLetter = 'S';

        public char Letter => ActionLetter;
        public string Alias => "sort";
        public string Category => Constants.Conventions.PermissionCategories.StructureCategory;
        public string Icon => "navigation-vertical";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
