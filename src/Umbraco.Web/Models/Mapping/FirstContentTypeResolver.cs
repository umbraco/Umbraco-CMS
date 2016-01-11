using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Models.Mapping
{
    internal class FirstContentTypeResolver : ValueResolver<IContentType, bool>
    {
        private readonly Lazy<IContentTypeService> _contentTypeService;

        public FirstContentTypeResolver(Lazy<IContentTypeService> contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        protected override bool ResolveCore(IContentType source)
        {
            var contentTypes = _contentTypeService.Value.GetAllContentTypes();
            return contentTypes.Any() == false;
        }
    }
}