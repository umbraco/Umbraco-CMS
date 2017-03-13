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
                switch (propertyType.PropertyEditorAlias)
                {
                    case Constants.PropertyEditors.MemberPickerAlias:
                        int sourceInt;
                        if (int.TryParse(source.ToString(), out sourceInt))
                        {
                            var membershipHelper = new MembershipHelper(UmbracoContext.Current);
                            member = membershipHelper.GetById((int)source);
                            return member;
                        }
                        break;
                    case Constants.PropertyEditors.MemberPicker2Alias:
                        Udi udi;
                        if (Udi.TryParse(source.ToString(), out udi))
                        {
                            member = udi.ToPublishedContent();
                            if (member != null)
                                return member;
                        }
                        break;
                }

            }

            return source;
        }

    }
}
