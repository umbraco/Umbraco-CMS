namespace Umbraco.Cms.Core.Actions
{

    /// <summary>
    /// This action is invoked when a document is being unpublished
    /// </summary>
    public class ActionUnpublish : IAction
    {
        public const char ActionLetter = 'Z';

        public char Letter => ActionLetter;
        public string Alias => "unpublish";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
        public string Icon => "circle-dotted";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
    }

}
