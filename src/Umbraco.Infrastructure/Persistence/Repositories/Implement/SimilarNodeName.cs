using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Extensions;
using static Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.SimilarNodeName;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal static class ListExtensions
{
    internal static bool Contains(this IEnumerable<StructuredName> items, StructuredName model) =>
        items.Any(x => x.FullName.InvariantEquals(model.FullName));

    internal static bool SimpleNameExists(this IEnumerable<StructuredName> items, string name) =>
        items.Any(x => x.FullName.InvariantEquals(name));

    internal static bool SuffixedNameExists(this IEnumerable<StructuredName> items) =>
        items.Any(x => x.Suffix.HasValue);
}

internal class SimilarNodeName
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public static string? GetUniqueName(IEnumerable<SimilarNodeName> names, int nodeId, string? nodeName)
    {
        IEnumerable<string?> items = names
            .Where(x => x.Id != nodeId) // ignore same node
            .Select(x => x.Name);

        var uniqueName = GetUniqueName(items, nodeName);

        return uniqueName;
    }

    public static string? GetUniqueName(IEnumerable<string?> names, string? name)
    {
        var model = new StructuredName(name);
        IEnumerable<StructuredName> items = names
            .Where(x => x?.InvariantStartsWith(model.Text) ?? false) // ignore non-matching names
            .Select(x => new StructuredName(x)).ToArray();

        // name is empty, and there are no other names with suffixes, so just return " (1)"
        if (model.IsEmptyName() && !items.Any())
        {
            model.Suffix = StructuredName.Initialsuffix;

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
            model.Suffix = StructuredName._nosuffix;

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
            model.Suffix = StructuredName._nosuffix;

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
            model.Suffix = StructuredName._nosuffix;

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
        var current = 1;
        foreach (StructuredName item in items.OrderBy(x => x.Suffix))
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
        internal const uint Initialsuffix = 1;
        private const string Spacecharacter = " ";
        private const string Suffixedpattern = @"(.*) \(([1-9]\d*)\)$";
        internal static readonly uint? _nosuffix = default;

        internal StructuredName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Text = Spacecharacter;

                return;
            }

            var rg = new Regex(Suffixedpattern);
            MatchCollection matches = rg.Matches(name);
            if (matches.Count > 0)
            {
                Match match = matches[0];
                Text = match.Groups[1].Value;
                int number = int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
                    ? number
                    : 0;
                Suffix = (uint?)number;

                return;
            }

            Text = name;
        }

        public string FullName
        {
            get
            {
                var text = Text == Spacecharacter ? Text.Trim() : Text;

                return Suffix > 0 ? $"{text} ({Suffix})" : text;
            }
        }

        internal string Text { get; set; }

        internal uint? Suffix { get; set; }

        internal bool IsEmptyName() => string.IsNullOrWhiteSpace(Text);
    }
}
