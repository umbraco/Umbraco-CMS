using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Examine.Config;
using Examine.LuceneEngine;

namespace Umbraco.Examine
{
    /// <summary>
    /// An indexer for Umbraco content and media
    /// </summary>
    public class UmbracoContentIndexer : UmbracoExamineIndexer, IUmbracoContentIndexer, IUmbracoMediaIndexer
    {
        public const string VariesByCultureFieldName = UmbracoExamineIndexer.SpecialFieldPrefix + "VariesByCulture";
        protected ILocalizationService LanguageService { get; }

        private int? _parentId;

        #region Constructors

        /// <summary>
        /// Constructor for configuration providers
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public UmbracoContentIndexer()
        {
            LanguageService = Current.Services.LocalizationService;
        }

        /// <summary>
        /// Create an index at runtime
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fieldDefinitions"></param>
        /// <param name="luceneDirectory"></param>
        /// <param name="defaultAnalyzer"></param>
        /// <param name="profilingLogger"></param>
        /// <param name="validator"></param>
        /// <param name="options"></param>
        /// <param name="indexValueTypes"></param>
        public UmbracoContentIndexer(
            string name,
            IEnumerable<FieldDefinition> fieldDefinitions,
            Directory luceneDirectory,
            Analyzer defaultAnalyzer,
            ProfilingLogger profilingLogger,
            ILocalizationService languageService,
            IValueSetValidator validator,
            UmbracoContentIndexerOptions options,
            IReadOnlyDictionary<string, Func<string, IIndexValueType>> indexValueTypes = null)
            : base(name, fieldDefinitions, luceneDirectory, defaultAnalyzer, profilingLogger, validator, indexValueTypes)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (options == null) throw new ArgumentNullException(nameof(options));

            SupportProtectedContent = options.SupportProtectedContent;
            SupportUnpublishedContent = options.SupportUnpublishedContent;
            ParentId = options.ParentId;
            LanguageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        }


        #endregion

        #region Initialize

        /// <summary>
        /// Set up all properties for the indexer based on configuration information specified. This will ensure that
        /// all of the folders required by the indexer are created and exist. This will also create an instruction
        /// file declaring the computer name that is part taking in the indexing. This file will then be used to
        /// determine the master indexer machine in a load balanced environment (if one exists).
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The name of the provider is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The name of the provider has a length of zero.
        /// </exception>
        /// <exception cref="T:System.InvalidOperationException">
        /// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            //check if there's a flag specifying to support unpublished content,
            //if not, set to false;
            if (config["supportUnpublished"] != null && bool.TryParse(config["supportUnpublished"], out var supportUnpublished))
                SupportUnpublishedContent = supportUnpublished;
            else
                SupportUnpublishedContent = false;

            //check if there's a flag specifying to support protected content,
            //if not, set to false;
            if (config["supportProtected"] != null && bool.TryParse(config["supportProtected"], out var supportProtected))
                SupportProtectedContent = supportProtected;
            else
                SupportProtectedContent = false;

            //now we need to build up the indexer options so we can create our validator
            int? parentId = null;
            if (IndexSetName.IsNullOrWhiteSpace() == false)
            {
                var indexSet = IndexSets.Instance.Sets[IndexSetName];
                parentId = indexSet.IndexParentId;
            }

            ValueSetValidator = new UmbracoContentValueSetValidator(
                new UmbracoContentIndexerOptions(
                    SupportUnpublishedContent, SupportProtectedContent, parentId,
                    ConfigIndexCriteria.IncludeItemTypes, ConfigIndexCriteria.ExcludeItemTypes),
                //Using a singleton here, we can't inject this when using config based providers and we don't use this
                //anywhere else in this class
                Current.Services.PublicAccessService);

        }

        #endregion

        #region Properties

        /// <summary>
        /// By default this is false, if set to true then the indexer will include indexing content that is flagged as publicly protected.
        /// This property is ignored if SupportUnpublishedContent is set to true.
        /// </summary>
        public bool SupportProtectedContent { get; protected set; }

        /// <summary>
        /// If set this will filter the content items allowed to be indexed
        /// </summary>
        public int? ParentId
        {
            get => _parentId ?? ConfigIndexCriteria?.ParentNodeId;
            protected set => _parentId = value;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Deletes a node from the index.
        /// </summary>
        /// <remarks>
        /// When a content node is deleted, we also need to delete it's children from the index so we need to perform a
        /// custom Lucene search to find all decendents and create Delete item queues for them too.
        /// </remarks>
        /// <param name="nodeId">ID of the node to delete</param>
        public override void DeleteFromIndex(string nodeId)
        {
            //find all descendants based on path
            var descendantPath = $@"\-1\,*{nodeId}\,*";
            var rawQuery = $"{IndexPathFieldName}:{descendantPath}";
            var searcher = GetSearcher();
            var c = searcher.CreateCriteria();
            var filtered = c.RawQuery(rawQuery);
            var results = searcher.Search(filtered);

            ProfilingLogger.Logger.Debug(GetType(), "DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery, results.TotalItemCount);

            //need to queue a delete item for each one found
            foreach (var r in results)
            {
                QueueIndexOperation(new IndexOperation(IndexItem.ForId(r.Id),  IndexOperationType.Delete));
            }

            base.DeleteFromIndex(nodeId);
        }
        #endregion

        #region Protected

        /// <summary>
        /// Overridden to ensure that the variant system fields have the right value types
        /// </summary>
        /// <param name="x"></param>
        /// <param name="indexValueTypesFactory"></param>
        /// <returns></returns>
        protected override FieldValueTypeCollection CreateFieldValueTypes(IReadOnlyDictionary<string, Func<string, IIndexValueType>> indexValueTypesFactory = null)
        {
            //fixme: languages are dynamic so although this will work on startup it wont work when languages are edited
            foreach(var lang in LanguageService.GetAllLanguages())
            {
                foreach (var field in UmbracoIndexFieldDefinitions)
                {
                    var def = new FieldDefinition($"{field.Name}_{lang.IsoCode.ToLowerInvariant()}", field.Type);
                    FieldDefinitionCollection.TryAdd(def.Name, def);
                }
            }

            return base.CreateFieldValueTypes(indexValueTypesFactory);
        }


        #endregion
    }
}
