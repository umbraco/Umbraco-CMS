using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Xml;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.IO;

namespace umbraco
{
    /// <summary>
    /// The ui class handles the multilingual text in the umbraco back-end.
    /// Provides access to language settings and language files used in the umbraco back-end.
    /// </summary>
    public class ui
    {
        private static readonly string umbracoDefaultUILanguage = GlobalSettings.DefaultUILanguage;
        private static readonly string umbracoPath = SystemDirectories.Umbraco;

        /// <summary>
        /// Gets the current Culture for the logged-in users
        /// </summary>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Culture(User u)
        {
            XmlDocument langFile = getLanguageFile(u.Language);
            try
            {
                return langFile.SelectSingleNode("/language").Attributes.GetNamedItem("culture").Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if th user is logged in, if they are, return their language specified in the database.
        /// If they aren't logged in, check the current thread culture and return it, however if that is
        /// null, then return the default Umbraco culture.
        /// </summary>
        /// <returns></returns>
        private static string getLanguage()
        {
            return getLanguage(UmbracoEnsuredPage.CurrentUser);
        }

        /// <summary>
        /// Check if th user is logged in, if they are, return their language specified in the database.
        /// If they aren't logged in, check the current thread culture and return it, however if that is
        /// null, then return the default Umbraco culture.
        /// </summary>
        private static string getLanguage(User u)
        {
            if (u != null)
            {
                return u.Language;
            }
            else
            {
                string language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                if (string.IsNullOrEmpty(language))
                    language = umbracoDefaultUILanguage;
                return language;
            }
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the specified user's language settings
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <param name="u">The user.</param>
        /// <returns></returns>
        public static string Text(string Key, User u)
        {
            return GetText(string.Empty, Key, null, getLanguage(u));
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the logged-in user's language settings
        /// </summary>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string Text(string Key)
        {
            return GetText(Key);
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
            return GetText(Area, Key, null, getLanguage(u));
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area, based on the logged-in users language settings
        /// </summary>
        /// <param name="Area">The area.</param>
        /// <param name="Key">The key.</param>
        /// <returns></returns>
        public static string Text(string Area, string Key)
        {
            return GetText(Area, Key, getLanguage());
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
            return GetText(Area, Key, Variables, getLanguage(u));
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
            return GetText(Area, Key, new string[] { Variable }, getLanguage(u));
        }

        /// <summary>
        /// Returns translated UI text with a specific key based on the logged-in user's language settings
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetText(string key)
        {
            return GetText(string.Empty, key, null, getLanguage());            
        }

        /// <summary>
        /// Returns translated UI text with a specific key and area based on the logged-in users language settings
        /// </summary>
        /// <param name="area">The area.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetText(string area, string key)
        {
            return GetText(area, key, null, getLanguage());
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
            return GetText(area, key, variables, getLanguage());
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
            return GetText(area, key, new string[] { variable }, getLanguage());
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
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            if (string.IsNullOrEmpty(language))
                language = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            
            XmlDocument langFile = getLanguageFile(language);
            
            if (langFile != null)
            {
                XmlNode node;
                if (string.IsNullOrEmpty(area))
                {
                    node = langFile.SelectSingleNode(string.Format("//key [@alias = '{0}']", key));
                }
                else
                {
                    node = langFile.SelectSingleNode(string.Format("//area [@alias = '{0}']/key [@alias = '{1}']", area, key));
                }
                
                if (node != null)
                {
                    if (variables != null && variables.Length > 0)
                    {
                        return GetStringWithVars(node, variables);
                    }
                    else
                    {
                        return xmlHelper.GetNodeValue(node);
                    }
                }
            }
            return "[" + key + "]";
        }

        private static string GetStringWithVars(XmlNode node, string[] variables)
        {
            string stringWithVars = xmlHelper.GetNodeValue(node);
            MatchCollection vars =
                Regex.Matches(stringWithVars, @"\%(\d)\%",
                              RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            foreach (Match var in vars)
            {
                stringWithVars =
                    stringWithVars.Replace(var.Value,
                                           variables[Convert.ToInt32(var.Groups[0].Value.Replace("%", ""))]);
            }
            return stringWithVars;
        }

        /// <summary>
        /// Gets the language file as a xml document.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public static XmlDocument getLanguageFile(string language)
        {
            XmlDocument langFile = new XmlDocument();

            string cacheKey = "uitext_" + language;

            // Check for language file in cache
            if (HttpRuntime.Cache[cacheKey] == null)
            {
                using (XmlTextReader langReader =
                    new XmlTextReader(
                        IOHelper.MapPath(umbracoPath + "/config/lang/" + language + ".xml")))
                {
                    try
                    {
                        langFile.Load(langReader);
                        HttpRuntime.Cache.Insert(cacheKey, langFile,
                                                 new CacheDependency(
                                                     IOHelper.MapPath(umbracoPath + "/config/lang/" +
                                                                                        language + ".xml")));
                    }
                    catch (Exception e)
                    {
                        HttpContext.Current.Trace.Warn("ui",
                                                       "Error reading umbraco language xml source (" + language + ")", e);
                    }
                }
            }
            else
                langFile = (XmlDocument) HttpRuntime.Cache["uitext_" + language];

            return langFile;
        }

        private void insertKey(string key, string area, string language) {
            
        }
    }
}