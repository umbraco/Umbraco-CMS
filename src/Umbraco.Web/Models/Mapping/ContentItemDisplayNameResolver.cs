//using AutoMapper;
//using Umbraco.Core;
//using Umbraco.Core.Models;
//using Umbraco.Web.Models.ContentEditing;
//using ContentVariation = Umbraco.Core.Models.ContentVariation;

// fixme remove this file?
//namespace Umbraco.Web.Models.Mapping
//{
//    /// <summary>
//    /// Used to map the <see cref="TabbedContentItem{ContentPropertyDisplay, IContent}"/> name from an <see cref="IContent"/> depending on it's variation settings
//    /// </summary>
//    internal class ContentItemDisplayNameResolver : IValueResolver<IContent, ContentVariationDisplay, string>
//    {
//        public string Resolve(IContent source, ContentVariationDisplay destination, string destMember, ResolutionContext context)
//        {
//            var culture = context.GetCulture();
//            return source.ContentType.VariesByCulture() && culture != null
//                ? source.GetCultureName(culture)
//                : source.Name;
//        }
//    }
//}
