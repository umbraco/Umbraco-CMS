using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when rights are changed on a document
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionRights : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionRights m_instance = new ActionRights();
#pragma warning restore 612,618

        public static ActionRights Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'R';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionRights()", ClientTools.Scripts.GetAppActions);
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

                return "rights";
            }
        }

        public string Icon
        {
            get
            {

                return "vcard";
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
