using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public class LocalLinkProcessor
{
    private readonly HtmlLocalLinkParser _localLinkParser;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IEnumerable<ITypedLocalLinkProcessor> _localLinkProcessors;

    public LocalLinkProcessor(
        HtmlLocalLinkParser localLinkParser,
        IIdKeyMap idKeyMap,
        IEnumerable<ITypedLocalLinkProcessor> localLinkProcessors)
    {
        _localLinkParser = localLinkParser;
        _idKeyMap = idKeyMap;
        _localLinkProcessors = localLinkProcessors;
    }

    public IEnumerable<string> GetSupportedPropertyEditorAliases() =>
        _localLinkProcessors.SelectMany(p => p.PropertyEditorAliases);

    public bool ProcessToEditorValue(object? editorValue)
    {
        ITypedLocalLinkProcessor? processor =
            _localLinkProcessors.FirstOrDefault(p => p.PropertyEditorValueType == editorValue?.GetType());

        return processor is not null && processor.Process.Invoke(editorValue, ProcessToEditorValue, ProcessStringValue);
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
