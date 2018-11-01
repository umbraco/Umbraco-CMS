using System;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when a node reloads its children
    /// Concerns only the tree itself and thus you should not handle
    /// this action from without umbraco.
    /// </summary>
    [LegacyActionMenuItem("umbracoMenuActions", "RefreshNode")]
    public class ActionRefresh : IAction
    {
        //create singleton
        private static readonly ActionRefresh InnerInstance = new ActionRefresh();

        public static ActionRefresh Instance
        {
            get { return InnerInstance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'L';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionRefresh()", ClientTools.Scripts.GetAppActions);
            }
        }

        public string JsSource
        {
            get
            {

                return null;
            }
        }

        public string Alias
        {
            get
            {

                return "refreshNode";
            }
        }

        public string Icon
        {
            get
            {

                return "refresh";
            }
        }

        public bool ShowInNotifier
        {
            get
            {

                return false;
            }
        }
        public bool CanBePermissionAssigned
        {
            get
            {

                return false;
            }
        }

        public bool OpensDialog => false;

        #endregion
    }
}
