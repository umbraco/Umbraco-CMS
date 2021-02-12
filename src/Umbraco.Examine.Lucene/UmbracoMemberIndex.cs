// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Lucene.Net.Analysis;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Examine
{
    /// <summary>
    /// Custom indexer for members
    /// </summary>
    public class UmbracoMemberIndex : UmbracoExamineIndex, IUmbracoMemberIndex
    {
        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="runtimeState"></param>
        /// <param name="validator"></param>
        /// <param name="analyzer"></param>
        public UmbracoMemberIndex(
            string name,
            FieldDefinitionCollection fieldDefinitions,
            Directory luceneDirectory,
            Analyzer analyzer,
            IProfilingLogger profilingLogger,
            ILogger<UmbracoMemberIndex> logger,
            ILoggerFactory loggerFactory,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            IValueSetValidator validator = null) :
            base(name, luceneDirectory, fieldDefinitions, analyzer, profilingLogger, logger, loggerFactory, hostingEnvironment, runtimeState, validator)
        {
        }

    }
}
