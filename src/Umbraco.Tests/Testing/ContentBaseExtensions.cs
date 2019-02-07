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
        
        /// <summary>
        /// Set property values by alias with an anonymous object.
        /// </summary>
        /// <remarks>Does not support variants.</remarks>
        public static void PropertyValues(this IContentBase content, object value, string culture = null, string segment = null)
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

                    //fixme: Can this ever happen? According to the ctor in ContentBase (EnsurePropertyTypes), all properties will be created based on the content type's property types
                    // so how can a property not be resolved by the alias on the content.Properties but it can on the content type?
                    // This maybe can happen if a developer has removed a property with the api and is trying to then set the value of that property again...
                    // BUT, as it turns out the content.Properties.Remove(...) method is NEVER used, because why and how could it? you never remove a property from
                    // a content item directly.
                    
                    var propertyType = ((ContentBase)content).AllPropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
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
