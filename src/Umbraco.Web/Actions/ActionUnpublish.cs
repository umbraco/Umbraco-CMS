using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;


namespace Umbraco.Web.Actions
{

    /// <summary>
    /// This action is invoked when a document is being unpublished
    /// </summary>
    public class ActionUnpublish : IAction
    {
        public char Letter => 'Z';
        public string Alias => "unpublish";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
        public string Icon => "circle-dotted";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
    }

}
