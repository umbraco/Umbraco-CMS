using Examine;
using Examine.Lucene;
using Examine.Lucene.Analyzers;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;
using Umbraco.Search;
using Umbraco.Search.Examine;

namespace Umbraco.Cms.Infrastructure.Examine.DependencyInjection;

/// <summary>
///     Configures the index options to construct the Examine indexes
/// </summary>
public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
{
    private readonly IExamineIndexConfiguration _examineIndexConfiguration;
    private readonly IndexCreatorSettings _settings;
    private readonly IUmbracoExamineIndexConfig _umbracoIndexConfig;

    public ConfigureIndexOptions(
        IExamineIndexConfiguration examineIndexConfiguration,
        IOptions<IndexCreatorSettings> settings, IUmbracoExamineIndexConfig umbracoIndexConfig)
    {
        _examineIndexConfiguration = examineIndexConfiguration;
        _umbracoIndexConfig = umbracoIndexConfig;
        _settings = settings.Value;
    }

    public void Configure(string? name, LuceneDirectoryIndexOptions options)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }
        var configuration = _examineIndexConfiguration.Configuration(name);
        options.Analyzer = configuration.Analyzer;
        options.Validator = configuration.GetContentValueSetValidator();
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
