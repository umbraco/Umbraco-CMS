using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    public class UserPickerValueConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        private readonly IUserService _userService;

        public UserPickerValueConverter()
        {
            _userService = ApplicationContext.Current.Services.UserService;
        }

        public UserPickerValueConverter(IUserService userService)
        {
            Mandate.That<ArgumentNullException>(userService != null);
            _userService = userService;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.UserPickerAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var intAttempt = source.TryConvertTo<int>();
            if (intAttempt.Success)
                return intAttempt.Result;

            return null;
        }

        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var userId = (int) source;
            var user = _userService.GetUserById(userId);
            if (user != null)
            {
                return user;
            }

            return userId;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return typeof(IUser);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            PropertyCacheLevel returnLevel;
            switch (cacheValue)
            {
                case PropertyCacheValue.Object:
                    returnLevel = PropertyCacheLevel.ContentCache;
                    break;
                case PropertyCacheValue.Source:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                case PropertyCacheValue.XPath:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                default:
                    returnLevel = PropertyCacheLevel.None;
                    break;
            }

            return returnLevel;
        }
    }
}
