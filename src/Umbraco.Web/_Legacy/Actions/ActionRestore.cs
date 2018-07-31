namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when the content/media item is to be restored from the recycle bin
    /// </summary>
    public class ActionRestore : IAction
    {
        //create singleton

        private ActionRestore() { }

        public static ActionRestore Instance { get; } = new ActionRestore();

        #region IAction Members

        public char Letter => 'V';

        public string JsFunctionName => null;

        public string JsSource => null;

        public string Alias => "restore";

        public string Icon => "undo";

        public bool ShowInNotifier => true;

        public bool CanBePermissionAssigned => false;

        #endregion
    }
}
