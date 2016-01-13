using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public static class ContentTypeServiceExtensions
    {
        /// <summary>
        /// Returns the available composite content types for a given content type
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IContentTypeComposition> GetAvailableCompositeContentTypes(this IContentTypeService ctService,
            IContentTypeComposition source,
            IContentTypeComposition[] allContentTypes)
        {
            //below is all ported from the old doc type editor and comes with the same weaknesses /insanity / magic

            // note: there are many sanity checks missing here and there ;-((
            // make sure once and for all
            //if (allContentTypes.Any(x => x.ParentId > 0 && x.ContentTypeComposition.Any(y => y.Id == x.ParentId) == false))
            //    throw new Exception("A parent does not belong to a composition.");

            if (source != null)
            {
                // find out if any content type uses this content type
                var isUsing = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == source.Id)).ToArray();
                if (isUsing.Length > 0)
                {
                    //if already in use a composition, do not allow any composited types
                    return new List<IContentTypeComposition>();
                }
            }

            // if it is not used then composition is possible
            // hashset guarantees unicity on Id
            var list = new HashSet<IContentTypeComposition>(new DelegateEqualityComparer<IContentTypeComposition>(
                (x, y) => x.Id == y.Id,
                x => x.Id));

            // usable types are those that are top-level
            var usableContentTypes = allContentTypes
                .Where(x => x.ContentTypeComposition.Any() == false).ToArray();
            foreach (var x in usableContentTypes)
                list.Add(x);

            // indirect types are those that we use, directly or indirectly
            var indirectContentTypes = GetDirectOrIndirect(source).ToArray();
            foreach (var x in indirectContentTypes)
                list.Add(x);

            //// directContentTypes are those we use directly
            //// they are already in indirectContentTypes, no need to add to the list
            //var directContentTypes = source.ContentTypeComposition.ToArray();

            //var enabled = usableContentTypes.Select(x => x.Id) // those we can use
            //    .Except(indirectContentTypes.Select(x => x.Id)) // except those that are indirectly used
            //    .Union(directContentTypes.Select(x => x.Id)) // but those that are directly used
            //    .Where(x => x != source.ParentId) // but not the parent
            //    .Distinct()
            //    .ToArray();

            return list
                .Where(x => x.Id != (source != null ? source.Id : 0))
                .OrderBy(x => x.Name)                
                .ToList();
        }

        /// <summary>
        /// Get those that we use directly or indirectly
        /// </summary>
        /// <param name="ctype"></param>
        /// <returns></returns>
        private static IEnumerable<IContentTypeComposition> GetDirectOrIndirect(IContentTypeComposition ctype)
        {
            // hashset guarantees unicity on Id
            var all = new HashSet<IContentTypeComposition>(new DelegateEqualityComparer<IContentTypeComposition>(
                (x, y) => x.Id == y.Id,
                x => x.Id));

            var stack = new Stack<IContentTypeComposition>();

            if (ctype != null)
            {
                foreach (var x in ctype.ContentTypeComposition)
                    stack.Push(x);
            }

            while (stack.Count > 0)
            {
                var x = stack.Pop();
                all.Add(x);
                foreach (var y in x.ContentTypeComposition)
                    stack.Push(y);
            }

            return all;
        }

    }
}