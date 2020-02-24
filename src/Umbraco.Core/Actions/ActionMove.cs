using Umbraco.Core;

namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked upon creation of a document, media, member
    /// </summary>
    public class ActionMove : IAction
    {
        public const char ActionLetter = 'M';

        public char Letter => ActionLetter;
        public string Alias => "move";
        public string Category => Constants.Conventions.PermissionCategories.StructureCategory;
        public string Icon => "enter";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
