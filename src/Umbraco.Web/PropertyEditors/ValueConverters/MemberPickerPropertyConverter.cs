using System;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Security;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MemberPickerPropertyConverter : PropertyValueConverterBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public MemberPickerPropertyConverter(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.MemberPickerAlias)
                || propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.MemberPicker2Alias);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
        {
            return PropertyCacheLevel.Facade;
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IPublishedContent);
        }

        public override object ConvertSourceToInter(PublishedPropertyType propertyType, object source, bool preview)
        {
            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
                return attemptConvertInt.Result;
            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        public override object ConvertInterToObject(PublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            if (source == null)
                return null;

            if (UmbracoContext.Current != null)
            {
                IPublishedContent member;
                var membershipHelper = new MembershipHelper(_umbracoContextAccessor.UmbracoContext);
                if (source is int)
                {
                    member = membershipHelper.GetById((int)source);
                    if (member != null)
                        return member;
                }
                else
                {
                    var sourceUdi = source as Udi;
                    member = membershipHelper.Get(sourceUdi);
                    if (member != null)
                        return member;
                }
            }

            return source;
        }
    }
}
