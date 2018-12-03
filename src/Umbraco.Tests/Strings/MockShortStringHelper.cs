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

        public string CleanStringForSafeAlias(string text)
        {
            return "SAFE-ALIAS::" + text;
        }

        public string CleanStringForSafeAlias(string text, string culture)
        {
            return "SAFE-ALIAS-CULTURE::" + text;
        }

        public string CleanStringForUrlSegment(string text)
        {
            return "URL-SEGMENT::" + text;
        }

        public string CleanStringForUrlSegment(string text, string culture)
        {
            return "URL-SEGMENT-CULTURE::" + text;
        }

        public string CleanStringForSafeFileName(string text)
        {
            return "SAFE-FILE-NAME::" + text;
        }

        public string CleanStringForSafeFileName(string text, string culture)
        {
            return "SAFE-FILE-NAME-CULTURE::" + text;
        }

        public string SplitPascalCasing(string text, char separator)
        {
            return "SPLIT-PASCAL-CASING::" + text;
        }

        public string CleanString(string text, CleanStringType stringType)
        {
            return "CLEAN-STRING-A::" + text;
        }

        public string CleanString(string text, CleanStringType stringType, char separator)
        {
            return "CLEAN-STRING-B::" + text;
        }

        public string CleanString(string text, CleanStringType stringType, string culture)
        {
            return "CLEAN-STRING-C::" + text;
        }

        public string CleanString(string text, CleanStringType stringType, char separator, string culture)
        {
            return "CLEAN-STRING-D::" + text;
        }
    }
}
