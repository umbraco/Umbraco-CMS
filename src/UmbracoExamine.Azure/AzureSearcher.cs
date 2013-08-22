using System;
using System.IO;
using Examine.Azure;
using Examine.LuceneEngine.Config;
using Lucene.Net.Analysis;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace UmbracoExamine.Azure
{


    public class AzureSearcher : UmbracoExamineSearcher, IAzureCatalogue
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzureSearcher()
        {
            AzureExtensions.EnsureAzureConfig();
        }

        /// <summary>
		/// Default constructor
		/// </summary>
        public AzureSearcher()
		{
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>
        public AzureSearcher(DirectoryInfo indexPath, Analyzer analyzer)
            : base(indexPath, analyzer)
        {            
        }


        public string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);           
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;
        }

        protected override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            return this.GetAzureDirectory();
        }

    }
}