using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when a document is being published
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionPublish : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionPublish m_instance = new ActionPublish();
#pragma warning restore 612,618

        public static ActionPublish Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'U';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Empty;
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
                return "publish";
            }
        }

        public string Icon
        {
            get
            {
                return string.Empty;
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
