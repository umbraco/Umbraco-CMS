namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when a document, media, member is deleted
    /// </summary>
    public class ActionDelete : IAction
    {
        public const string ActionAlias = "delete";
        public const char ActionLetter = 'D';

        /// <inheritdoc/>
        public char Letter => ActionLetter;

        /// <inheritdoc/>
        public string Alias => ActionAlias;

        /// <inheritdoc/>
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

        /// <inheritdoc/>
        public string Icon => "delete";

        /// <inheritdoc/>
        public bool ShowInNotifier => true;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => true;
    }
}
