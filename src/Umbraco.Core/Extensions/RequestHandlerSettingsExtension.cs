using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;

namespace Umbraco.Extensions;

/// <summary>
///     Get concatenated user and default character replacements
///     taking into account <see cref="RequestHandlerSettings.EnableDefaultCharReplacements" />.
/// </summary>
public static class RequestHandlerSettingsExtension
{
    /// <summary>
    ///     Get concatenated user and default character replacements
    ///     taking into account <see cref="RequestHandlerSettings.EnableDefaultCharReplacements" />.
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

        // Merges CharCollection and UserDefinedCharCollection, prioritizing UserDefinedCharCollection.
        return MergeUnique(requestHandlerSettings.UserDefinedCharCollection, RequestHandlerSettings.DefaultCharCollection);
    }

    private static IEnumerable<CharItem> GetReplacements(IConfiguration configuration, string key)
    {
        var replacements = new List<CharItem>();
        IEnumerable<IConfigurationSection> config = configuration.GetSection(key).GetChildren();

        foreach (IConfigurationSection section in config)
        {
            var @char = section.GetValue<string>(nameof(CharItem.Char));
            var replacement = section.GetValue<string>(nameof(CharItem.Replacement));

            if (@char is null || replacement is null)
            {
                continue;
            }

            replacements.Add(new CharItem { Char = @char, Replacement = replacement });
        }

        return replacements;
    }

    /// <summary>
    ///     Merges two IEnumerable of CharItem without any duplicates, items in priorityReplacements will override those in alternativeReplacements.
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
