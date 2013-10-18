using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Tests.CodeFirst
{
    public static class ContentTypeMapper
    {
        public static T Map<T>(IPublishedContent content) where T : ContentTypeBase, new()
        {
            //TODO Verify passed in type T against DocumentType name/alias
            //TODO Cache mapped properties?

            T mapped = new T();
            mapped.Content = content;

            var propertyInfos = typeof(T).GetProperties();

            foreach (var property in content.Properties)
            {
                var @alias = property.PropertyTypeAlias.ToLowerInvariant();

                var propertyInfo = propertyInfos.FirstOrDefault(x => x.Name.ToLowerInvariant() == @alias);
                if (propertyInfo == null) continue;

                object value = null;
                //TODO Proper mapping of types
                if (propertyInfo.PropertyType == typeof(string))
                    value = property.Value;
                else if (propertyInfo.PropertyType == typeof(DateTime))
                    value = DateTime.Parse(property.Value.ToString());
                else if (propertyInfo.PropertyType == typeof(Boolean))
                {
                    if (String.IsNullOrEmpty(property.Value.ToString()) || property.Value == "0")
                    {
                        value = false;
                    }
                    else
                    {
                        value = true;
                    }
                }

                propertyInfo.SetValue(mapped, value, null);
            }

            return mapped;
        }
    }
}