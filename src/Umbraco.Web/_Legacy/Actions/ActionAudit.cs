using System;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked upon viewing audittrailing on a document
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.AdministrationCategory)]
    public class ActionAudit : IAction
    {
        public static ActionAudit Instance { get; } = new ActionAudit();

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'Z';
            }
        }

        public string JsFunctionName
        {
            get
            {
                return string.Format("{0}.actionAudit()", ClientTools.Scripts.GetAppActions);
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
                return "auditTrail";
            }
        }

        public string Icon
        {
            get
            {
                return "time";
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
                return true;
            }
        }

        #endregion
    }
}
