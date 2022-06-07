// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for ILocalizedTextService
/// </summary>
public static class LocalizedTextServiceExtensions
{
    public static string Localize<T>(this ILocalizedTextService manager, string area, T key)
        where T : Enum =>
        manager.Localize(area, key.ToString(), Thread.CurrentThread.CurrentUICulture);

    public static string Localize(this ILocalizedTextService manager, string? area, string? alias)
        => manager.Localize(area, alias, Thread.CurrentThread.CurrentUICulture);

    /// <summary>
    ///     Localize using the current thread culture
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="area"></param>
    /// <param name="alias"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public static string Localize(this ILocalizedTextService manager, string? area, string alias, string?[]? tokens)
        => manager.Localize(area, alias, Thread.CurrentThread.CurrentUICulture, ConvertToDictionaryVars(tokens));

    /// <summary>
    ///     Localize a key without any variables
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="area"></param>
    /// <param name="alias"></param>
    /// <param name="culture"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    public static string Localize(this ILocalizedTextService manager, string area, string alias, CultureInfo culture, string?[] tokens)
        => manager.Localize(area, alias, culture, ConvertToDictionaryVars(tokens));

    public static string? UmbracoDictionaryTranslate(
        this ILocalizedTextService manager,
        ICultureDictionary cultureDictionary,
        string? text)
    {
        if (text == null)
        {
            return null;
        }

        if (text.StartsWith("#") == false)
        {
            return text;
        }

        text = text.Substring(1);
        var value = cultureDictionary[text];
        if (value.IsNullOrWhiteSpace() == false)
        {
            return value;
        }

        if (text.IndexOf('_') == -1)
        {
            return text;
        }

        var areaAndKey = text.Split('_');

        if (areaAndKey.Length < 2)
        {
            return text;
        }

        value = manager.Localize(areaAndKey[0], areaAndKey[1]);
        return value.StartsWith("[") ? text : value;
    }

    /// <summary>
    ///     Convert an array of strings to a dictionary of indices -> values
    /// </summary>
    /// <param name="variables"></param>
    /// <returns></returns>
    internal static IDictionary<string, string?>? ConvertToDictionaryVars(string?[]? variables)
    {
        if (variables == null)
        {
            return null;
        }

        if (variables.Any() == false)
        {
            return null;
        }

        return variables.Select((s, i) => new { index = i.ToString(CultureInfo.InvariantCulture), value = s })
            .ToDictionary(keyvals => keyvals.index, keyvals => keyvals.value);
    }
}
