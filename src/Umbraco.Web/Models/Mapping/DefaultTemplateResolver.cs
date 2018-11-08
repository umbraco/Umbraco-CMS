using AutoMapper;
using LightInject;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DefaultTemplateResolver : IValueResolver<IContent, ContentItemDisplay, string>
    {
        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        [Inject]
        public ServiceContext Services { get; set; }

        public string Resolve(IContent source, ContentItemDisplay destination, string destMember, ResolutionContext context)
        {
            if (source == null)
                return null;

            // If no template id was set return default template.
            if (source.TemplateId == 0 && !string.IsNullOrWhiteSpace(source.ContentType.DefaultTemplate?.Alias))
            {
                var defaultTemplate = Services.FileService.GetTemplate(source.ContentType.DefaultTemplate.Alias);
                return defaultTemplate.Alias;
            }

            var template = Services.FileService.GetTemplate(source.TemplateId);

            return template.Alias;
        }
    }
}
