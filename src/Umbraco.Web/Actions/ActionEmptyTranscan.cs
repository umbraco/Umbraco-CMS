using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    //fixme: not needed, remove this
    public class ActionEmptyTranscan : IAction
    {
        public char Letter => 'N';
        public string Alias => "emptyRecycleBin";
        public string Category => null;
        public string Icon => "trash";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => false;
    }
}
