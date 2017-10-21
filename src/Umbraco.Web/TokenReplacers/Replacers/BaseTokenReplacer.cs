using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.TokenReplacers.Replacers
{
    public abstract class BaseTokenReplacer
    {
        protected BaseTokenReplacer(TokenReplacerContext context)
        {
            TokenReplacerContext = context;
        }

        public abstract string Token { get; }

        public TokenReplacerContext TokenReplacerContext { get; private set; }

        protected void ReplaceTokens(ContentItemDisplay contentItem, string replacement)
        {
            foreach (var property in GetPropertiesWithValues(contentItem))
            {
                var stringValue = property.Value.ToString();
                if (string.IsNullOrWhiteSpace(stringValue) || ContainsToken(stringValue) == false)
                {
                    continue;
                }

                property.Value = ReplaceTokens(stringValue, replacement);
            }
        }

        protected void ReplaceTokens(ContentItemDisplay contentItem, DateTime replacement)
        {
            foreach (var property in GetPropertiesWithValues(contentItem))
            {
                var stringValue = property.Value.ToString();
                var format = string.Empty;
                if (string.IsNullOrWhiteSpace(stringValue) || (ContainsToken(stringValue) == false && ContainsToken(stringValue, out format) == false))
                {
                    continue;
                }

                property.Value = ReplaceTokens(stringValue, replacement.ToString(format), format);
            }
        }

        private static IEnumerable<ContentPropertyDisplay> GetPropertiesWithValues(ContentItemDisplay contentItem)
        {
            return contentItem.Properties
                .Where(x => x.Value != null);
        }

        private bool ContainsToken(string value)
        {
            return value.IndexOf("#" + Token + "#", StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        private bool ContainsToken(string value, out string format)
        {
            var pattern = "#" + Token + "(.*?)#";
            var regex = new Regex(pattern);
            var result = regex.Match(value);
            if (result.Success)
            {
                format = result.Groups[1].ToString().Substring(1);
                return true;
            }

            format = string.Empty;
            return false;
        }

        private string ReplaceTokens(string value, string replacement, string format = "")
        {
            var pattern = "#" + Token + (format == string.Empty ? string.Empty : ":" + format) + "#";
            return Regex.Replace(value, pattern, replacement, RegexOptions.IgnoreCase);
        }
    }
}