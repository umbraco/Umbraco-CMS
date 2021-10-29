namespace Umbraco.Cms.Core.Actions
{

    /// <summary>
    /// This action is invoked when a document is being unpublished
    /// </summary>
    public class ActionUnpublish : IAction
    {
        public const char ActionLetter = 'Z';

        /// <inheritdoc/>
        public char Letter => ActionLetter;

        /// <inheritdoc/>
        public string Alias => "unpublish";

        /// <inheritdoc/>
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

        /// <inheritdoc/>
        public string Icon => "circle-dotted";

        /// <inheritdoc/>
        public bool ShowInNotifier => false;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => true;
    }

}
