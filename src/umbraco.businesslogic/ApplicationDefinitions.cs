using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.businesslogic
{
    // The application definitions are intended as a means to auto populate
    // the application.config. On app startup, Umbraco will look for any
    // unregistered classes with an ApplicationAttribute and add them to the cache

    [Application("content", "Content", ".traycontent")]
    public class ContentApplicationDefinition : IApplication
    { }

    [Application("media", "Media", ".traymedia", sortOrder: 1)]
    public class MediaApplicationDefinition : IApplication
    { }

    [Application("settings", "Settings", ".traysettings", sortOrder: 2)]
    public class SettingsApplicationDefinition : IApplication
    { }

    [Application("developer", "Developer", ".traydeveloper", sortOrder: 3)]
    public class DeveloperApplicationDefinition : IApplication
    { }

    [Application("users", "Users", ".trayusers", sortOrder: 4)]
    public class UsersApplicationDefinition : IApplication
    { }

    [Application("member", "Members", ".traymember", sortOrder: 5)]
    public class MembersApplicationDefinition : IApplication
    { }

    [Application("translation", "Translation", ".traytranslation", sortOrder: 6)]
    public class TranslationApplicationDefinition : IApplication
    { }
}
