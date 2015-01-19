using System.Collections.Generic;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Strings
{
    class MockShortStringHelper : IShortStringHelper
    {
        public void Freeze()
        {
            IsFrozen = true;
        }

        public bool IsFrozen { get; private set; }

        public string GetShortStringServicesJavaScript(string controllerPath) { return "SSSJS"; }

        public string CleanStringForSafeAlias(string text)
        {
            return "SAFE-ALIAS::" + text;
        }

        public string CleanStringForSafeCamelAlias(string text)
        {
            return "SAFE-ALIAS::" + text;
        }

        public string CleanStringForSafeAlias(string text, System.Globalization.CultureInfo culture)
        {
            return "SAFE-ALIAS-CULTURE::" + text;
        }

        public string CleanStringForSafeCamelAlias(string text, System.Globalization.CultureInfo culture)
        {
            return "SAFE-ALIAS-CULTURE::" + text;
        }

        public string CleanStringForUrlSegment(string text)
        {
            return "URL-SEGMENT::" + text;
        }

        public string CleanStringForUrlSegment(string text, System.Globalization.CultureInfo culture)
        {
            return "URL-SEGMENT-CULTURE::" + text;
        }

        public string CleanStringForSafeFileName(string text)
        {
            return "SAFE-FILE-NAME::" + text;
        }

        public string CleanStringForSafeFileName(string text, System.Globalization.CultureInfo culture)
        {
            return "SAFE-FILE-NAME-CULTURE::" + text;
        }

        public string SplitPascalCasing(string text, char separator)
        {
            return "SPLIT-PASCAL-CASING::" + text;
        }

        public string ReplaceMany(string text, IDictionary<string, string> replacements)
        {
            return "REPLACE-MANY-A::" + text;
        }

        public string ReplaceMany(string text, char[] chars, char replacement)
        {
            return "REPLACE-MANY-B::" + text;
        }

        public string CleanString(string text, CleanStringType stringType)
        {
            return "CLEAN-STRING-A::" + text;
        }

        public string CleanString(string text, CleanStringType stringType, char separator)
        {
            return "CLEAN-STRING-B::" + text;
        }

        public string CleanString(string text, CleanStringType stringType, System.Globalization.CultureInfo culture)
        {
            return "CLEAN-STRING-C::" + text;
        }

        public string CleanString(string text, CleanStringType stringType, char separator, System.Globalization.CultureInfo culture)
        {
            return "CLEAN-STRING-D::" + text;
        }
    }
}
