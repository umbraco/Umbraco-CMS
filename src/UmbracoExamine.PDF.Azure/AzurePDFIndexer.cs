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
using UmbracoExamine.Azure;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.PDF.Azure
{
    public class AzurePDFIndexer : PDFIndexer, IAzureCatalogue
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzurePDFIndexer()
        {
            AzureExtensions.EnsureAzureConfig();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AzurePDFIndexer()
        {
            SupportedExtensions = new[] { ".pdf" };
            UmbracoFileProperty = "umbracoFile";
            //By default, we will be using the UmbracoAzureDataService
            DataService = new UmbracoAzureDataService();
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public AzurePDFIndexer(DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexPath, dataService, analyzer, async)
        {
        }


        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            this.SetOptimizationThresholdOnInit(config);
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;
        }

        /// <summary>
        /// The blob storage catalogue name to store the index in
        /// </summary>
        public string Catalogue { get; private set; }

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
