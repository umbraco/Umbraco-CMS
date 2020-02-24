using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;


namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when a domain is being assigned to a document
    /// </summary>
    public class ActionAssignDomain : IAction
    {
        public const char ActionLetter = 'I';

        public char Letter => ActionLetter;
        public string Alias => "assignDomain";
        public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;
        public string Icon => "home";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
    }
}
