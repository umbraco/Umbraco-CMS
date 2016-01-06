using System;
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
using UmbracoExamine.Config;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Analysis;
using Umbraco.Core.Logging;
using UmbracoExamine.LocalStorage;


namespace UmbracoExamine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the 
    /// </summary>
    public class UmbracoExamineSearcher : LuceneSearcher
    {

        private volatile Lucene.Net.Store.Directory _localTempDirectory;
        private static readonly object Locker = new object();
        private string _localTempPath = null;
        private LocalStorageType _localStorageType = LocalStorageType.Sync;

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoExamineSearcher()
            : base()
        {
        }

        private string _name;

        /// <summary>
        /// we override name because we need to manually set it if !CanInitialize() 
        /// since we cannot call base.Initialize in that case.
        /// </summary>
        public override string Name
        {
            get
            {
                return _name;
            }
        }


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
                    _localTempPath = tempPath.FullName;
                    _localStorageType = attemptUseTempStorage.Result;
                }
            }
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>

        public UmbracoExamineSearcher(DirectoryInfo indexPath, Analyzer analyzer)
            : base(indexPath, analyzer)
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="luceneDirectory"></param>
        /// <param name="analyzer"></param>

        public UmbracoExamineSearcher(Lucene.Net.Store.Directory luceneDirectory, Analyzer analyzer)
            : base(luceneDirectory, analyzer)
        {
        }

        #endregion

        /// <summary>
        /// Used for unit tests
        /// </summary>
        internal static bool? DisableInitializationCheck = null;

        /// <summary>
        /// Returns true if the Umbraco application is in a state that we can initialize the examine indexes
        /// </summary>
        /// <returns></returns>

        protected bool CanInitialize()
        {
            //check the DisableInitializationCheck and ensure that it is not set to true
            if (!DisableInitializationCheck.HasValue || !DisableInitializationCheck.Value)
            {
                //We need to check if we actually can initialize, if not then don't continue
                if (ApplicationContext.Current == null
                    || !ApplicationContext.Current.IsConfigured
                    || !ApplicationContext.Current.DatabaseContext.IsDatabaseConfigured)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Override in order to set the nodeTypeAlias field name of the underlying SearchCriteria to __NodeTypeAlias
        /// </summary>
        /// <param name="type"></param>
        /// <param name="defaultOperation"></param>
        /// <returns></returns>
        public override ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
            var criteria = base.CreateSearchCriteria(type, defaultOperation) as LuceneSearchCriteria;
            criteria.NodeTypeAliasField = UmbracoContentIndexer.NodeTypeAliasFieldName;
            return criteria;
        }

        /// <summary>
        /// Returns a list of fields to search on, this will also exclude the IndexPathFieldName and node type alias
        /// </summary>
        /// <returns></returns>
        protected internal override string[] GetSearchFields()
        {
            var fields = base.GetSearchFields();
            return fields
                .Where(x => x != UmbracoContentIndexer.IndexPathFieldName)
                .Where(x => x != UmbracoContentIndexer.NodeTypeAliasFieldName)
                .ToArray();
        }

        protected override IndexReader OpenNewReader()
        {
            var directory = GetLuceneDirectory();
            return IndexReader.Open(
                directory, 
                //DeletePolicyTracker.Current.GetPolicy(directory), 
                true);
        }

        protected override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            //local temp storage is not enabled, just return the default
            if (_localTempPath == null) return base.GetLuceneDirectory();

            //local temp storage is enabled, configure the local directory instance
            if (_localTempDirectory == null)
            {
                lock (Locker)
                {
                    if (_localTempDirectory == null)
                    {
                        switch (_localStorageType)
                        {
                            case LocalStorageType.Sync:
                                var fsDir = base.GetLuceneDirectory() as FSDirectory;
                                if (fsDir != null)
                                {
                                    _localTempDirectory = LocalTempStorageDirectoryTracker.Current.GetDirectory(
                                        new DirectoryInfo(_localTempPath),
                                        fsDir);
                                }
                                else
                                {
                                    return base.GetLuceneDirectory();
                                }
                                break;
                            case LocalStorageType.LocalOnly:
                                _localTempDirectory = DirectoryTracker.Current.GetDirectory(new DirectoryInfo(_localTempPath));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }


                    }
                }
            }

            return _localTempDirectory;
        }
    }
}
