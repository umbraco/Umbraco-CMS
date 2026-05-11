using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Processes faulty local links detected during the upgrade to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class LocalLinkProcessorForFaultyLinks
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly IEnumerable<ITypedLocalLinkProcessor> _localLinkProcessors;
    private const string LocalLinkLocation = "__LOCALLINKLOCATION__";
    private const string TypeAttributeLocation = "__TYPEATTRIBUTELOCATION__";

    internal static readonly Regex FaultyHrefPattern = new(
        @"<a (?<faultyHref>href=['""] ?(?<typeAttribute> type=*?['""][^'""]*?['""] )?(?<localLink>\/{localLink:[a-fA-F0-9-]+}['""])).*?>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalLinkProcessorForFaultyLinks"/> class.
    /// </summary>
    /// <param name="idKeyMap">Provides the mapping between IDs and keys for link processing.</param>
    /// <param name="localLinkProcessors">The collection of typed local link processors to use.</param>
    public LocalLinkProcessorForFaultyLinks(
        IIdKeyMap idKeyMap,
        IEnumerable<ITypedLocalLinkProcessor> localLinkProcessors)
    {
        _idKeyMap = idKeyMap;
        _localLinkProcessors = localLinkProcessors;
    }

    /// <summary>
    /// Returns the property editor aliases supported by the local link processors.
    /// </summary>
    /// <returns>An enumerable collection of supported property editor aliases.</returns>
    public IEnumerable<string> GetSupportedPropertyEditorAliases() =>
        _localLinkProcessors.SelectMany(p => p.PropertyEditorAliases);

    public bool ProcessToEditorValue(object? editorValue)
    {
        ITypedLocalLinkProcessor? processor =
            _localLinkProcessors.FirstOrDefault(p => p.PropertyEditorValueType == editorValue?.GetType());

        return processor is not null && processor.Process.Invoke(editorValue, ProcessToEditorValue, ProcessStringValue);
    }

    /// <summary>
    /// Scans the input string for local link tags with misplaced or swapped attributes and corrects them to the expected format.
    /// </summary>
    /// <param name="input">The input string potentially containing local link tags with incorrect attribute placement.</param>
    /// <returns>The input string with all faulty local link tags corrected to the proper attribute order.</returns>
    public string ProcessStringValue(string input)
    {
        MatchCollection faultyTags = FaultyHrefPattern.Matches(input);

        foreach (Match fullTag in faultyTags)
        {
            var newValue =
                fullTag.Value.Replace(fullTag.Groups["typeAttribute"].Value, LocalLinkLocation)
                    .Replace(fullTag.Groups["localLink"].Value, TypeAttributeLocation)
                    .Replace(LocalLinkLocation, fullTag.Groups["localLink"].Value)
                    .Replace(TypeAttributeLocation, fullTag.Groups["typeAttribute"].Value);
            input = input.Replace(fullTag.Value, newValue);
        }

        return input;
    }

    private (Guid Key, string EntityType)? CreateIntBasedKeyType(int id)
    {
        // very old data, best effort replacement
        Attempt<Guid> documentAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (documentAttempt.Success)
        {
            return (Key: documentAttempt.Result, EntityType: UmbracoObjectTypes.Document.ToString());
        }

        Attempt<Guid> mediaAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Media);
        if (mediaAttempt.Success)
        {
            return (Key: mediaAttempt.Result, EntityType: UmbracoObjectTypes.Media.ToString());
        }

        return null;
    }
}
