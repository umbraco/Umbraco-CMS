using System;
using System.Collections.Generic;
using System.IO;
using Examine;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Core;

namespace Umbraco.Examine
{
    /// <inheritdoc />
    /// <summary>
    /// Abstract class for creating Lucene based Indexes
    /// </summary>
    public abstract class LuceneIndexCreator : IIndexCreator
    {
        private readonly ITypeFinder _typeFinder;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IndexCreatorSettings _settings;

        protected LuceneIndexCreator(ITypeFinder typeFinder, IHostingEnvironment hostingEnvironment, IOptions<IndexCreatorSettings> settings)
        {
            _typeFinder = typeFinder;
            _hostingEnvironment = hostingEnvironment;
            _settings = settings.Value;
        }

        public abstract IEnumerable<IIndex> Create();
    }
}
