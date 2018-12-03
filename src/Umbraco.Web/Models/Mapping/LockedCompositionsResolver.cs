using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.Mapping
{
    internal class LockedCompositionsResolver
    {
        private readonly IContentTypeService _contentTypeService;

        public LockedCompositionsResolver(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public IEnumerable<string> Resolve(IContentTypeComposition source)
        {
            var aliases = new List<string>();
            // get ancestor ids from path of parent if not root
            if (source.ParentId != Constants.System.Root)
            {
                var parent = _contentTypeService.Get(source.ParentId);
                if (parent != null)
                {
                    var ancestorIds = parent.Path.Split(',').Select(int.Parse);
                    // loop through all content types and return ordered aliases of ancestors
                    var allContentTypes = _contentTypeService.GetAll().ToArray();
                    foreach (var ancestorId in ancestorIds)
                    {
                        var ancestor = allContentTypes.FirstOrDefault(x => x.Id == ancestorId);
                        if (ancestor != null)
                        {
                            aliases.Add(ancestor.Alias);
                        }
                    }
                }
            }
            return aliases.OrderBy(x => x);
        }
    }
}
