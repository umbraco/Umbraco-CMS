using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DefaultTemplateResolver : IValueResolver<IContent, ContentItemDisplay, string>
    {
        public string Resolve(IContent source, ContentItemDisplay destination, string destMember, ResolutionContext context)
        {
            if (source == null || source.Template == null) return null;

            var alias = source.Template.Alias;

            //set default template if template isn't set
            if (string.IsNullOrEmpty(alias))
                alias = source.ContentType.DefaultTemplate == null
                    ? string.Empty
                    : source.ContentType.DefaultTemplate.Alias;

            return alias;
        }
    }
}