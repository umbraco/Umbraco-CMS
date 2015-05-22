using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class AvailableCompositeContentTypesResolver : ValueResolver<IContentType, IEnumerable<EntityBasic>>
    {
        private ApplicationContext _context;
        private bool _mediaType;
        internal AvailableCompositeContentTypesResolver(ApplicationContext context, bool mediaType = false)
        {
            _context = context;
            _mediaType = mediaType;
        }

        protected override IEnumerable<EntityBasic> ResolveCore(IContentType source)
        {
            //below is all ported from the old doc type editor and comes with the same weaknesses /insanity / magic

            //get all types
            var allContentTypes = _mediaType
                    ? _context.Services.ContentTypeService.GetAllMediaTypes().Cast<IContentTypeComposition>().ToArray()
                    : _context.Services.ContentTypeService.GetAllContentTypes().Cast<IContentTypeComposition>().ToArray();

            // note: there are many sanity checks missing here and there ;-((
            // make sure once and for all
            //if (allContentTypes.Any(x => x.ParentId > 0 && x.ContentTypeComposition.Any(y => y.Id == x.ParentId) == false))
            //    throw new Exception("A parent does not belong to a composition.");

            // find out if any content type uses this content type
            var isUsing = allContentTypes.Where(x => x.ContentTypeComposition.Any(y => y.Id == source.Id)).ToArray();
            if (isUsing.Length > 0)
            {
                //if already in use a composition, do not allow any composited types
                return new List<EntityBasic>();
            }
            else
            {
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
                var indirectContentTypes = GetIndirect(source).ToArray();
                foreach (var x in indirectContentTypes)
                    list.Add(x);

                // directContentTypes are those we use directly
                // they are already in indirectContentTypes, no need to add to the list
                var directContentTypes = source.ContentTypeComposition.ToArray();

                var enabled = usableContentTypes.Select(x => x.Id) // those we can use
                    .Except(indirectContentTypes.Select(x => x.Id)) // except those that are indirectly used
                    .Union(directContentTypes.Select(x => x.Id)) // but those that are directly used
                    .Where(x => x != source.ParentId) // but not the parent
                    .Distinct()
                    .ToArray();

                var wtf = new List<EntityBasic>();
                foreach (var contentType in list.OrderBy(x => x.Name).Where(x => x.Id != source.Id))
                    wtf.Add(Mapper.Map<IContentTypeComposition, EntityBasic>(contentType));

                return wtf;
            }
        }


        private IEnumerable<IContentTypeComposition> GetIndirect(IContentTypeComposition ctype)
        {
            // hashset guarantees unicity on Id
            var all = new HashSet<IContentTypeComposition>(new DelegateEqualityComparer<IContentTypeComposition>(
                (x, y) => x.Id == y.Id,
                x => x.Id));

            var stack = new Stack<IContentTypeComposition>();

            foreach (var x in ctype.ContentTypeComposition)
                stack.Push(x);

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
