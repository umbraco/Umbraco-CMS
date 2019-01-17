using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Trees
{
    /// <summary>
    /// Defines the back office content section
    /// </summary>
    public class ContentBackOfficeSectionDefinition : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Content;
        public string Name => "Content";
        public int SortOrder => 0;
    }

    /// <summary>
    /// Defines the back office media section
    /// </summary>
    public class MediaBackOfficeSectionDefinition : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Media;
        public string Name => "Media";
        public int SortOrder => 1;
    }

    /// <summary>
    /// Defines the back office settings section
    /// </summary>
    public class SettingsBackOfficeSectionDefinition : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Settings;
        public string Name => "Settings";
        public int SortOrder => 2;
    }

    /// <summary>
    /// Defines the back office packages section
    /// </summary>
    public class PackagesBackOfficeSectionDefinition : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Packages;
        public string Name => "Packages";
        public int SortOrder => 3;
    }

    /// <summary>
    /// Defines the back office users section
    /// </summary>
    public class UsersBackOfficeSectionDefinition : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Users;
        public string Name => "Users";
        public int SortOrder => 4;
    }

    /// <summary>
    /// Defines the back office members section
    /// </summary>
    public class MembersBackOfficeSectionDefinition : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Members;
        public string Name => "Members";
        public int SortOrder => 5;
    }

    /// <summary>
    /// Defines the back office translation section
    /// </summary>
    public class TranslationBackOfficeSectionDefinition : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Translation;
        public string Name => "Translation";
        public int SortOrder => 6;
    }
}
