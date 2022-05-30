// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Common.Extensions;

public static class ContentBaseExtensions
{
    /// <summary>
    ///     Set property values by alias with an anonymous object.
    /// </summary>
    /// <remarks>Does not support variants.</remarks>
    public static void PropertyValues(this IContentBase content, object value, string? culture = null, string? segment = null)
    {
        if (value == null)
        {
            throw new Exception("No properties has been passed in");
        }

        var propertyInfos = value.GetType().GetProperties();
        foreach (var propertyInfo in propertyInfos)
        {
            if (!content.Properties.TryGetValue(propertyInfo.Name, out var property))
            {
                throw new Exception(
                    $"The property alias {propertyInfo.Name} is not valid, because no PropertyType with this alias exists");
            }

            property.SetValue(propertyInfo.GetValue(value, null), culture, segment);

            // Update item with newly added value
            content.Properties.Add(property);
        }
    }
}
