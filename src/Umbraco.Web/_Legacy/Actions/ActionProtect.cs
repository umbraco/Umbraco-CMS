using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when a document is protected or unprotected
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.AdministrationCategory)]
    public class ActionProtect : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionProtect m_instance = new ActionProtect();
#pragma warning restore 612,618

        public static ActionProtect Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'P';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionProtect()", ClientTools.Scripts.GetAppActions);
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

                return "protect";
            }
        }

        public string Icon
        {
            get
            {

                return "lock";
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
