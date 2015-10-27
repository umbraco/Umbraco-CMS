using System;
using umbraco.interfaces;
using umbraco.BasePages;

namespace umbraco.BusinessLogic.Actions
{
    /// <summary>
    /// This action is invoked when the content item is to be restored from the recycle bin
    /// </summary>
    public class ActionRestore : IAction
    {
        //create singleton
        private static readonly ActionRestore SingletonInstance = new ActionRestore();

        private ActionRestore() { }

        public static ActionRestore Instance
        {
            get { return SingletonInstance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'V';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return null;
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

                return "restore";
            }
        }

        public string Icon
        {
            get
            {

                return "undo";
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

                return false;
            }
        }

        #endregion
    }
}