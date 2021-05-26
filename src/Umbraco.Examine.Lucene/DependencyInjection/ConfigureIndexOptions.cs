using System;
using Examine;
using Examine.Lucene;
using Examine.Lucene.Analyzers;
using Lucene.Net.Analysis.Standard;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Examine.DependencyInjection
{
    /// <summary>
    /// Configures the index options to construct the Examine indexes
    /// </summary>
    public sealed class ConfigureIndexOptions : IConfigureNamedOptions<LuceneDirectoryIndexOptions>
    {
        private readonly IUmbracoIndexConfig _umbracoIndexConfig;

        public ConfigureIndexOptions(IUmbracoIndexConfig umbracoIndexConfig)
            => _umbracoIndexConfig = umbracoIndexConfig;

        public void Configure(string name, LuceneDirectoryIndexOptions options)
        {
            switch (name)
            {
                case Constants.UmbracoIndexes.InternalIndexName:
                    options.Analyzer = new CultureInvariantWhitespaceAnalyzer();
                    options.Validator = _umbracoIndexConfig.GetContentValueSetValidator();
                    options.FieldDefinitions = new UmbracoFieldDefinitionCollection();
                    break;
                case Constants.UmbracoIndexes.ExternalIndexName:
                    options.Analyzer = new StandardAnalyzer(LuceneInfo.CurrentVersion);
                    options.Validator = _umbracoIndexConfig.GetPublishedContentValueSetValidator();
                    options.FieldDefinitions = new UmbracoFieldDefinitionCollection();
                    break;
                case Constants.UmbracoIndexes.MembersIndexName:
                    options.Analyzer = new CultureInvariantWhitespaceAnalyzer();
                    options.Validator = _umbracoIndexConfig.GetMemberValueSetValidator();
                    options.FieldDefinitions = new UmbracoFieldDefinitionCollection();
                    break;
            }
        }

        public void Configure(LuceneDirectoryIndexOptions options)
            => throw new NotImplementedException("This is never called and is just part of the interface");
    }
}
