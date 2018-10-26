using System;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked upon creation of a document, media, member
    /// </summary>
    public class ActionPackage : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionPackage m_instance = new ActionPackage();
#pragma warning restore 612,618

        public static ActionPackage Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'X';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionPackage()", ClientTools.Scripts.GetAppActions);
            }
        }

        public string JsSource
        {
            get
            {
                return "";
            }
        }

        public string Alias
        {
            get
            {
                return "importPackage";
            }
        }

        public string Icon
        {
            get
            {
                return "gift";
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
