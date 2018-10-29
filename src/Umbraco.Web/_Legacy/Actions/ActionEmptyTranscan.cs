using System;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when the trash can is emptied
    /// </summary>
    public class ActionEmptyTranscan : IAction
    {
        //create singleton
        private static readonly ActionEmptyTranscan InnerInstance = new ActionEmptyTranscan();

        public static ActionEmptyTranscan Instance
        {
            get { return InnerInstance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'N';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionEmptyTranscan()", ClientTools.Scripts.GetAppActions);
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
                return "emptyRecycleBin";
            }
        }

        public string Icon
        {
            get
            {
                return "trash";
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
