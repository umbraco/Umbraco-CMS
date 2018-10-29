

namespace Umbraco.Web.Actions
{
    //fixme: not needed, remove this
    public class ActionExport : IAction
    {
        public char Letter => '9';
        public string Alias => "export";
        public string Category => null;
        public string Icon => "download-alt";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => false;
    }
}
