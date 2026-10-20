using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Represents the name of a node that is similar to another, and provides the logic for resolving a
/// unique name among a set of siblings by appending or incrementing a " (n)" suffix.
/// </summary>
internal sealed partial class SimilarNodeName
{
    /// <summary>
    /// Gets or sets the unique identifier for this SimilarNodeName instance.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of a node that is considered similar, typically used for comparison or validation purposes.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Returns the base text of a name, i.e. the name with any trailing " (n)" suffix removed. This
    /// matches the base text that <see cref="GetUniqueName(IEnumerable{string?}, string?)"/> compares
    /// siblings against, so it can be used to fetch only the siblings that could actually collide.
    /// </summary>
    /// <param name="name">The name to extract the base text from.</param>
    /// <returns>The name with any trailing " (n)" suffix removed.</returns>
    internal static string GetBaseText(string? name) => new StructuredName(name).Text;

    /// <summary>
    /// Returns a unique node name by comparing the proposed name against a collection of similar node names, excluding the node with the specified ID.
    /// </summary>
    /// <param name="names">A collection of <see cref="SimilarNodeName"/> objects to check for name uniqueness.</param>
    /// <param name="nodeId">The ID of the node to exclude from the uniqueness check (typically the current node).</param>
    /// <param name="nodeName">The proposed node name to validate and make unique if necessary.</param>
    /// <returns>A unique node name based on <paramref name="nodeName"/>, or <c>null</c> if a unique name cannot be generated.</returns>
    public static string? GetUniqueName(IEnumerable<SimilarNodeName> names, int nodeId, string? nodeName)
    {
        IEnumerable<string?> items = names
            .Where(x => x.Id != nodeId) // ignore same node
            .Select(x => x.Name);

        var uniqueName = GetUniqueName(items, nodeName);

        return uniqueName;
    }

    /// <summary>
    /// Returns a unique name by comparing the proposed name against a collection of existing names,
    /// appending or incrementing a " (n)" suffix as required to avoid a collision.
    /// </summary>
    /// <param name="names">The existing names to compare against.</param>
    /// <param name="name">The proposed name to make unique.</param>
    /// <returns>A unique name derived from <paramref name="name"/>.</returns>
    public static string? GetUniqueName(IEnumerable<string?> names, string? name)
    {
        var model = new StructuredName(name);
        IEnumerable<StructuredName> items = names
            .Where(x => x?.InvariantStartsWith(model.Text) ?? false) // ignore non-matching names
            .Select(x => new StructuredName(x))
            .ToArray();

        if (model.IsEmptyName())
        {
            return ResolveEmptyName(model, items);
        }

        return model.Suffix.HasValue
            ? ResolveSuffixedName(model, items)
            : ResolveUnsuffixedName(model, items);
    }

    private static string ResolveEmptyName(StructuredName model, IEnumerable<StructuredName> items)
    {
        if (!items.Any())
        {
            // Nothing to clash with, so the first duplicate suffix (" (1)") is enough.
            model.Suffix = StructuredName.Initialsuffix;
        }
        else if (SuffixedNameExists(items))
        {
            model.Suffix = (uint?)GetSuffixNumber(items);
        }

        return model.FullName;
    }

    private static string ResolveUnsuffixedName(StructuredName model, IEnumerable<StructuredName> items)
    {
        if (!SimpleNameExists(items, model.Text))
        {
            // The plain name is free.
            model.Suffix = StructuredName._nosuffix;
        }
        else
        {
            // The plain name is taken, so take the next free suffix (or the first, if none exist yet).
            model.Suffix = (uint?)(SuffixedNameExists(items)
                ? GetSuffixNumber(items)
                : GetFirstSuffix(items));
        }

        return model.FullName;
    }

    private static string ResolveSuffixedName(StructuredName model, IEnumerable<StructuredName> items)
    {
        // The suffixed name itself is free, so there is no conflict.
        if (!SimpleNameExists(items, model.FullName))
        {
            return model.FullName;
        }

        // The base name (without the supplied suffix) also exists, so simply take the next suffix.
        if (SimpleNameExists(items, model.Text))
        {
            model.Suffix = (uint?)GetSuffixNumber(items);

            return model.FullName;
        }

        // The user supplied a suffix that has no base name of its own, so treat the whole thing as
        // the base name and add a secondary suffix.
        model.Text = model.FullName;
        model.Suffix = StructuredName._nosuffix;
        items = items.Where(x => x.Text.InvariantStartsWith(model.FullName));
        model.Suffix = (uint?)GetFirstSuffix(items);

        return model.FullName;
    }

    private static bool SimpleNameExists(IEnumerable<StructuredName> items, string name) =>
        items.Any(x => x.FullName.InvariantEquals(name));

    private static bool SuffixedNameExists(IEnumerable<StructuredName> items) =>
        items.Any(x => x.Suffix.HasValue);

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

    /// <summary>
    /// Represents a name split into its base text and an optional trailing " (n)" numeric suffix.
    /// </summary>
    internal sealed partial class StructuredName
    {
        /// <summary>
        /// The suffix applied to the first duplicate of a name (i.e. the "1" in "Name (1)").
        /// </summary>
        internal const uint Initialsuffix = 1;

        private const string Spacecharacter = " ";

        [GeneratedRegex(@"(.*) \(([1-9]\d*)\)$")]
        private static partial Regex SuffixedPatternRegex();

        /// <summary>
        /// Represents the absence of a numeric suffix.
        /// </summary>
        internal static readonly uint? _nosuffix = default;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredName"/> class, splitting
        /// <paramref name="name"/> into its base text and any trailing " (n)" numeric suffix.
        /// </summary>
        /// <param name="name">The name to parse.</param>
        internal StructuredName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Text = Spacecharacter;

                return;
            }

            MatchCollection matches = SuffixedPatternRegex().Matches(name);
            if (matches.Count > 0)
            {
                Match match = matches[0];
                Text = match.Groups[1].Value;
                int number = int.TryParse(match.Groups[2].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
                    ? number
                    : 0;
                Suffix = (uint?)number;

                return;
            }

            Text = name;
        }

        /// <summary>
        /// Gets the full name of the node, including the suffix in parentheses if the suffix is greater than zero.
        /// For example, returns "NodeName (2)" if the suffix is 2, otherwise just the trimmed node name.
        /// </summary>
        public string FullName
        {
            get
            {
                var text = Text == Spacecharacter ? Text.Trim() : Text;

                return Suffix > 0 ? $"{text} ({Suffix})" : text;
            }
        }

        /// <summary>
        /// Gets or sets the base text of the name, i.e. the name without its numeric suffix.
        /// </summary>
        internal string Text { get; set; }

        /// <summary>
        /// Gets or sets the numeric suffix parsed from the name, or <c>null</c> when the name has none.
        /// </summary>
        internal uint? Suffix { get; set; }

        /// <summary>
        /// Returns a value indicating whether the name has no meaningful base text.
        /// </summary>
        /// <returns><c>true</c> if the base text is null or whitespace; otherwise, <c>false</c>.</returns>
        internal bool IsEmptyName() => string.IsNullOrWhiteSpace(Text);
    }
}
