using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentTypeModelMapper
    {
        private readonly ApplicationContext _applicationContext;

        public ContentTypeModelMapper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public ContentTypeBasic ToContentTypeBasic(IContentType contentType)
        {
            return new ContentTypeBasic
                {
                    Alias = contentType.Alias,
                    Id = contentType.Id,
                    Description = contentType.Description,
                    Icon = contentType.Icon,
                    Name = contentType.Name
                };
        }
    }

    internal class MediaTypeModelMapper
    {
        private readonly ApplicationContext _applicationContext;

        public MediaTypeModelMapper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public ContentTypeBasic ToMediaTypeBasic(IMediaType contentType)
        {
            return new ContentTypeBasic
            {
                Alias = contentType.Alias,
                Id = contentType.Id,
                Description = contentType.Description,
                Icon = contentType.Icon,
                Name = contentType.Name
            };
        }
    }
}