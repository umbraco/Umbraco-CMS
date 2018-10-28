using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when children to a document is being sent to published (by an editor without publishrights)
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionToPublish : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionToPublish m_instance = new ActionToPublish();
#pragma warning restore 612,618

        public static ActionToPublish Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'H';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionToPublish()", ClientTools.Scripts.GetAppActions);
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
                return "sendtopublish";
            }
        }

        public string Icon
        {
            get
            {
                return "outbox";
            }
        }

        public bool ShowInNotifier
        {
            get
            {
                return true;
            }
        }
        public bool CanBePermissionAssigned
        {
            get
            {
                //SD: Changed this to true so that any user may be able to perform this action, not just a writer
                return true;
            }
        }

        public bool OpensDialog => false;

        #endregion
    }
}
