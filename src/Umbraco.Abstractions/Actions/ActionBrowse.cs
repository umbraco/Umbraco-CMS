using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is used as a security constraint that grants a user the ability to view nodes in a tree
    /// that has  permissions applied to it.
    /// </summary>
    /// <remarks>
    /// This action should not be invoked. It is used as the minimum required permission to view nodes in the content tree. By
    /// granting a user this permission, the user is able to see the node in the tree but not edit the document. This may be used by other trees
    /// that support permissions in the future.
    /// </remarks>
    public class ActionBrowse : IAction
    {
        public const char ActionLetter = 'F';

        public char Letter => ActionLetter;
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
        public string Icon => "";
        public string Alias => "browse";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
    }
}
