namespace Umbraco.Cms.Core.Actions
{
    /// <summary>
    /// This action is invoked when a domain is being assigned to a document
    /// </summary>
    public class ActionAssignDomain : IAction
    {
        public const char ActionLetter = 'I';

        public char Letter => ActionLetter;
        public string Alias => "assigndomain";
        public string Category => Constants.Conventions.PermissionCategories.AdministrationCategory;
        public string Icon => "home";
        public bool ShowInNotifier => false;
        public bool CanBePermissionAssigned => true;
    }
}
