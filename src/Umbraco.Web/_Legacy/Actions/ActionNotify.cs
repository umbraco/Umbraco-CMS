using System;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when a notification is sent
    /// </summary>
    public class ActionNotify : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionNotify m_instance = new ActionNotify();
#pragma warning restore 612,618

        public static ActionNotify Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'T';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionNotify()", ClientTools.Scripts.GetAppActions);
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
                return "notify";
            }
        }

        public string Icon
        {
            get
            {
                return "megaphone";
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
