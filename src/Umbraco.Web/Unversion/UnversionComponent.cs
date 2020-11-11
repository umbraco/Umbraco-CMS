using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Umbraco.Web.Unversion
{
    public class UnversionComponent : IComponent
    {
        private readonly IUnversionService unVersionService;

        public UnversionComponent(IUnversionService _unVersionService)
        {
            unVersionService = _unVersionService;
        }

        public void Initialize()
        {
            ContentService.Published += ContentService_Published;
        }

        private void ContentService_Published(IContentService sender, ContentPublishedEventArgs e)
        {
            foreach (var content in e.PublishedEntities)
            {
                unVersionService.Unversion(content);
            }
        }

        public void Terminate()
        {
            ContentService.Published -= ContentService_Published;
        }
    }
}
