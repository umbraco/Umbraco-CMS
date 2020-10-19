using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static Umbraco.Core.Persistence.Repositories.Implement.SimilarNodeName;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class SimilarNodeName
    {
        const string SPACE_CHARACTER = " ";

        public int Id { get; set; }
        public string Name { get; set; }

        // gets a unique name
        public static string GetUniqueName(IEnumerable<SimilarNodeName> names, int nodeId, string nodeName)
        {
            var cleanName = string.IsNullOrWhiteSpace(nodeName) ? SPACE_CHARACTER : nodeName;

            var items = names
                    .Where(x => x.Id != nodeId) // ignore same node
                    .Select(x => x.Name);

            var uniqueName = GetUniqueName(items, cleanName);

            return uniqueName;
        }


        public static string GetUniqueName(IEnumerable<string> names, string name)
        {
            var model = new StructuredName(name);
            var items = names
                    .Where(x => x.InvariantStartsWith(model.Text)) // ignore non-matching names
                    .Select(x => new StructuredName(x));

            // name is empty, and there are no other names with suffixes, so just return " (1)"
            if (model.IsEmptyName() && !items.Any())
            {
                model.SetSuffix(1);

                return model.FullName;
            }

            // name is empty, and there are other names with suffixes
            if (model.IsEmptyName() && items.SuffixedNameExists())
            {
                var emptyNameSuffix = GetSuffixNumber(items);

                if (emptyNameSuffix > 0)
                {
                    model.SetSuffix(emptyNameSuffix);

                    return model.FullName;
                }
            }

            //  no suffix - name without suffix does NOT exist, AND name with suffix does NOT exist
            if (!model.HasSuffix() && !items.SimpleNameExists(model.Text) && !items.SuffixedNameExists())
            {
                model.SetNoSuffix();

                return model.FullName;
            }

            // no suffix - name without suffix exists, however name with suffix does NOT exist
            if (!model.HasSuffix() && items.SimpleNameExists(model.Text) && !items.SuffixedNameExists())
            {
                var firstSuffix = GetFirstSuffix(items);
                model.SetSuffix(firstSuffix);

                return model.FullName;
            }

            // no suffix - name without suffix exists, AND name with suffix does exist
            if (!model.HasSuffix() && items.SimpleNameExists(model.Text) && items.SuffixedNameExists())
            {
                var nextSuffix = GetSuffixNumber(items);
                model.SetSuffix(nextSuffix);

                return model.FullName;
            }

            // no suffix - name without suffix does NOT exist, however name with suffix exists
            if (!model.HasSuffix() && !items.SimpleNameExists(model.Text) && items.SuffixedNameExists())
            {
                var nextSuffix = GetSuffixNumber(items);
                model.SetSuffix(nextSuffix);

                return model.FullName;
            }

            // has suffix - name without suffix exists
            if (model.HasSuffix() && items.SimpleNameExists(model.Text))
            {
                var nextSuffix = GetSuffixNumber(items);
                model.SetSuffix(nextSuffix);

                return model.FullName;
            }

            // has suffix - name without suffix does NOT exist
            // a case where the user added the suffix, so add a secondary suffix
            if (model.HasSuffix() && !items.SimpleNameExists(model.Text))
            {
                model.SetText(model.FullName);
                model.SetNoSuffix();

                // filter items based on full name with suffix
                items = items.Where(x => x.Text.InvariantStartsWith(model.FullName));
                var secondarySuffix = GetFirstSuffix(items);
                model.SetSuffix(secondarySuffix);

                return model.FullName;
            }

            // has suffix - name without suffix also exists, therefore we simply increment
            if (model.HasSuffix() && items.SimpleNameExists(model.Text))
            {
                var nextSuffix = GetSuffixNumber(items);
                model.SetSuffix(nextSuffix);

                return model.FullName;
            }

            return name;
        }

        private static int GetFirstSuffix(IEnumerable<StructuredName> items)
        {
            const int suffixStart = 1;

            if (!items.Any(x => x.Number == suffixStart))
            {
                // none of the suffixes are the same as suffixStart, so we can use suffixStart!
                return suffixStart;
            }

            return GetSuffixNumber(items);
        }

        private static int GetSuffixNumber(IEnumerable<StructuredName> items)
        {
            int current = 1;
            foreach (var item in items.OrderBy(x => x.Number))
            {
                if (item.Number == current)
                {
                    current++;
                }
                else if (item.Number > current)
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
            private const string Suffixed_Pattern = @"(.*)\s\((\d+)\)$";
            internal string Text { get; private set; }
            internal int Number { get; private set; }
            public string FullName
            {
                get
                {
                    string text = string.IsNullOrWhiteSpace(Text) ? Text.Trim() : Text;

                    return Number > 0 ? $"{text} ({Number})" : text;
                }
            }

            internal StructuredName(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    Text = SPACE_CHARACTER;
                    Number = 0;

                    return;
                }

                var rg = new Regex(Suffixed_Pattern);
                var matches = rg.Matches(name);
                if (matches.Count > 0)
                {
                    var match = matches[0];
                    Text = match.Groups[1].Value;
                    int number;
                    Number = int.TryParse(match.Groups[2].Value, out number) ? number : 0;

                    return;
                }
                else
                {
                    Text = name;
                    Number = 0;
                }
            }

            internal bool HasSuffix()
            {
                return Number > 0;
            }

            internal void SetSuffix(int number)
            {
                Number = number;
            }

            internal void SetNoSuffix()
            {
                Number = 0;
            }

            internal void SetText(string text)
            {
                Text = text;
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
            return items.Any(x => x.HasSuffix());
        }
    }
}
