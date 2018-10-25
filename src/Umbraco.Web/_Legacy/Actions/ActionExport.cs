using System;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when exporting a document type
    /// </summary>
    public class ActionExport : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionExport m_instance = new ActionExport();
#pragma warning restore 612,618

        public static ActionExport Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return '9';
            }
        }

        public string JsFunctionName
        {
            get { return ""; }
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
                return "export";
            }
        }

        public string Icon
        {
            get
            {
                return "download-alt";
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
