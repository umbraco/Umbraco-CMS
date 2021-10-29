namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when copying a document is being rolled back
    /// </summary>
    public class ActionRollback : IAction
    {
        public const char ActionLetter = 'K';

        /// <inheritdoc/>
        public char Letter => ActionLetter;

        /// <inheritdoc/>
        public string Alias => "rollback";

        /// <inheritdoc/>
        public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;

        /// <inheritdoc/>
        public string Icon => "undo";

        /// <inheritdoc/>
        public bool ShowInNotifier => true;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => true;
    }
}
