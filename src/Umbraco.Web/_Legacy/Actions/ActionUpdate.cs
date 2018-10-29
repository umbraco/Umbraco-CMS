using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when copying a document or media
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionUpdate : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionUpdate m_instance = new ActionUpdate();
#pragma warning restore 612,618

        public static ActionUpdate Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'A';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionUpdate()", ClientTools.Scripts.GetAppActions);
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
                return "update";
            }
        }

        public string Icon
        {
            get
            {
                return "save";
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
                return true;
            }
        }

        public bool OpensDialog => false;

        #endregion
    }
}
