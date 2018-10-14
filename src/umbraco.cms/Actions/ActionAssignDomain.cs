using System;
using umbraco.interfaces;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;

namespace umbraco.BusinessLogic.Actions
{
	/// <summary>
	/// This action is invoked when a domain is being assigned to a document
	/// </summary>
	[ActionMetadata(Constants.Conventions.PermissionCategories.AdministrationCategory)]
	public class ActionAssignDomain : IAction
    {
        private static readonly ActionAssignDomain instance = new ActionAssignDomain();

        public static ActionAssignDomain Instance
        {
            get { return instance; }
        }

        public char Letter { get; private set; }
        public bool ShowInNotifier { get; private set; }
        public bool CanBePermissionAssigned { get; private set; }
        public string Icon { get; private set; }
        public string Alias { get; private set; }
        public string JsFunctionName { get; private set; }
        public string JsSource { get; private set; }

        public ActionAssignDomain()
        {
            Letter = 'I';
            CanBePermissionAssigned = true;
            Icon = "home";
            Alias = "assignDomain";
        }

    }
}
