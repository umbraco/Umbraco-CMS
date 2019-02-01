using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services.Implement
{
    public class ContentTypeServiceBaseFactory : IContentTypeServiceBaseFactory
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMemberTypeService _memberTypeService;

        public ContentTypeServiceBaseFactory(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
        {
            _contentTypeService = contentTypeService;
            _mediaTypeService = mediaTypeService;
            _memberTypeService = memberTypeService;
        }

        public IContentTypeServiceBase Create(IContentBase contentBase)
        {
            switch (contentBase)
            {
                case IContent _:
                    return  _contentTypeService;
                case IMedia _:
                    return   _mediaTypeService;
                case IMember _:
                    return  _memberTypeService;
                default:
                    throw new ArgumentException($"Invalid contentBase type: {contentBase.GetType().FullName}" , nameof(contentBase));
            }
        }

    }
}
