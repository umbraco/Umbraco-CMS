using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Compilation;
using Examine;
using Examine.LuceneEngine.Config;
using Examine.Providers;
using Examine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Umbraco.Core;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;
using Umbraco.Core.Logging;
using UmbracoExamine.LocalStorage;
using Directory = Lucene.Net.Store.Directory;


namespace UmbracoExamine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the 
    /// </summary>
    public class UmbracoExamineSearcher : LuceneSearcher
    {

        private Lazy<Directory> _localTempDirectory;        
        private LocalStorageType _localStorageType = LocalStorageType.Sync;
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

        /// <summary>
        /// we override name because we need to manually set it if !CanInitialize() 
        /// since we cannot call base.Initialize in that case.
        /// </summary>
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Method used for initializing based on a configuration based searcher
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (name == null) throw new ArgumentNullException("name");

            //ensure name is set
            _name = name;

            //We need to check if we actually can initialize, if not then don't continue
            if (CanInitialize() == false)
            {
                return;
            }

            base.Initialize(name, config);

            if (config != null && config["useTempStorage"] != null)
            {
                throw new NotImplementedException("Fix how local temp storage works and is synced with Examine v2.0 - since a writer is always open we cannot snapshot it, we need to use the same logic in AzureDirectory");

                //Use the temp storage directory which will store the index in the local/codegen folder, this is useful
                // for websites that are running from a remove file server and file IO latency becomes an issue
                var attemptUseTempStorage = config["useTempStorage"].TryConvertTo<LocalStorageType>();
                if (attemptUseTempStorage)
                {
                    //this is the default
                    ILocalStorageDirectory localStorageDir = new CodeGenLocalStorageDirectory();
                    if (config["tempStorageDirectory"] != null)
                    {
                        //try to get the type
                        var dirType = BuildManager.GetType(config["tempStorageDirectory"], false);
                        if (dirType != null)
                        {
                            try
                            {
                                localStorageDir = (ILocalStorageDirectory)Activator.CreateInstance(dirType);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error<UmbracoExamineSearcher>(
                                    string.Format("Could not create a temp storage location of type {0}, reverting to use the " + typeof(CodeGenLocalStorageDirectory).FullName, dirType),
                                    ex);
                            }
                        }
                    }
                    var indexSet = IndexSets.Instance.Sets[IndexSetName];
                    var configuredPath = indexSet.IndexPath;
                    var tempPath = localStorageDir.GetLocalStorageDirectory(config, configuredPath);
                    if (tempPath == null) throw new InvalidOperationException("Could not resolve a temp location from the " + localStorageDir.GetType() + " specified");
                    var localTempPath = tempPath.FullName;
                    _localStorageType = attemptUseTempStorage.Result;

                    //initialize the lazy callback
                    _localTempDirectory = new Lazy<Directory>(() =>
                    {
                        switch (_localStorageType)
                        {
                            case LocalStorageType.Sync:
                                var fsDir = base.GetLuceneDirectory() as FSDirectory;
                                if (fsDir != null)
                                {
                                    return LocalTempStorageDirectoryTracker.Current.GetDirectory(
                                        new DirectoryInfo(localTempPath),
                                        fsDir);
                                }
                                return base.GetLuceneDirectory();
                            case LocalStorageType.LocalOnly:
                                return DirectoryTracker.Current.GetDirectory(new DirectoryInfo(localTempPath));
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Returns true if the Umbraco application is in a state that we can initialize the examine indexes
        /// </summary>
        /// <returns></returns>

        protected bool CanInitialize()
        {
            //We need to check if we actually can initialize, if not then don't continue
            if (_configBased
                && (ApplicationContext.Current == null
                || ApplicationContext.Current.IsConfigured == false
                || ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured == false))
            {
                return false;
            }
            return true;
        }    

        /// <summary>
        /// Returns a list of fields to search on, this will also exclude the IndexPathFieldName and node type alias
        /// </summary>
        /// <returns></returns>
        protected internal override string[] GetSearchFields()
        {
            var fields = base.GetSearchFields();
            return fields
                .Where(x => x != BaseUmbracoIndexer.IndexPathFieldName)
                .Where(x => x != LuceneIndexer.NodeTypeAliasFieldName)
                .ToArray();
        }
        
        protected override Directory GetLuceneDirectory()
        {
            //local temp storage is not enabled, just return the default
            return _localTempDirectory.IsValueCreated == false 
                ? base.GetLuceneDirectory() 
                : _localTempDirectory.Value;
        }
    }
}
