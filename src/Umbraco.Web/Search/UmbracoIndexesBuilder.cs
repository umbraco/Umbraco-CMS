using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Examine;
using Umbraco.Core.Persistence;
using Umbraco.Core.IO;
using System.IO;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Examine.LuceneEngine;
using Examine;
using Examine.LuceneEngine.Providers;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Creates the indexes used by Umbraco
    /// </summary>
    public class UmbracoIndexesBuilder : IUmbracoIndexesBuilder
    {
        //TODO: we should inject the different IValueSetValidator so devs can just register them instead of overriding this class?

        public UmbracoIndexesBuilder(ProfilingLogger profilingLogger,
            ILocalizationService languageService,
            IPublicAccessService publicAccessService,
            IMemberService memberService)
        {
            ProfilingLogger = profilingLogger ?? throw new System.ArgumentNullException(nameof(profilingLogger));
            LanguageService = languageService ?? throw new System.ArgumentNullException(nameof(languageService));
            PublicAccessService = publicAccessService ?? throw new System.ArgumentNullException(nameof(publicAccessService));
            MemberService = memberService ?? throw new System.ArgumentNullException(nameof(memberService));
        }

        protected ProfilingLogger ProfilingLogger { get; }
        protected ILocalizationService LanguageService { get; }
        protected IPublicAccessService PublicAccessService { get; }
        protected IMemberService MemberService { get; }

        /// <summary>
        /// By default these are the member fields we index
        /// </summary>
        public static readonly string[] DefaultMemberIndexFields = new[] { "id", "nodeName", "updateDate", "writerName", "loginName", "email", "nodeTypeAlias" };

        /// <summary>
        /// Creates the Umbraco indexes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IIndexer> Create()
        {
            return new []
            {
                CreateContentIndex(Constants.UmbracoIndexes.InternalIndexName, Constants.UmbracoIndexes.InternalIndexPath, new UmbracoContentIndexerOptions(true, true, null), new CultureInvariantWhitespaceAnalyzer()),
                CreateContentIndex(Constants.UmbracoIndexes.ExternalIndexName, Constants.UmbracoIndexes.ExternalIndexPath, new UmbracoContentIndexerOptions(false, false, null), new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30)),
                CreateMemberIndex()
            };
        }

        private IIndexer CreateContentIndex(string name, string path, UmbracoContentIndexerOptions options, Analyzer analyzer)
        {
            var index = new UmbracoContentIndexer(
                name,
                //fixme - how to deal with languages like in UmbracoContentIndexer.CreateFieldValueTypes
                UmbracoExamineIndexer.UmbracoIndexFieldDefinitions,
                GetFileSystemLuceneDirectory(path),
                analyzer,
                ProfilingLogger,
                LanguageService, 
                GetContentValueSetValidator(options),
                options);
            return index;
        }

        private IIndexer CreateMemberIndex()
        {
            var index = new UmbracoMemberIndexer(
                Constants.UmbracoIndexes.MembersIndexName,
                //fixme - how to deal with languages like in UmbracoContentIndexer.CreateFieldValueTypes
                UmbracoExamineIndexer.UmbracoIndexFieldDefinitions,
                GetFileSystemLuceneDirectory(Constants.UmbracoIndexes.MembersIndexPath),
                new CultureInvariantWhitespaceAnalyzer(),
                ProfilingLogger, 
                GetMemberValueSetValidator());
            return index;
        }

        public virtual Lucene.Net.Store.Directory GetFileSystemLuceneDirectory(string name)
        {
            var dirInfo = new DirectoryInfo(Path.Combine(IOHelper.MapPath(SystemDirectories.Data), "TEMP", "ExamineIndexes", name));
            if (!dirInfo.Exists)
                System.IO.Directory.CreateDirectory(dirInfo.FullName);

            var luceneDir = new SimpleFSDirectory(dirInfo);
            //we want to tell examine to use a different fs lock instead of the default NativeFSFileLock which could cause problems if the appdomain
            //terminates and in some rare cases would only allow unlocking of the file if IIS is forcefully terminated. Instead we'll rely on the simplefslock
            //which simply checks the existence of the lock file
            luceneDir.SetLockFactory(new NoPrefixSimpleFsLockFactory(dirInfo));
            return luceneDir;
        }

        public virtual IValueSetValidator GetContentValueSetValidator(UmbracoContentIndexerOptions options)
        {
            return new UmbracoContentValueSetValidator(options, PublicAccessService);
        }

        /// <summary>
        /// Returns the <see cref="IValueSetValidator"/> for the member indexer
        /// </summary>
        /// <returns></returns>
        public virtual IValueSetValidator GetMemberValueSetValidator()
        {
            //This validator is used purely to filter the value set
            return new ValueSetValidatorDelegate(valueSet =>
            {
                
                foreach(var key in valueSet.Values.Keys.ToList())
                {
                    if (!DefaultMemberIndexFields.InvariantContains(key))
                        valueSet.Values.Remove(key); //remove any value with a key that doesn't match our list
                }
                
                return true;
            });
        }
        
    }
}
