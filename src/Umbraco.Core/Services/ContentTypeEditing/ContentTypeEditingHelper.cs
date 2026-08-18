using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Shared helper logic for content type editing.
/// </summary>
internal static class ContentTypeEditingHelper
{
    /// <summary>
    ///     Gets the combined, full effective property alias set (own properties plus everything each one gets from
    ///     its own compositions) of every content type descending from <paramref name="source"/>, across both the
    ///     tree inheritance and composition axes - excluding whatever already flows into those descendants through
    ///     <paramref name="source"/>'s own current state.
    /// </summary>
    /// <param name="source">The content type to find descendants of, or <c>null</c> when there are none (e.g. creating a new content type).</param>
    /// <param name="allContentTypes">All existing content type compositions, used to resolve the descendant tree.</param>
    /// <returns>The property aliases (compared case-insensitively) already effective on every descendant of <paramref name="source"/>, excluding aliases <paramref name="source"/> itself already contributes.</returns>
    /// <remarks>
    ///     Inheritance children hold their parent in their own <see cref="IContentTypeComposition.ContentTypeComposition"/>,
    ///     so walking the composition graph downward captures inheritance and composition descendants alike.
    /// </remarks>
    internal static HashSet<string> GetPropertyAliasesReservedByDescendants(this IContentTypeComposition? source, IContentTypeComposition[] allContentTypes)
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
        var descendantPropertyAliases = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
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
                    descendantPropertyAliases.Add(descendantPropertyType.Alias);
                }

                stack.Push(descendant.Id);
            }
        }

        descendantPropertyAliases.ExceptWith(source.CompositionPropertyTypes.Select(pt => pt.Alias));

        return descendantPropertyAliases;
    }
}
