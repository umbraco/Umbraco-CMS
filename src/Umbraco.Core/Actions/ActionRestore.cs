

namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when the content/media item is to be restored from the recycle bin
    /// </summary>
    public class ActionRestore : IAction
    {
        public const string ActionAlias = "restore";

        public char Letter => 'V';
        public string Alias => ActionAlias;
        public string Category => null;
        public string Icon => "undo";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => false;

    }
}
