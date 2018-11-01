using System;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when all documents are being republished
    /// </summary>
    public class ActionRePublish : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionRePublish m_instance = new ActionRePublish();
#pragma warning restore 612,618

        public static ActionRePublish Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'B';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionRePublish()", ClientTools.Scripts.GetAppActions);
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
                return "republish";
            }
        }

        public string Icon
        {
            get
            {
                return "globe";
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

        public bool OpensDialog => true;

        #endregion
    }
}
