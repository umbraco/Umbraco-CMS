using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked upon creation of a document
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.ContentCategory)]
    public class ActionNew : IAction
    {
        public static ActionNew Instance { get; } = new ActionNew();

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'C';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionNew()", ClientTools.Scripts.GetAppActions);
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
                return "create";
            }
        }

        public string Icon
        {
            get
            {
                return "add";
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
