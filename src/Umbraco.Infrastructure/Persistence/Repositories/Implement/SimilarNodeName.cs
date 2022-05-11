using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Extensions;
using static Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.SimilarNodeName;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement
{
    internal class SimilarNodeName
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public static string? GetUniqueName(IEnumerable<SimilarNodeName> names, int nodeId, string? nodeName)
        {
            var items = names
                    .Where(x => x.Id != nodeId) // ignore same node
                    .Select(x => x.Name);

            var uniqueName = GetUniqueName(items, nodeName);

            return uniqueName;
        }

        public static string? GetUniqueName(IEnumerable<string?> names, string? name)
        {
            var model = new StructuredName(name);
            var items = names
                    .Where(x => x?.InvariantStartsWith(model.Text) ?? false) // ignore non-matching names
                    .Select(x => new StructuredName(x));

            // name is empty, and there are no other names with suffixes, so just return " (1)"
            if (model.IsEmptyName() && !items.Any())
            {
                model.Suffix = StructuredName.INITIAL_SUFFIX;

                return model.FullName;
            }

            // name is empty, and there are other names with suffixes
            if (model.IsEmptyName() && items.SuffixedNameExists())
            {
                var emptyNameSuffix = GetSuffixNumber(items);

                if (emptyNameSuffix > 0)
                {
                    model.Suffix = (uint?)emptyNameSuffix;

                    return model.FullName;
                }
            }

            // no suffix - name without suffix does NOT exist - we can just use the name without suffix.
            if (!model.Suffix.HasValue && !items.SimpleNameExists(model.Text))
            {
                model.Suffix = StructuredName.NO_SUFFIX;

                return model.FullName;
            }

            // suffix - name with suffix does NOT exist
            // We can just return the full name as it is as there's no conflict.
            if (model.Suffix.HasValue && !items.SimpleNameExists(model.FullName))
            {
                return model.FullName;
            }

            // no suffix - name without suffix does NOT exist, AND name with suffix does NOT exist
            if (!model.Suffix.HasValue && !items.SimpleNameExists(model.Text) && !items.SuffixedNameExists())
            {
                model.Suffix = StructuredName.NO_SUFFIX;

                return model.FullName;
            }

            // no suffix - name without suffix exists, however name with suffix does NOT exist
            if (!model.Suffix.HasValue && items.SimpleNameExists(model.Text) && !items.SuffixedNameExists())
            {
                var firstSuffix = GetFirstSuffix(items);
                model.Suffix = (uint?)firstSuffix;

                return model.FullName;
            }

            // no suffix - name without suffix exists, AND name with suffix does exist
            if (!model.Suffix.HasValue && items.SimpleNameExists(model.Text) && items.SuffixedNameExists())
            {
                var nextSuffix = GetSuffixNumber(items);
                model.Suffix = (uint?)nextSuffix;

                return model.FullName;
            }

            // no suffix - name without suffix does NOT exist, however name with suffix exists
            if (!model.Suffix.HasValue && !items.SimpleNameExists(model.Text) && items.SuffixedNameExists())
            {
                var nextSuffix = GetSuffixNumber(items);
                model.Suffix = (uint?)nextSuffix;

                return model.FullName;
            }

            // has suffix - name without suffix exists
            if (model.Suffix.HasValue && items.SimpleNameExists(model.Text))
            {
                var nextSuffix = GetSuffixNumber(items);
                model.Suffix = (uint?)nextSuffix;

                return model.FullName;
            }

            // has suffix - name without suffix does NOT exist
            // a case where the user added the suffix, so add a secondary suffix
            if (model.Suffix.HasValue && !items.SimpleNameExists(model.Text))
            {
                model.Text = model.FullName;
                model.Suffix = StructuredName.NO_SUFFIX;

                // filter items based on full name with suffix
                items = items.Where(x => x.Text.InvariantStartsWith(model.FullName));
                var secondarySuffix = GetFirstSuffix(items);
                model.Suffix = (uint?)secondarySuffix;

                return model.FullName;
            }

            // has suffix - name without suffix also exists, therefore we simply increment
            if (model.Suffix.HasValue && items.SimpleNameExists(model.Text))
            {
                var nextSuffix = GetSuffixNumber(items);
                model.Suffix = (uint?)nextSuffix;

                return model.FullName;
            }

            return name;
        }

        private static int GetFirstSuffix(IEnumerable<StructuredName> items)
        {
            const int suffixStart = 1;

            if (!items.Any(x => x.Suffix == suffixStart))
            {
                // none of the suffixes are the same as suffixStart, so we can use suffixStart!
                return suffixStart;
            }

            return GetSuffixNumber(items);
        }

        private static int GetSuffixNumber(IEnumerable<StructuredName> items)
        {
            int current = 1;
            foreach (var item in items.OrderBy(x => x.Suffix))
            {
                if (item.Suffix == current)
                {
                    current++;
                }
                else if (item.Suffix > current)
                {
                    // do nothing - we found our number!
                    // eg. when suffixes are 1 & 3, then this method is required to generate 2
                    break;
                }
            }

            return current;
        }

        internal class StructuredName
        {
            const string SPACE_CHARACTER = " ";
            const string SUFFIXED_PATTERN = @"(.*) \(([1-9]\d*)\)$";
            internal const uint INITIAL_SUFFIX = 1;
            internal static readonly uint? NO_SUFFIX = default;

            internal string Text { get; set; }
            internal uint? Suffix { get; set; }
            public string FullName
            {
                get
                {
                    string text = (Text == SPACE_CHARACTER) ? Text.Trim() : Text;

                    return Suffix > 0 ? $"{text} ({Suffix})" : text;
                }
            }

            internal StructuredName(string? name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    Text = SPACE_CHARACTER;

                    return;
                }

                var rg = new Regex(SUFFIXED_PATTERN);
                var matches = rg.Matches(name);
                if (matches.Count > 0)
                {
                    var match = matches[0];
                    Text = match.Groups[1].Value;
                    int number = int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number) ? number : 0;
                    Suffix = (uint?)(number);

                    return;
                }
                else
                {
                    Text = name;
                }
            }

            internal bool IsEmptyName()
            {
                return string.IsNullOrWhiteSpace(Text);
            }
        }
    }

    internal static class ListExtensions
    {
        internal static bool Contains(this IEnumerable<StructuredName> items, StructuredName model)
        {
            return items.Any(x => x.FullName.InvariantEquals(model.FullName));
        }

        internal static bool SimpleNameExists(this IEnumerable<StructuredName> items, string name)
        {
            return items.Any(x => x.FullName.InvariantEquals(name));
        }

        internal static bool SuffixedNameExists(this IEnumerable<StructuredName> items)
        {
            return items.Any(x => x.Suffix.HasValue);
        }
    }
}
