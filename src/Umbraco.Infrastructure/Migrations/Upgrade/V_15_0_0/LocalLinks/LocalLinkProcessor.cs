using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Handles the processing and migration of local links as part of the upgrade process to Umbraco version 15.0.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class LocalLinkProcessor
{
    private readonly HtmlLocalLinkParser _localLinkParser;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IEnumerable<ITypedLocalLinkProcessor> _localLinkProcessors;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalLinkProcessor"/> class, which processes and parses local links during migration upgrades.
    /// </summary>
    /// <param name="localLinkParser">An instance of <see cref="HtmlLocalLinkParser"/> used to parse local links within HTML content.</param>
    /// <param name="idKeyMap">An implementation of <see cref="IIdKeyMap"/> that provides mapping between IDs and keys for content references.</param>
    /// <param name="localLinkProcessors">A collection of <see cref="ITypedLocalLinkProcessor"/> used to handle specific types of local link processing.</param>
    public LocalLinkProcessor(
        HtmlLocalLinkParser localLinkParser,
        IIdKeyMap idKeyMap,
        IEnumerable<ITypedLocalLinkProcessor> localLinkProcessors)
    {
        _localLinkParser = localLinkParser;
        _idKeyMap = idKeyMap;
        _localLinkProcessors = localLinkProcessors;
    }

    /// <summary>
    /// Gets all supported property editor aliases by aggregating the aliases from the registered local link processors.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of strings representing the supported property editor aliases.</returns>
    public IEnumerable<string> GetSupportedPropertyEditorAliases() =>
        _localLinkProcessors.SelectMany(p => p.PropertyEditorAliases);

    /// <summary>
    /// Attempts to process the specified editor value using a matching local link processor based on the value's type.
    /// </summary>
    /// <param name="editorValue">The editor value to process; its runtime type is used to select the appropriate processor.</param>
    /// <returns><c>true</c> if a suitable processor was found and processing succeeded; otherwise, <c>false</c>.</returns>
    public bool ProcessToEditorValue(object? editorValue)
    {
        ITypedLocalLinkProcessor? processor =
            _localLinkProcessors.FirstOrDefault(p => p.PropertyEditorValueType == editorValue?.GetType());

        return processor is not null && processor.Process.Invoke(editorValue, ProcessToEditorValue, ProcessStringValue);
    }

    /// <summary>
    /// Processes the input string by searching for legacy local link tags and replacing them with updated link tags that include GUIDs and entity types.
    /// If a legacy tag cannot be converted (due to missing information), it is left unchanged in the output.
    /// </summary>
    /// <param name="input">The input string potentially containing legacy local link tags to process.</param>
    /// <returns>
    /// The input string with all convertible legacy local link tags replaced by updated tags containing GUIDs and entity types; unconvertible tags remain unchanged.
    /// </returns>
    public string ProcessStringValue(string input)
    {
        // find all legacy tags
        var tags = _localLinkParser.FindLegacyLocalLinkIds(input).ToList();

        foreach (HtmlLocalLinkParser.LocalLinkTag tag in tags)
        {
            string newTagHref;
            if (tag.Udi is not null)
            {
                newTagHref = tag.TagHref.Replace(tag.Udi.ToString(), tag.Udi.Guid.ToString())
                             + $"\" type=\"{tag.Udi.EntityType}";
            }
            else if (tag.IntId is not null)
            {
                // try to get the key and type from the int, else do nothing
                (Guid Key, string EntityType)? conversionResult = CreateIntBasedKeyType(tag.IntId.Value);
                if (conversionResult is null)
                {
                    continue;
                }

                newTagHref = tag.TagHref.Replace(tag.IntId.Value.ToString(), conversionResult.Value.Key.ToString())
                             + $"\" type=\"{conversionResult.Value.EntityType}";
            }
            else
            {
                // tag does not contain enough information to convert
                continue;
            }

            input = input.Replace(tag.TagHref, newTagHref);
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
