using System;
using System.Collections.Generic;
using System.IO;
using Examine;
using Examine.LuceneEngine.Directories;
using Lucene.Net.Store;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;

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
        private readonly IIndexCreatorSettings _settings;

        protected LuceneIndexCreator(ITypeFinder typeFinder, IHostingEnvironment hostingEnvironment, IIndexCreatorSettings settings)
        {
            _typeFinder = typeFinder;
            _hostingEnvironment = hostingEnvironment;
            _settings = settings;
        }

        public abstract IEnumerable<IIndex> Create();        
    }
}
