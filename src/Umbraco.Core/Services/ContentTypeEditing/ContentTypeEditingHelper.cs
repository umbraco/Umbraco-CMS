using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Shared helper logic for content type editing.
/// </summary>
internal static class ContentTypeEditingHelper
{
    /// <summary>
    ///     Gets the property aliases on a <paramref name="contentType"/>: its own properties plus everything it gets
    ///     from its current compositions and inheritance.
    /// </summary>
    /// <param name="contentType">The content type to get the effective property aliases of, or <c>null</c> when there is none yet (e.g. creating a new content type).</param>
    /// <returns>The property aliases already effective on <paramref name="contentType"/>.</returns>
    internal static HashSet<string> GetAllPropertyAliases(this IContentTypeComposition? contentType)
    {
        if (contentType is null)
        {
            return [];
        }

        return
        [
            .. contentType.PropertyTypes
                .Select(pt => pt.Alias)
                .Concat(
                    contentType.ContentTypeComposition.SelectMany(c =>
                        c.CompositionPropertyTypes.Select(pt => pt.Alias)))
        ];
    }

    /// <summary>
    ///     Gets the combined, full effective property alias set (own properties plus everything each one gets from
    ///     its own compositions) of every content type descending from <paramref name="source"/>, across both the
    ///     tree inheritance and composition axes.
    /// </summary>
    /// <param name="source">The content type to find descendants of, or <c>null</c> when there are none (e.g. creating a new content type).</param>
    /// <param name="allContentTypes">All existing content type compositions, used to resolve the descendant tree.</param>
    /// <returns>The lowercased property aliases already effective on every descendant of <paramref name="source"/>.</returns>
    /// <remarks>
    ///     Inheritance children hold their parent in their own <see cref="IContentTypeComposition.ContentTypeComposition"/>,
    ///     so walking the composition graph downward captures inheritance and composition descendants alike.
    /// </remarks>
    internal static HashSet<string> GetAllDescendantPropertyAliases(this IContentTypeComposition? source, IContentTypeComposition[] allContentTypes)
    {
        if (source is null)
        {
            return [];
        }

        // build a "referenced id -> types that directly reference it" lookup once, so the traversal is O(n + d)
        // rather than rescanning every content type for each descendant
        ILookup<int, IContentTypeComposition> directReferencingTypes = allContentTypes
            .SelectMany(
                contentType => contentType.ContentTypeComposition,
                (contentType, referenced) => (ReferencedId: referenced.Id, ContentType: contentType))
            .ToLookup(x => x.ReferencedId, x => x.ContentType);

        var descendantIds = new HashSet<int>();
        var descendantPropertyAliases = new HashSet<string>();
        var stack = new Stack<int>();
        stack.Push(source.Id);
        while (stack.Count > 0)
        {
            var currentId = stack.Pop();
            foreach (IContentTypeComposition descendant in directReferencingTypes[currentId])
            {
                if (!descendantIds.Add(descendant.Id))
                {
                    continue;
                }

                IEnumerable<IPropertyType> allProperties = descendant.PropertyTypes
                    .Concat(descendant.CompositionPropertyTypes);
                foreach (IPropertyType descendantPropertyType in allProperties)
                {
                    descendantPropertyAliases.Add(descendantPropertyType.Alias.ToLowerInvariant());
                }

                stack.Push(descendant.Id);
            }
        }

        return descendantPropertyAliases;
    }
}
