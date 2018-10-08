using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Trees
{
    // The application definitions are intended as a means to auto populate
    // the application.config. On app startup, Umbraco will look for any
    // unregistered classes with an ApplicationAttribute and add them to the cache

    [Application(Constants.Applications.Content, "Content", ".traycontent")]
    public class ContentApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Media, "Media", ".traymedia", sortOrder: 1)]
    public class MediaApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Settings, "Settings", ".traysettings", sortOrder: 2)]
    public class SettingsApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Packages, "Packages", ".traydeveloper", sortOrder: 3)]
    public class PackagesApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Users, "Users", ".trayusers", sortOrder: 4)]
    public class UsersApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Members, "Members", ".traymember", sortOrder: 5)]
    public class MembersApplicationDefinition : IApplication
    { }

    [Application(Constants.Applications.Translation, "Translation", ".traytranslation", sortOrder: 6)]
    public class TranslationApplicationDefinition : IApplication
    { }
}
