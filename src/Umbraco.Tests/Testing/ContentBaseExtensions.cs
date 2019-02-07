using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Testing
{
    public static class ContentBaseExtensions
    {
        public static void PropertyValues(this IContentBase content, object value, string culture = null, string segment = null)
        {
            content.PropertyValues(Current.Services.ContentTypeBaseServices, value, culture, segment);
        }

        /// <summary>
        /// Set property values by alias with an anonymous object.
        /// </summary>
        /// <remarks>Does not support variants.</remarks>
        public static void PropertyValues(this IContentBase content, IContentTypeBaseServiceProvider contentTypeServiceProvider, object value, string culture = null, string segment = null)
        {
            if (value == null)
                throw new Exception("No properties has been passed in");

            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                if (content.Properties.TryGetValue(propertyInfo.Name, out var property))
                {
                    property.SetValue(propertyInfo.GetValue(value, null), culture, segment);
                    //Update item with newly added value
                    content.Properties.Add(property);
                }
                else
                {

                    //TODO: Will this ever happen?? In theory we don't need to lookup the content type here since we can just check if the content contains properties with the correct name,
                    // however, i think this may be needed in the case where the content type contains property types that do not exist yet as properties on the
                    // content item? But can that happen? AFAIK it can't/shouldn't because of how we create content items in ContentBase we do _properties.EnsurePropertyTypes!

                    var contentType = contentTypeServiceProvider.GetContentTypeOf(content);
                    var propertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                    if (propertyType == null)
                        throw new Exception($"The property alias {propertyInfo.Name} is not valid, because no PropertyType with this alias exists");
                    //Create new Property to add to collection
                    property = propertyType.CreateProperty();
                    property.SetValue(propertyInfo.GetValue(value, null), culture, segment);
                    content.Properties.Add(property);
                }
            }
        }
    }
}
