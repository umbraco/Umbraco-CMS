namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when a document is protected or unprotected
    /// </summary>
    public class ActionProtect : IAction
    {
        public const char ActionLetter = 'P';

        /// <inheritdoc/>
        public char Letter => ActionLetter;

        /// <inheritdoc/>
        public string Alias => "protect";

        /// <inheritdoc/>
        public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;

        /// <inheritdoc/>
        public string Icon => "lock";

        /// <inheritdoc/>
        public bool ShowInNotifier => true;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => true;
    }
}
