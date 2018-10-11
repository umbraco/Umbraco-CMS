using System;
using Umbraco.Web.UI.Pages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web._Legacy.Actions
{
    /// <summary>
    /// This action is invoked when a domain is being assigned to a document
    /// </summary>
    [ActionMetadata(Constants.Conventions.PermissionCategories.AdministrationCategory)]
    public class ActionAssignDomain : IAction
    {
        public static ActionAssignDomain Instance { get; } = new ActionAssignDomain();

        #region IAction Members

        public char Letter
        {
            get
            {
                return 'I';
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
                return "assignDomain";
            }
        }

        public string Icon
        {
            get
            {
                return "home";
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
