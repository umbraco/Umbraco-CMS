using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

public class LocalLinkProcessor
{
    private readonly HtmlLocalLinkParser _localLinkParser;
    private readonly IIdKeyMap _idKeyMap;
    private readonly Lazy<IOptions<ConvertLocalLinkOptions>> _lazyOptions;

    public LocalLinkProcessor(
        HtmlLocalLinkParser localLinkParser,
        IIdKeyMap idKeyMap,
        Lazy<IOptions<ConvertLocalLinkOptions>> lazyOptions)
    {
        _localLinkParser = localLinkParser;
        _idKeyMap = idKeyMap;
        _lazyOptions = lazyOptions;
    }

    public IEnumerable<string> GetSupportedPropertyEditorAliases() =>
        _lazyOptions.Value.Value.Processors.SelectMany(p => p.PropertyEditorAliases);

    public bool ProcessToEditorValue(object? editorValue)
    {
        ProcessorInformation? processor = _lazyOptions.Value.Value.Processors.FirstOrDefault(p => p.PropertyEditorValueType == editorValue?.GetType());

        return processor is not null && processor.Process.Invoke(editorValue);
    }

    public string ProcessStringValue(string input)
    {
        // find all legacy tags
        var tags = _localLinkParser.FindLegacyLocalLinkIds(input).ToList();

        foreach (HtmlLocalLinkParser.LocalLinkTag tag in tags)
        {
            string newTagHref;
            if (tag.Udi is not null)
            {
                newTagHref = $" type=\"{tag.Udi.EntityType}\" "
                             + tag.TagHref.Replace(tag.Udi.ToString(), tag.Udi.Guid.ToString());
            }
            else if (tag.IntId is not null)
            {
                // try to get the key and type from the int, else do nothing
                (Guid Key, string EntityType)? conversionResult = CreateIntBasedKeyType(tag.IntId.Value);
                if (conversionResult is null)
                {
                    continue;
                }

                newTagHref = $" type=\"{conversionResult.Value.EntityType}\" "
                             + tag.TagHref.Replace(tag.IntId.Value.ToString(), conversionResult.Value.Key.ToString());
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
