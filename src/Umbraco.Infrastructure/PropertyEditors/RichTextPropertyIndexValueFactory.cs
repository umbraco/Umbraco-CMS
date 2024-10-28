using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class RichTextPropertyIndexValueFactory : BlockValuePropertyIndexValueFactoryBase<RichTextEditorValue>, IRichTextPropertyIndexValueFactory
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<RichTextPropertyIndexValueFactory> _logger;

    public RichTextPropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings,
        ILogger<RichTextPropertyIndexValueFactory> logger)
        : base(propertyEditorCollection, jsonSerializer, indexingSettings)
    {
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    public override IEnumerable<IndexValue> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var val = property.GetValue(culture, segment, published);
        if (RichTextPropertyEditorHelper.TryParseRichTextEditorValue(val, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue) is false)
        {
            return [];
        }

        // always index the "raw" value
        var indexValues = new List<IndexValue>
        {
            new IndexValue
            {
                Culture = culture,
                FieldName = $"{UmbracoExamineFieldNames.RawFieldPrefix}{property.Alias}",
                Values = [richTextEditorValue.Markup]
            }
        };

        // the actual content (RTE content without markup, i.e. the actual words) must be indexed under the property alias
        var richTextWithoutMarkup = richTextEditorValue.Markup.StripHtml();
        if (richTextEditorValue.Blocks?.ContentData.Any() is not true)
        {
            // no blocks; index the content for the culture and be done with it
            indexValues.Add(new IndexValue
            {
                Culture = culture,
                FieldName = property.Alias,
                Values = [richTextWithoutMarkup]
            });
            return indexValues;
        }

        // the "blocks values resume" (the combined searchable text values from all blocks) is stored as a string value under the property alias by the base implementation
        var blocksIndexValuesResumes = base
            .GetIndexValues(property, culture, segment, published, availableCultures, contentTypeDictionary)
            .Where(value => value.FieldName == property.Alias)
            .GroupBy(value => value.Culture?.ToLowerInvariant())
            .Select(group => new
            {
                Culture = group.Key,
                Resume = string.Join(Environment.NewLine, group.Select(v => v.Values.FirstOrDefault() as string))
            })
            .ToArray();

        // is this RTE sat on culture variant content?
        if (culture is not null)
        {
            // yes, append the "block values resume" for the specific culture only (if present)
            var blocksResume = blocksIndexValuesResumes
                .FirstOrDefault(r => r.Culture.InvariantEquals(culture))?
                .Resume;

            indexValues.Add(new IndexValue
            {
                Culture = culture,
                FieldName = property.Alias,
                Values = [$"{richTextWithoutMarkup}{Environment.NewLine}{blocksResume}"]
            });
            return indexValues;
        }

        // is there an invariant "block values resume"? this might happen for purely invariant blocks or in a culture invariant context
        var invariantResume = blocksIndexValuesResumes
            .FirstOrDefault(r => r.Culture is null)
            ?.Resume;
        if (invariantResume != null)
        {
            // yes, append the invariant "block values resume"
            indexValues.Add(new IndexValue
            {
                Culture = culture,
                FieldName = property.Alias,
                Values = [$"{richTextWithoutMarkup}{Environment.NewLine}{invariantResume}"]
            });
            return indexValues;
        }

        // at this point we have encountered block level variance - add explicit index values for all "block values resume" cultures found
        indexValues.AddRange(blocksIndexValuesResumes.Select(resume =>
            new IndexValue
            {
                Culture = resume.Culture,
                FieldName = property.Alias,
                Values = [$"{richTextWithoutMarkup}{Environment.NewLine}{resume.Resume}"]
            }));

        // if one or more cultures did not have any (exposed) blocks, ensure that the RTE content is still indexed for those cultures
        IEnumerable<string?> missingBlocksResumeCultures = availableCultures.Except(blocksIndexValuesResumes.Select(r => r.Culture), StringComparer.CurrentCultureIgnoreCase);
        indexValues.AddRange(missingBlocksResumeCultures.Select(missingResumeCulture =>
            new IndexValue
            {
                Culture = missingResumeCulture,
                FieldName = property.Alias,
                Values = [richTextWithoutMarkup]
            }));

        return indexValues;
    }

    protected override IEnumerable<RawDataItem> GetDataItems(RichTextEditorValue input, bool published)
        => GetDataItems(input.Blocks?.ContentData ?? [], input.Blocks?.Expose ?? [], published);
}
