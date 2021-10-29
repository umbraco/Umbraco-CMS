namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when a document is being published
    /// </summary>
    public class ActionPublish : IAction
    {
        public const char ActionLetter = 'U';

        /// <inheritdoc/>
        public char Letter => ActionLetter;

        /// <inheritdoc/>
        public string Alias => "publish";

        /// <inheritdoc/>
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

        /// <inheritdoc/>
        public string Icon => string.Empty;

        /// <inheritdoc/>
        public bool ShowInNotifier => true;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => true;
    }
}
