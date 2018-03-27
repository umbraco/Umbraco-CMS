using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Umbraco.Core.Composing;
using Umbraco.Examine.Config;
using Directory = Lucene.Net.Store.Directory;


namespace Umbraco.Examine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the
    /// </summary>
    public class UmbracoExamineSearcher : LuceneSearcher
    {

        private string _name;
        private readonly bool _configBased = false;

        /// <summary>
        /// Default constructor for config based construction
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public UmbracoExamineSearcher()
            : base()
        {
            _configBased = true;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>
        public UmbracoExamineSearcher(DirectoryInfo indexPath, Analyzer analyzer)
            : base(indexPath, analyzer)
        {
            _configBased = false;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="luceneDirectory"></param>
        /// <param name="analyzer"></param>
        public UmbracoExamineSearcher(Directory luceneDirectory, Analyzer analyzer)
            : base(luceneDirectory, analyzer)
        {
            _configBased = false;
        }

        //TODO: What about the NRT ctor?

        /// <summary>
        /// we override name because we need to manually set it if !CanInitialize()
        /// since we cannot call base.Initialize in that case.
        /// </summary>
        public override string Name => _name;

        /// <summary>
        /// Name of the Lucene.NET index set
        /// </summary>
        public string IndexSetName { get; private set; }

        /// <summary>
        /// Method used for initializing based on a configuration based searcher
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            //ensure name is set
            _name = name ?? throw new ArgumentNullException(nameof(name));

            //We need to check if we actually can initialize, if not then don't continue
            if (CanInitialize() == false)
            {
                return;
            }

            //need to check if the index set is specified, if it's not, we'll see if we can find one by convension
            //if the folder is not null and the index set is null, we'll assume that this has been created at runtime.
            //NOTE: Don't proceed if the _luceneDirectory is set since we already know where to look.
            var luceneDirectory = GetLuceneDirectory();
            if (config["indexSet"] == null && (LuceneIndexFolder == null && luceneDirectory == null))
            {
                //if we don't have either, then we'll try to set the index set by naming convensions
                var found = false;
                if (name.EndsWith("Searcher"))
                {
                    var setNameByConvension = name.Remove(name.LastIndexOf("Searcher")) + "IndexSet";
                    //check if we can assign the index set by naming convension
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>().SingleOrDefault(x => x.SetName == setNameByConvension);

                    if (set != null)
                    {
                        set.ReplaceTokensInIndexPath();

                        //we've found an index set by naming convensions :)
                        IndexSetName = set.SetName;
                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration");

                //get the folder to index
                LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[IndexSetName].IndexDirectory.FullName, "Index"));
            }
            else if (config["indexSet"] != null && luceneDirectory == null)
            {
                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");

                IndexSetName = config["indexSet"];

                var indexSet = IndexSets.Instance.Sets[IndexSetName];

                indexSet.ReplaceTokensInIndexPath();

                //get the folder to index
                LuceneIndexFolder = new DirectoryInfo(Path.Combine(indexSet.IndexDirectory.FullName, "Index"));
            }

            base.Initialize(name, config);

        }

        /// <summary>
        /// Returns true if the Umbraco application is in a state that we can initialize the examine indexes
        /// </summary>

        protected bool CanInitialize()
        {
            // only affects indexers that are config file based, if an index was created via code then
            // this has no effect, it is assumed the index would not be created if it could not be initialized
            return _configBased == false || Current.RuntimeState.Level == RuntimeLevel.Run;
        }

        /// <summary>
        /// Returns a list of fields to search on, this will also exclude the IndexPathFieldName and node type alias
        /// </summary>
        /// <returns></returns>
        protected override string[] GetSearchFields()
        {
            var fields = base.GetSearchFields();
            return fields
                .Where(x => x != UmbracoExamineIndexer.IndexPathFieldName)
                .Where(x => x != LuceneIndexer.ItemTypeFieldName)
                .ToArray();
        }

    }
}
