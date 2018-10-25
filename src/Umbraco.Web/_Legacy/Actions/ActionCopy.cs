using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when copying a document, media, member
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.StructureCategory)]
    public class ActionCopy : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionCopy m_instance = new ActionCopy();
#pragma warning restore 612,618

        public static ActionCopy Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'O';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionCopy()", ClientTools.Scripts.GetAppActions);
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

                return "copy";
            }
        }

        public string Icon
        {
            get
            {

                return "documents";
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
