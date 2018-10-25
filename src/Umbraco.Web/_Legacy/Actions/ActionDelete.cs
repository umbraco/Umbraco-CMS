using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when a document, media, member is deleted
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionDelete : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionDelete m_instance = new ActionDelete();
#pragma warning restore 612,618

        public static ActionDelete Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'D';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionDelete()", ClientTools.Scripts.GetAppActions);
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
                return "delete";
            }
        }

        public string Icon
        {
            get
            {
                return "delete";
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

        public bool OpensDialog => true;

        #endregion
    }
}
