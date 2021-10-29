

namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when the content/media item is to be restored from the recycle bin
    /// </summary>
    public class ActionRestore : IAction
    {
        public const string ActionAlias = "restore";

        /// <inheritdoc/>
        public char Letter => 'V';

        /// <inheritdoc/>
        public string Alias => ActionAlias;

        /// <inheritdoc/>
        public string Category => null;

        /// <inheritdoc/>
        public string Icon => "undo";

        /// <inheritdoc/>
        public bool ShowInNotifier => true;

        /// <inheritdoc/>
        public bool CanBePermissionAssigned => false;

    }
}
