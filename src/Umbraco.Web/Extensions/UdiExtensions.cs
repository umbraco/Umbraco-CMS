using System;
using System.ComponentModel;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Extensions
{
    [Obsolete("Use methods on UmbracoHelper instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UdiExtensions
    {
        [Obsolete("Use methods on UmbracoHelper instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IPublishedContent ToPublishedContent(this Udi udi)
        {
            var guidUdi = udi as GuidUdi;
            if (guidUdi == null) return null;

            var umbracoType = Constants.UdiEntityType.ToUmbracoObjectType(guidUdi.EntityType);

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
