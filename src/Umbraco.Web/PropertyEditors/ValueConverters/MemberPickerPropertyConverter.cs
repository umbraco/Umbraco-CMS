using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Extensions;
using Umbraco.Web.Security;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    [PropertyValueType(typeof(IPublishedContent))]
    [PropertyValueCache(PropertyCacheValue.Object, PropertyCacheLevel.ContentCache)]
    [PropertyValueCache(PropertyCacheValue.Source, PropertyCacheLevel.Content)]
    [PropertyValueCache(PropertyCacheValue.XPath, PropertyCacheLevel.Content)]
    public class MemberPickerPropertyConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (UmbracoConfig.For.UmbracoSettings().Content.EnablePropertyValueConverters)
            {
                return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.MemberPickerAlias)
                    || propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.MemberPicker2Alias);
            }
            return false;
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
                return attemptConvertInt.Result;
            var attemptConvertUdi = source.TryConvertTo<Udi>();
            if (attemptConvertUdi.Success)
                return attemptConvertUdi.Result;
            return null;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
                return null;

            if (UmbracoContext.Current != null)
            {
                IPublishedContent member;
                if (source is int)
                {
                    var membershipHelper = new MembershipHelper(UmbracoContext.Current);
                    member = membershipHelper.GetById((int)source);
                    if (member != null)
                        return member;
                }
                else
                {
                    var sourceUdi = source as Udi;
                    member = sourceUdi.ToPublishedContent();
                    if (member != null)
                        return member;
                }
            }

            return source;
        }

    }
}
