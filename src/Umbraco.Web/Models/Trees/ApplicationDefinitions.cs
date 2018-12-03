using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Trees
{
    // The application definitions are intended as a means to auto populate
    // the application.config. On app startup, Umbraco will look for any
    // unregistered classes with an ApplicationAttribute and add them to the cache

    [Application(Constants.Applications.Content, "Content")]
    public class ContentApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Media, "Media", sortOrder: 1)]
    public class MediaApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Settings, "Settings", sortOrder: 2)]
    public class SettingsApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Packages, "Packages", sortOrder: 3)]
    public class PackagesApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Users, "Users", sortOrder: 4)]
    public class UsersApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Members, "Members", sortOrder: 5)]
    public class MembersApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Translation, "Translation", sortOrder: 6)]
    public class TranslationApplicationDefinition : IApplication
    { }
}
