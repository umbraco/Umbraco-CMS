using System.Collections.Generic;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.CoreStrings
{
    class MockShortStringHelper : IShortStringHelper
    {
        public void Freeze()
        {
            IsFrozen = true;
        }

        public bool IsFrozen { get; private set; }

        public string CleanStringForSafeAliasJavaScriptCode { get { return "CSFSAJS"; } }

        public string CleanStringForSafeAlias(string text)
        {
            return "SAFE-ALIAS::" + text;
        }

        public string CleanStringForSafeAlias(string text, System.Globalization.CultureInfo culture)
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

        public string SplitPascalCasing(string text, char separator)
        {
            return "SPLIT-PASCAL-CASING::" + text;
        }

        public string ReplaceMany(string text, IDictionary<string, string> replacements)
        {
            return "REPLACE-MANY::" + text;
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
