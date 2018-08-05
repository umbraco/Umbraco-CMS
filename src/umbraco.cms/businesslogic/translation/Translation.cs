using System;
using System.Text.RegularExpressions;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

namespace umbraco.cms.businesslogic.translation
{
    [Obsolete("This will be removed in future versions, the translation utility will not work perfectly in v7.x")]
    public class Translation
    {
        public static int CountWords(int DocumentId)
        {
            Document d = new Document(DocumentId);

            int words = CountWordsInString(d.Text);
            var props = d.GenericProperties;
            foreach (Property p in props)
            {
                var asString = p.Value as string;
                if (asString != null)
                {
                    var trimmed = asString.Trim();
                    if (trimmed.IsNullOrWhiteSpace() == false)
                    {
                        words += CountWordsInString(trimmed);
                    }
                }
            }

            return words;
        }

        private static int CountWordsInString(string Text)
        {
            string pattern = @"<(.|\n)*?>";
            string tmpStr = Regex.Replace(Text, pattern, string.Empty);

            tmpStr = tmpStr.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");

            MatchCollection collection = Regex.Matches(tmpStr, @"[\S]+");
            return collection.Count;
        }
    }
}
