namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when rights are changed on a document
    /// </summary>
    public class ActionRights : IAction
    {
        public const char ActionLetter = 'R';

        /// <inheritdoc/>
        public char Letter => ActionLetter;

        /// <inheritdoc/>
        public string Alias => "rights";

        /// <inheritdoc/>
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;

        /// <inheritdoc/>
        public string Icon => "vcard";

        /// <inheritdoc/>
        public bool ShowInNotifier => true;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => true;
    }
}
