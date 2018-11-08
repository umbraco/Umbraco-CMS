using AutoMapper;
using LightInject;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DefaultTemplateResolver : IValueResolver<IContent, ContentItemDisplay, string>
    {
        public string Resolve(IContent source, ContentItemDisplay destination, string destMember, ResolutionContext context)
        {
            var fileService = DependencyResolver.Current.GetService<IFileService>();
            if (source == null)
                return null;

            // If no template id was set return default template.
            if (source.TemplateId == 0 && !string.IsNullOrWhiteSpace(source.ContentType.DefaultTemplate?.Alias))
            {
                var defaultTemplate = fileService.GetTemplate(source.ContentType.DefaultTemplate.Alias);
                return defaultTemplate.Alias;
            }

            var template = fileService.GetTemplate(source.TemplateId);

            return template.Alias;
        }
    }
}
