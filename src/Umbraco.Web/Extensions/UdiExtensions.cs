using System;
using System.ComponentModel;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Security;

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

            var umbracoContext = UmbracoContext.Current;
            if (umbracoContext == null) return null;

            var umbracoType = Constants.UdiEntityType.ToUmbracoObjectType(guidUdi.EntityType);

            switch (umbracoType)
            {
                case UmbracoObjectTypes.Document:
                    return umbracoContext.ContentCache.GetById(guidUdi.Guid);
                case UmbracoObjectTypes.Media:
                    return umbracoContext.MediaCache.GetById(guidUdi.Guid);
                case UmbracoObjectTypes.Member:
                    var membershipHelper = new MembershipHelper(umbracoContext);
                    return membershipHelper.GetByProviderKey(guidUdi.Guid);
            }

            return null;
        }
    }
}
