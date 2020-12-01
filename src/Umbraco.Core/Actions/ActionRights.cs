using Umbraco.Core;

namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when rights are changed on a document
    /// </summary>
    public class ActionRights : IAction
    {
        public const char ActionLetter = 'R';

        public char Letter => ActionLetter;
        public string Alias => "rights";
        public string Category => Constants.Conventions.PermissionCategories.ContentCategory;
        public string Icon => "vcard";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
