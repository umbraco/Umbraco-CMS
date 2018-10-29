using Umbraco.Web.UI.Pages;


namespace Umbraco.Web.Actions
{
    //fixme: not needed, remove this
    public class ActionRePublish : IAction
    {
        public char Letter => 'B';
        public string Alias => "republish";
        public string Category => null;
        public string Icon => "globe";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => false;
    }
}
