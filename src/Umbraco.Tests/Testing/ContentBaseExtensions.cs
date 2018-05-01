using System;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Testing
{
    public static class ContentBaseExtensions
    {
        /// <summary>
        /// Set property values by alias with an annonymous object.
        /// </summary>
        /// <remarks>Does not support variants.</remarks>
        public static void PropertyValues(this IContentBase content, object value, string culture = null, string segment = null)
        {
            if (value == null)
                throw new Exception("No properties has been passed in");

            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                //Check if a PropertyType with alias exists thus being a valid property
                var propertyType = content.PropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (propertyType == null)
                    throw new Exception($"The property alias {propertyInfo.Name} is not valid, because no PropertyType with this alias exists");

                //Check if a Property with the alias already exists in the collection thus being updated or inserted
                var item = content.Properties.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (item != null)
                {
                    item.SetValue(propertyInfo.GetValue(value, null), culture, segment);
                    //Update item with newly added value
                    content.Properties.Add(item);
                }
                else
                {
                    //Create new Property to add to collection
                    var property = propertyType.CreateProperty();
                    property.SetValue(propertyInfo.GetValue(value, null), culture, segment);
                    content.Properties.Add(property);
                }
            }
        }
    }
}
