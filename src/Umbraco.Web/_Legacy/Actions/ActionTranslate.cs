using System;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when a translation occurs
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.AdministrationCategory)]
    public class ActionTranslate : IAction
    {
        //create singleton
#pragma warning disable 612,618
        private static readonly ActionTranslate m_instance = new ActionTranslate();
#pragma warning restore 612,618

        public static ActionTranslate Instance
        {
            get { return m_instance; }
        }

        #region IAction Members

        public char Letter
        {
            get
            {
                return '4';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return "";
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
                return "translate";
            }
        }

        public string Icon
        {
            get
            {
                return "comments";
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
