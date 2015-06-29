using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Examine;
using Examine.Azure;
using Examine.LuceneEngine.Config;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.Azure
{
    public class AzureContentIndexer : UmbracoContentIndexer, IAzureCatalogue
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzureContentIndexer()
        {
            AzureExtensions.EnsureAzureConfig();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AzureContentIndexer()
            : base()
        {
            //By default, we will be using the UmbracoAzureDataService
            DataService = new UmbracoAzureDataService();
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public AzureContentIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            
        }

        public string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            this.SetOptimizationThresholdOnInit(config);
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;

        }

        private Lucene.Net.Store.Directory _directory;
        public override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            return _directory ?? (_directory = this.GetAzureDirectory());
        }

        public override Lucene.Net.Index.IndexWriter GetIndexWriter()
        {
            return this.GetAzureIndexWriter();
        }

        protected override void OnIndexingError(IndexingErrorEventArgs e)
        {
            AzureExtensions.LogExceptionFile(Name, e);
            base.OnIndexingError(e);
        }

    }
}
