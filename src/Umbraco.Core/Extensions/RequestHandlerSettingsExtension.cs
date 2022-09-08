using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;

namespace Umbraco.Extensions;

/// <summary>
///     Get concatenated user and default character replacements
///     taking into account <see cref="RequestHandlerSettings.EnableDefaultCharReplacements" />
/// </summary>
public static class RequestHandlerSettingsExtension
{
    /// <summary>
    ///     Get concatenated user and default character replacements
    ///     taking into account <see cref="RequestHandlerSettings.EnableDefaultCharReplacements" />
    /// </summary>
    public static IEnumerable<CharItem> GetCharReplacements(this RequestHandlerSettings requestHandlerSettings)
    {
        if (requestHandlerSettings.EnableDefaultCharReplacements is false)
        {
            return requestHandlerSettings.UserDefinedCharCollection ?? Enumerable.Empty<CharItem>();
        }

        if (requestHandlerSettings.UserDefinedCharCollection == null ||
            requestHandlerSettings.UserDefinedCharCollection.Any() is false)
        {
            return RequestHandlerSettings.DefaultCharCollection;
        }

        return MergeUnique(
            requestHandlerSettings.UserDefinedCharCollection,
            RequestHandlerSettings.DefaultCharCollection);
    }

    /// <summary>
    ///     Merges CharCollection and UserDefinedCharCollection, prioritizing UserDefinedCharCollection
    /// </summary>
    internal static void MergeReplacements(
        this RequestHandlerSettings requestHandlerSettings,
        IConfiguration configuration)
    {
        var sectionKey = $"{Constants.Configuration.ConfigRequestHandler}:";

        IEnumerable<CharItem> charCollection = GetReplacements(
            configuration,
            $"{sectionKey}{nameof(RequestHandlerSettings.CharCollection)}");

        IEnumerable<CharItem> userDefinedCharCollection = GetReplacements(
            configuration,
            $"{sectionKey}{nameof(requestHandlerSettings.UserDefinedCharCollection)}");

        IEnumerable<CharItem> mergedCollection = MergeUnique(userDefinedCharCollection, charCollection);

        requestHandlerSettings.UserDefinedCharCollection = mergedCollection;
    }

    private static IEnumerable<CharItem> GetReplacements(IConfiguration configuration, string key)
    {
        var replacements = new List<CharItem>();
        IEnumerable<IConfigurationSection> config = configuration.GetSection(key).GetChildren();

        foreach (IConfigurationSection section in config)
        {
            var @char = section.GetValue<string>(nameof(CharItem.Char));
            var replacement = section.GetValue<string>(nameof(CharItem.Replacement));
            replacements.Add(new CharItem { Char = @char, Replacement = replacement });
        }

        return replacements;
    }

    /// <summary>
    ///     Merges two IEnumerable of CharItem without any duplicates, items in priorityReplacements will override those in
    ///     alternativeReplacements
    /// </summary>
    private static IEnumerable<CharItem> MergeUnique(
        IEnumerable<CharItem> priorityReplacements,
        IEnumerable<CharItem> alternativeReplacements)
    {
        var priorityReplacementsList = priorityReplacements.ToList();
        var alternativeReplacementsList = alternativeReplacements.ToList();

        foreach (CharItem alternativeReplacement in alternativeReplacementsList)
        {
            foreach (CharItem priorityReplacement in priorityReplacementsList)
            {
                if (priorityReplacement.Char == alternativeReplacement.Char)
                {
                    alternativeReplacement.Replacement = priorityReplacement.Replacement;
                }
            }
        }

        return priorityReplacementsList.Union<CharItem>(
            alternativeReplacementsList,
            new CharacterReplacementEqualityComparer());
    }
}
