using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core
{
    public class PublishedContentTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            var convertableTypes = new[]
            {
                typeof(int)
            };

            return convertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
                   || base.CanConvertFrom(context, destinationType);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            var publishedContent = value as IPublishedContent;
            if (publishedContent == null)
                return null;

            if (TypeHelper.IsTypeAssignableFrom<int>(destinationType))
            {
                return publishedContent.Id;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
