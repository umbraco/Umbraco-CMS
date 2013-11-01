using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Umbraco.Core;

namespace Umbraco.Web
{
    /// <summary>
    /// Helper class to simplify querying section(folders).
    /// 
    /// Eg:
    ///   var Dictionary = Umbraco.GetDictionarySection("[Contact Us] ");
    ///   @Dictionary["My Sub Key"];
    /// </summary>
    public class DictionarySection
    {
        public string Section { get; private set; }
        private UmbracoHelper Umbraco;

        /// <summary>
        /// Use GetDictionarySection to access.
        /// </summary>
        public DictionarySection(UmbracoHelper umbraco, string section)
        {
            Section = section;
            Umbraco = umbraco;
        }

        private string GetHierarchyJoin
        {
            get {
                return ConfigurationManager.AppSettings.ContainsKey("dictionaryIsHierarchy") ? ConfigurationManager.AppSettings["dictionaryIsHierarchy"] : "";
            }
        }

        /// <summary>
        /// Get a dictionary value by combining the section key and child key.
        /// </summary>
        /// <param name="key">child key.</param>
        /// <returns>string</returns>
        public string GetDictionaryValue(string key)
        {
            return Umbraco.GetDictionaryValue(Section + GetHierarchyJoin + key);
        }

        /// <summary>
        /// Chain a sub category by appending the section key and sub section key.
        /// </summary>
        /// <param name="key">Sub sectin key</param>
        /// <returns>DictionarySection</returns>
        public DictionarySection GetDictionarySection(string key)
        {
            return new DictionarySection(Umbraco, Section + GetHierarchyJoin + key);
        }

        /// <summary>
        ///  Get this sections dictionary value.
        /// </summary>
        public string Value
        {
            get
            {
                return Umbraco.GetDictionaryValue(Section);
            }
        }

        /// <summary>
        /// Get a dictionary value by combining the section key and child key.
        /// </summary>
        /// <param name="key">child key.</param>
        /// <returns>string</returns>
        public string this[string key]
        {
            get
            {
                return Umbraco.GetDictionaryValue(Section + GetHierarchyJoin + key);
            }
        }
    }
}
