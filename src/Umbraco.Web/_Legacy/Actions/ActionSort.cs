using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when children to a document, media, member is being sorted
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.StructureCategory)]
    public class ActionSort : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionSort m_instance = new ActionSort();
#pragma warning restore 612,618

        public static ActionSort Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {

                return 'S';
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

                return "sort";
            }
        }

        public string Icon
        {
            get
            {

                return "navigation-vertical";
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
