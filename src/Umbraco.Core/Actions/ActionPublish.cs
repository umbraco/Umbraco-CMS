using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when a document is being published
    /// </summary>
    public class ActionPublish : IAction
    {
        public const char ActionLetter = 'U';

        public char Letter => ActionLetter;
        public string Alias => "publish";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
        public string Icon => string.Empty;
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
