using System;
using System.Collections;
using Umbraco.Web.Macros;

namespace umbraco
{
    /// <summary>
    /// Summary description for helper.
    /// </summary>
    [Obsolete("This needs to be removed, do not use")]
    public class helper
    {
        public static bool IsNumeric(string number)
        {
            int result;
            return int.TryParse(number, out result);
        }

        public static string FindAttribute(IDictionary attributes, string key)
        {
            return FindAttribute(null, attributes, key);
        }

        public static string FindAttribute(IDictionary pageElements, IDictionary attributes, string key)
        {
            // fix for issue 14862: lowercase for case insensitive matching
            key = key.ToLower();

            var attributeValue = string.Empty;
            if (attributes[key] != null)
                attributeValue = attributes[key].ToString();

            attributeValue = MacroRenderer.ParseAttribute(pageElements, attributeValue);
            return attributeValue;
        }
    }
}
