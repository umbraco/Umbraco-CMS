using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    //fixme: not needed, remove this
    public class ActionNotify : IAction
    {
        public char Letter => 'T';
        public string Alias => "notify";
        public string Category => null;
        public string Icon => "megaphone";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => false;
    }
}
