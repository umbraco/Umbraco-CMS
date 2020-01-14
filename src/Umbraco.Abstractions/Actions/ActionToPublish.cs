using Umbraco.Core;

namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when children to a document is being sent to published (by an editor without publishrights)
    /// </summary>
    public class ActionToPublish : IAction
    {
        public const char ActionLetter = 'H';

        public char Letter => ActionLetter;
        public string Alias => "sendtopublish";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
        public string Icon => "outbox";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
