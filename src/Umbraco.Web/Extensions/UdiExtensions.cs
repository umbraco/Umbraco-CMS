using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Extensions
{
    public static class UdiExtensions
    {
        /// <summary>
        /// An extension method to easily acquire a typed version of content, media or member item for a given Udi
        /// </summary>
        /// <param name="udi"></param>
        /// <returns>An <see cref="IPublishedContent"/> item if the item is a Document, Media or Member</returns>
        public static IPublishedContent ToPublishedContent(this Udi udi)
        {
            Udi identifier;
            if (Udi.TryParse(udi.ToString(), out identifier) == false)
                return null;

            var guidUdi = GuidUdi.Parse(udi.ToString());
            var umbracoType = Constants.UdiEntityType.ToUmbracoObjectType(identifier.EntityType);

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var entityService = ApplicationContext.Current.Services.EntityService;
            switch (umbracoType)
            {
                case UmbracoObjectTypes.Document:
                    return umbracoHelper.TypedContent(guidUdi.Guid);
                case UmbracoObjectTypes.Media:
                    var mediaAttempt = entityService.GetIdForKey(guidUdi.Guid, umbracoType);
                    if (mediaAttempt.Success)
                        return umbracoHelper.TypedMedia(mediaAttempt.Result);
                    break;
                case UmbracoObjectTypes.Member:
                    var memberAttempt = entityService.GetIdForKey(guidUdi.Guid, umbracoType);
                    if (memberAttempt.Success)
                        return umbracoHelper.TypedMember(memberAttempt.Result);
                    break;
            }
            
            return null;
        }
    }
}
