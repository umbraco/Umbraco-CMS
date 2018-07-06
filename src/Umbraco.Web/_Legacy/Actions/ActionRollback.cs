using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when copying a document is being rolled back
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.AdministrationCategory)]
    public class ActionRollback : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionRollback m_instance = new ActionRollback();
#pragma warning restore 612,618

        public static ActionRollback Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'K';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionRollback()", ClientTools.Scripts.GetAppActions);
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

                return "rollback";
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

                return true;
            }
        }
        #endregion
    }
}
