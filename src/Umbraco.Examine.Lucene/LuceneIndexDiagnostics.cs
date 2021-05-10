// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Examine.Lucene.Providers;
using Lucene.Net.Store;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class LuceneIndexDiagnostics : IIndexDiagnostics
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public LuceneIndexDiagnostics(LuceneIndex index, ILogger<LuceneIndexDiagnostics> logger, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            Index = index;
            Logger = logger;
        }

        public LuceneIndex Index { get; }
        public ILogger<LuceneIndexDiagnostics> Logger { get; }

       

        public Attempt<string> IsHealthy()
        {
            var isHealthy = Index.IsHealthy(out var indexError);
            return isHealthy ? Attempt<string>.Succeed() : Attempt.Fail(indexError.Message);
        }

        public long GetDocumentCount() => Index.GetDocumentCount();

        public IEnumerable<string> GetFieldNames() => Index.GetFieldNames();

        public virtual IReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                var luceneDir = Index.GetLuceneDirectory();
                var d = new Dictionary<string, object>
                {
                    [nameof(UmbracoExamineIndex.CommitCount)] = Index.CommitCount,
                    [nameof(UmbracoExamineIndex.DefaultAnalyzer)] = Index.DefaultAnalyzer.GetType().Name,
                    ["LuceneDirectory"] = luceneDir.GetType().Name
                };

                if (luceneDir is FSDirectory fsDir)
                {

                    var rootDir = _hostingEnvironment.ApplicationPhysicalPath;
                    d[nameof(UmbracoExamineIndex.LuceneIndexFolder)] = fsDir.Directory.ToString().ToLowerInvariant().TrimStart(rootDir.ToLowerInvariant()).Replace("\\", "/").EnsureStartsWith('/');
                }

                return d;
            }
        }


    }
}
