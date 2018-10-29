

namespace Umbraco.Web.Actions
{
    //fixme: not needed, remove this
    public class ActionImport : IAction
    {
        public char Letter => '8';
        public string Alias => "importDocumentType";
        public string Category => null;
        public string Icon => "page-up";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => false;
    }
}
