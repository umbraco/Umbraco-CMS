using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when the document type of a piece of content is changed
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.AdministrationCategory)]
    public class ActionChangeDocType : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionChangeDocType m_instance = new ActionChangeDocType();
#pragma warning restore 612,618

        public static ActionChangeDocType Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return '7';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionChangeDocType()", ClientTools.Scripts.GetAppActions);
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

                return "changeDocType";
            }
        }

        public string Icon
        {
            get
            {

                return "axis-rotation-2";
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
