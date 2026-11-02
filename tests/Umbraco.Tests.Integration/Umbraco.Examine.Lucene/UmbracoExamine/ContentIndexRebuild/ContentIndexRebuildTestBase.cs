using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine.ContentIndexRebuild;

/// <summary>
///     Seeds multilingual content with nested block editor values and rebuilds a content index from it through the real
///     <see cref="ContentIndexPopulator" />, capturing the value sets the index would receive.
/// </summary>
internal abstract partial class ContentIndexRebuildTestBase : UmbracoIntegrationTest
{
    /// <summary>
    ///     A selection of cultures to seed, in order. The first is the default language of a clean install.
    /// </summary>
    protected static readonly string[] AllCultures =
    [
        "en-US", "da-DK", "de-DE", "fr-FR", "es-ES", "it-IT", "nl-NL", "pl-PL", "pt-PT", "sv-SE", "fi-FI", "cs-CZ",
        "sk-SK", "sl-SI", "hu-HU", "ro-RO", "bg-BG", "el-GR", "et-EE", "lv-LV", "lt-LT", "hr-HR", "ga-IE", "mt-MT",
    ];

    protected ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    protected IContentService ContentService => GetRequiredService<IContentService>();

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    protected IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    protected PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    protected IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    protected async Task<string[]> CreateLanguagesAsync(int count)
    {
        if (count < 1 || count > AllCultures.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        foreach (var culture in AllCultures.Skip(1).Take(count - 1))
        {
            await LanguageService.CreateAsync(new Language(culture, culture), Constants.Security.SuperUserKey);
        }

        return AllCultures.Take(count).ToArray();
    }

    /// <summary>
    ///     Rebuilds a capturing index through <see cref="ContentIndexPopulator" /> and returns what it received.
    /// </summary>
    /// <returns>The value sets the index received, in batches.</returns>
    protected CapturedIndex Rebuild(bool publishedValuesOnly, int batchSize, Action<CapturedBatch>? onBatch = null)
    {
        IValueSetBuilder<IContent> valueSetBuilder = publishedValuesOnly
            ? GetRequiredService<IPublishedContentValueSetBuilder>()
            : GetRequiredService<IContentValueSetBuilder>();

        var populator = new ContentIndexPopulator(
            GetRequiredService<ILogger<ContentIndexPopulator>>(),
            publishedValuesOnly,
            null,
            ContentService,
            GetRequiredService<IUmbracoDatabaseFactory>(),
            valueSetBuilder,
            new TestOptionsMonitor<IndexingSettings>(new IndexingSettings { BatchSize = batchSize }));

        var captured = new CapturedIndex();
        var index = new Mock<IUmbracoContentIndex>();
        index.SetupGet(x => x.Name).Returns("CapturingIndex");
        index.SetupGet(x => x.PublishedValuesOnly).Returns(publishedValuesOnly);
        index
            .Setup(x => x.IndexItems(It.IsAny<IEnumerable<ValueSet>>()))
            .Callback<IEnumerable<ValueSet>>(valueSets =>
            {
                // Materialise the whole batch at once, as a remote index provider buffering a batch for upload does.
                var batch = new CapturedBatch(valueSets.ToList());
                captured.Batches.Add(batch);
                onBatch?.Invoke(batch);
            });

        populator.Populate(index.Object);
        return captured;
    }

    /// <summary>
    ///     Creates a deterministic key so that seeded data, and anything derived from it, is identical between runs.
    /// </summary>
    /// <returns>A key derived from the seed.</returns>
    protected static Guid DeterministicKey(string seed)
        => new(MD5.HashData(Encoding.UTF8.GetBytes(seed)));

    /// <summary>
    ///     Renders captured value sets as text that is stable between runs, for comparison against an expected snapshot.
    /// </summary>
    /// <remarks>
    ///     Database identities, dates and paths depend on insertion order and timing rather than on indexing behaviour,
    ///     so they are left out. Value sets are ordered by node name, which the seeded data keeps unique.
    /// </remarks>
    /// <returns>The rendered value sets.</returns>
    protected static string RenderSnapshot(IEnumerable<ValueSet> valueSets)
    {
        var builder = new StringBuilder();
        foreach (ValueSet valueSet in valueSets.OrderBy(x => NodeName(x), StringComparer.Ordinal))
        {
            builder.Append("### ").Append(NodeName(valueSet)).Append(" [").Append(valueSet.Category).Append('/').Append(valueSet.ItemType).Append(']').Append('\n');
            foreach (KeyValuePair<string, IReadOnlyList<object>> field in valueSet.Values.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                if (IsVolatileField(field.Key))
                {
                    continue;
                }

                builder.Append(field.Key).Append(" = ");
                builder.AppendJoin(" | ", field.Value.Select(FormatValue));
                builder.Append('\n');
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Renders the property values of loaded content as text that is stable between runs.
    /// </summary>
    /// <returns>The rendered property values.</returns>
    protected static string RenderSnapshot(IEnumerable<IContent> content)
    {
        var builder = new StringBuilder();
        foreach (IContent item in content.OrderBy(x => x.Name, StringComparer.Ordinal))
        {
            builder.Append("### ").Append(item.Name).Append(" [").Append(item.ContentType.Alias).Append(']').Append('\n');
            builder.Append("published = ").Append(item.Published).Append(", edited = ").Append(item.Edited).Append('\n');
            builder.Append("cultures = ").AppendJoin(", ", item.AvailableCultures.Order(StringComparer.Ordinal)).Append('\n');
            builder.Append("published cultures = ").AppendJoin(", ", item.PublishedCultures.Order(StringComparer.Ordinal)).Append('\n');
            foreach (IProperty property in item.Properties.OrderBy(x => x.Alias, StringComparer.Ordinal))
            {
                foreach (IPropertyValue value in property.Values.OrderBy(x => x.Culture, StringComparer.Ordinal).ThenBy(x => x.Segment, StringComparer.Ordinal))
                {
                    var variant = $" [{value.Culture ?? "*"}/{value.Segment ?? "*"}]";
                    builder.Append(property.Alias).Append(variant).Append(" edited = ").Append(value.EditedValue is null ? "<null>" : FormatValue(value.EditedValue)).Append('\n');
                    builder.Append(property.Alias).Append(variant).Append(" published = ").Append(value.PublishedValue is null ? "<null>" : FormatValue(value.PublishedValue)).Append('\n');
                }
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private static string NodeName(ValueSet valueSet)
        => valueSet.Values.TryGetValue(UmbracoExamineFieldNames.NodeNameFieldName, out IReadOnlyList<object>? names) && names.Count > 0
            ? names[0].ToString() ?? string.Empty
            : valueSet.Id;

    private static readonly HashSet<string> _volatileFields =
    [
        "id", UmbracoExamineFieldNames.NodeKeyFieldName, "parentID", "path", "createDate", "updateDate", "nodeType",
        "creatorID", "writerID", "templateID",
    ];

    private static bool IsVolatileField(string fieldName)
        => _volatileFields.Contains(fieldName) || fieldName.StartsWith("updateDate_", StringComparison.Ordinal);

    protected static string FormatValue(object value)
    {
        var formatted = value switch
        {
            DateTime dateTime => dateTime.ToString("O", CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

        // Summaries join values with Environment.NewLine, so normalise it to keep snapshots platform independent.
        return GuidPattern().Replace(formatted.Replace("\r\n", "\n").Replace("\r", "\\r").Replace("\n", "\\n"), "<guid>");
    }

    [GeneratedRegex("[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}")]
    private static partial Regex GuidPattern();

    protected sealed class CapturedIndex
    {
        public List<CapturedBatch> Batches { get; } = [];

        public IEnumerable<ValueSet> ValueSets => Batches.SelectMany(x => x.ValueSets);
    }

    protected sealed class CapturedBatch(List<ValueSet> valueSets)
    {
        public List<ValueSet> ValueSets { get; } = valueSets;
    }
}
