using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using ContentVariation = Umbraco.Core.Models.ContentVariation;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Used to map the <see cref="ContentItemDisplay"/> name from an <see cref="IContent"/> depending on it's variation settings
    /// </summary>
    internal class ContentItemDisplayNameResolver : IValueResolver<IContent, ContentItemDisplay, string>
    {
        public string Resolve(IContent source, ContentItemDisplay destination, string destMember, ResolutionContext context)
        {
            var culture = context.GetCulture();
            if (culture != null && source.ContentType.Variations.Has(ContentVariation.CultureNeutral))
            {
                //return the culture name being requested
                return source.GetName(culture);
            }
            return source.Name;
        }
    }
}
