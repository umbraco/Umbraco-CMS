using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    internal class LockedCompositionsResolver : ValueResolver<IContentTypeComposition, IEnumerable<string>>
    {
        private readonly ApplicationContext _applicationContext;

        public LockedCompositionsResolver(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        protected override IEnumerable<string> ResolveCore(IContentTypeComposition source)
        {
            // get ancestor ids from path (exclude current node id)
            var ancestorIds = source.Path.Substring(0, source.Path.LastIndexOf(',')).Split(',').Select(int.Parse);
            var aliases = new List<string>();
            // loop through all content types and return ordered aliases of ancestors
            var allContentTypes = _applicationContext.Services.ContentTypeService.GetAllContentTypes().ToArray();
            foreach (var ancestorId in ancestorIds)
            {
                var ancestor = allContentTypes.FirstOrDefault(x => x.Id == ancestorId);
                if (ancestor != null)
                {
                    aliases.Add(ancestor.Alias);
                }
            }
            return aliases.OrderBy(x => x);
        }
    }
}
