// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Extensions;

public static class HaveAdditionalDataExtensions
{
    /// <summary>
    ///     Gets additional data.
    /// </summary>
    public static object? GetAdditionalDataValueIgnoreCase(this IHaveAdditionalData entity, string key, object? defaultValue)
    {
        if (!entity.HasAdditionalData)
        {
            return defaultValue;
        }

        if (entity.AdditionalData?.ContainsKeyIgnoreCase(key) == false)
        {
            return defaultValue;
        }

        return entity.AdditionalData?.GetValueIgnoreCase(key, defaultValue);
    }
}
