using Examine.Lucene;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Search.Configuration;
using Umbraco.Search.Examine.Configuration;
using Umbraco.Search.Examine.Extensions;
using Umbraco.Search.Examine.ValueSet;

namespace Umbraco.Search.Examine.Lucene.DependencyInjection;

/// <summary>
///     Configures the index options to construct the Examine indexes
/// </summary>
public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    private readonly IUmbracoIndexesConfiguration _umbracoIndexesConfiguration;
    private readonly IndexCreatorSettings _settings;

    public ConfigureIndexOptions(
        IUmbracoIndexesConfiguration umbracoIndexesConfiguration,
        IOptions<IndexCreatorSettings> settings)
    {
        _umbracoIndexesConfiguration = umbracoIndexesConfiguration;
        _settings = settings.Value;
    }

    public void Configure(string? name, LuceneDirectoryIndexOptions options)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }
        var configuration = _umbracoIndexesConfiguration.Configuration(name) as IUmbracoExamineIndexConfig;
        options.Analyzer = configuration?.Analyzer ?? new StandardAnalyzer(LuceneVersion.LUCENE_48);
        options.Validator = new ExamineValueSetValidator( configuration?.PublishedValuesOnly ?? false ? configuration.GetPublishedContentValueSetValidator() : configuration?.GetContentValueSetValidator());
        options.FieldDefinitions = new UmbracoFieldDefinitionCollection().toExamineFieldDefinitionCollection();


        // ensure indexes are unlocked on startup
        options.UnlockIndex = true;

        if (_settings.LuceneDirectoryFactory == LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory)
        {
            // if this directory factory is enabled then a snapshot deletion policy is required
            options.IndexDeletionPolicy = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
        }
    }

    public void Configure(LuceneDirectoryIndexOptions options)
        => throw new NotImplementedException("This is never called and is just part of the interface");
}
