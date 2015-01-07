using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using umbraco.BasePages;
using Umbraco.Core.Services;
using File = System.IO.File;
using User = umbraco.BusinessLogic.User;

namespace umbraco
{

    //TODO: Make the User overloads obsolete, then publicize the IUser object

    /// <summary>
    /// The ui class handles the multilingual text in the umbraco back-end.
    /// Provides access to language settings and language files used in the umbraco back-end.
    /// </summary>
    [Obsolete("Use the ILocalizedTextService instead which is the ApplicationContext.Services")]
    public class ui
    {
        private static readonly string UmbracoDefaultUiLanguage = GlobalSettings.DefaultUILanguage;
        private static readonly string UmbracoPath = SystemDirectories.Umbraco;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Get the current culture/language from the currently logged in IUser, the IUser object is available on most Umbraco base classes and on the UmbracoContext")]
        public static string Culture(User u)
        {
            if (ApplicationContext.Current == null) return string.Empty;

            var found = UserExtensions.GetUserCulture(u.Language, ApplicationContext.Current.Services.TextService);
            return found == null ? string.Empty : found.Name;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Get the current culture/language from the currently logged in IUser, the IUser object is available on most Umbraco base classes and on the UmbracoContext")]
        internal static string Culture(IUser u)
        {
            if (ApplicationContext.Current == null) return string.Empty;

            var found = u.GetUserCulture(ApplicationContext.Current.Services.TextService);
            return found == null ? string.Empty : found.Name;
        }
        
        private static string GetLanguage()
        {
            var user = UmbracoEnsuredPage.CurrentUser;
            return GetLanguage(user);
        }

        private static string GetLanguage(User u)
        {
            if (u != null)
            {
                return u.Language;
            }
            return GetLanguage("");
        }

        private static string GetLanguage(IUser u)
        {
            if (u != null)
            {
                return u.Language;
            }
            return GetLanguage("");
        }

        private static string GetLanguage(string userLanguage)
        {
            if (userLanguage.IsNullOrWhiteSpace() == false)
            {
                return userLanguage;
            }
            var language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            if (string.IsNullOrEmpty(language))
                language = UmbracoDefaultUiLanguage;
            return language;
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the specified user's language settings
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Text(string Key, User u)
        {
            if (ApplicationContext.Current == null) return "[" + Key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(Key, GetCultureFromUserLanguage(GetLanguage(u)));
        }

        internal static string Text(string key, IUser u)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(key, GetCultureFromUserLanguage(GetLanguage(u)));
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the logged-in user's language settings
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string Text(string Key)
        {
            if (ApplicationContext.Current == null) return "[" + Key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(Key, GetCultureFromUserLanguage(GetLanguage()));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area, based on the specified users language settings
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key, User u)
        {
            if (ApplicationContext.Current == null) return "[" + Key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", Area, Key),
                GetCultureFromUserLanguage(GetLanguage(u)));
        }

        public static string Text(string area, string key, IUser u)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", area, key),
                GetCultureFromUserLanguage(GetLanguage(u)));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area, based on the logged-in users language settings
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key)
        {
            if (ApplicationContext.Current == null) return "[" + Key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", Area, Key),
                GetCultureFromUserLanguage(GetLanguage()));
        }

        /// <summary>
        /// Returns translated UI text with a specific area and key. based on the specified users language settings and variables array passed to the method
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <param name="Variables">The variables array.</param>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key, string[] Variables, User u)
        {
            if (ApplicationContext.Current == null) return "[" + Key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", Area, Key),
                GetCultureFromUserLanguage(GetLanguage(u)),
                ConvertToDictionaryVars(Variables));
        }

        internal static string Text(string area, string key, string[] variables)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", area, key),
                GetCultureFromUserLanguage(GetLanguage()),
                ConvertToDictionaryVars(variables));
        }

        internal static string Text(string area, string key, string[] variables, IUser u)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", area, key),
                GetCultureFromUserLanguage(GetLanguage(u)),
                ConvertToDictionaryVars(variables));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area based on the specified users language settings and single variable passed to the method
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <param name="Variable">The variable.</param>
        /// <param name="u">The u.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key, string Variable, User u)
        {
            if (ApplicationContext.Current == null) return "[" + Key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", Area, Key),
                GetCultureFromUserLanguage(GetLanguage(u)),
                ConvertToDictionaryVars(new[] { Variable }));
        }

        internal static string Text(string area, string key, string variable)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", area, key),
                GetCultureFromUserLanguage(GetLanguage()),
                ConvertToDictionaryVars(new[] { variable }));
        }

        internal static string Text(string area, string key, string variable, IUser u)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", area, key),
                GetCultureFromUserLanguage(GetLanguage(u)),
                ConvertToDictionaryVars(new[] { variable }));
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the logged-in user's language settings
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetText(string key)
        {
            return ApplicationContext.Current.Services.TextService.Localize(key, GetCultureFromUserLanguage(GetLanguage()));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area based on the logged-in users language settings
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetText(string area, string key)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
                string.Format("{0}/{1}", area, key),
                GetCultureFromUserLanguage(GetLanguage()));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area based on the logged-in users language settings and variables array send to the method.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="key">The key.</param>
        /// <param name="variables">The variables.</param>
        /// <returns></returns>
        public static string GetText(string area, string key, string[] variables)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
               string.Format("{0}/{1}", area, key),
               GetCultureFromUserLanguage(GetLanguage()),
               ConvertToDictionaryVars(variables));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area matching the variable send to the method.
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="key">The key.</param>
        /// <param name="variable">The variable.</param>
        /// <returns></returns>
        public static string GetText(string area, string key, string variable)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
               string.Format("{0}/{1}", area, key),
               GetCultureFromUserLanguage(GetLanguage()),
               ConvertToDictionaryVars(new[] { variable }));
        }

        /// <summary>
        /// Returns translated UI text with a specific key, area and language matching the variables send to the method.
        /// </summary>
        /// <param name="area">The area (Optional)</param>
        /// <param name="key">The key (Required)</param>
        /// <param name="variables">The variables (Optional)</param>
        /// <param name="language">The language (Optional)</param>
        /// <returns></returns>
        /// <remarks>This is the underlying call for all Text/GetText method calls</remarks>
        public static string GetText(string area, string key, string[] variables, string language)
        {
            if (ApplicationContext.Current == null) return "[" + key + "]";

            return ApplicationContext.Current.Services.TextService.Localize(
               string.Format("{0}/{1}", area, key),
               GetCultureFromUserLanguage(GetLanguage()),
               ConvertToDictionaryVars(variables));
        }
    
        /// <summary>
        /// Gets the language file as a xml document.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        [Obsolete("This is no longer used and will be removed from the codebase in future versions, to get the contents of language text use ILocalizedTextService.GetAllStoredValues")]
        public static XmlDocument getLanguageFile(string language)
        {
            var cacheKey = "uitext_" + language;

            var file = IOHelper.MapPath(UmbracoPath + "/config/lang/" + language + ".xml");
            if (File.Exists(file))
            {
                return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                    cacheKey,
                    CacheItemPriority.Default,
                    new CacheDependency(IOHelper.MapPath(UmbracoPath + "/config/lang/" + language + ".xml")),
                    () =>
                        {
                            using (var langReader = new XmlTextReader(IOHelper.MapPath(UmbracoPath + "/config/lang/" + language + ".xml")))
                            {
                                try
                                {
                                    var langFile = new XmlDocument();
                                    langFile.Load(langReader);
                                    return langFile;
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Error<ui>("Error reading umbraco language xml source (" + language + ")", e);
                                    return null;
                                }
                            }
                        });
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Convert an array of strings to a dictionary of indicies -> values
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        internal static IDictionary<string, string> ConvertToDictionaryVars(string[] variables)
        {
            if (variables == null) return null;
            if (variables.Any() == false) return null;

            return variables.Select((s, i) => new {index = i.ToString(CultureInfo.InvariantCulture), value = s})
                .ToDictionary(keyvals => keyvals.index, keyvals => keyvals.value);
        }

        private static CultureInfo GetCultureFromUserLanguage(string userLang)
        {
            return CultureInfo.GetCultureInfo(userLang.Replace("_", "-"));
        }

    }
}