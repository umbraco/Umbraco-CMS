using System;
using umbraco.interfaces;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace umbraco.BusinessLogic.Actions
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
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionBrowse : IAction
    {
        //create singleton
        private static readonly ActionBrowse instance = new ActionBrowse();
        private ActionBrowse() { }
        public static ActionBrowse Instance
        {
            get { return instance; }
        }

        #region IAction Members

        public char Letter
        {
            get { return 'F'; }
        }

        public bool ShowInNotifier
        {
            get { return false; }
        }

        public bool CanBePermissionAssigned
        {
            get { return true; }
        }

        public string Icon
        {
            get { return ""; }
        }

        public string Alias
        {
            get { return "browse"; }
        }

        public string JsFunctionName
        {
            get { return ""; }
        }

        public string JsSource
        {
            get { return ""; }
        }

        #endregion
        
    }
}
