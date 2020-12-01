using Umbraco.Core;

namespace Umbraco.Web.Actions
{
    /// <summary>
    /// This action is invoked when copying a document, media, member
    /// </summary>
    public class ActionCopy : IAction
    {
        public const char ActionLetter = 'O';

        public char Letter => ActionLetter;
        public string Alias => "copy";
        public string Category => Constants.Conventions.PermissionCategories.StructureCategory;
        public string Icon => "documents";
        public bool ShowInNotifier => true;
        public bool CanBePermissionAssigned => true;
    }
}
