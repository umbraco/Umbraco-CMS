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
    public class UmbracoIndexesCreator : IUmbracoIndexesCreator
    {
        //TODO: we should inject the different IValueSetValidator so devs can just register them instead of overriding this class?

        public UmbracoIndexesCreator(ProfilingLogger profilingLogger,
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
        /// Creates the Umbraco indexes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IIndex> Create()
        {
            return new []
            {
                CreateInternalIndex(),
                CreateExternalIndex(),
                CreateMemberIndex()
            };
        }

        private IIndex CreateInternalIndex()
        {
            var index = new UmbracoContentIndex(
                Constants.UmbracoIndexes.InternalIndexName,
                //fixme - how to deal with languages like in UmbracoContentIndexer.CreateFieldValueTypes
                UmbracoExamineIndex.UmbracoIndexFieldDefinitions,
                GetFileSystemLuceneDirectory(Constants.UmbracoIndexes.InternalIndexPath),
                new CultureInvariantWhitespaceAnalyzer(),
                ProfilingLogger,
                LanguageService, 
                GetContentValueSetValidator());
            return index;
        }

        private IIndex CreateExternalIndex()
        {
            var index = new UmbracoContentIndex(
                Constants.UmbracoIndexes.ExternalIndexName,
                //fixme - how to deal with languages like in UmbracoContentIndexer.CreateFieldValueTypes
                UmbracoExamineIndex.UmbracoIndexFieldDefinitions,
                GetFileSystemLuceneDirectory(Constants.UmbracoIndexes.ExternalIndexPath),
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                ProfilingLogger,
                LanguageService,
                GetPublishedContentValueSetValidator());
            return index;
        }

        private IIndex CreateMemberIndex()
        {
            var index = new UmbracoMemberIndex(
                Constants.UmbracoIndexes.MembersIndexName,
                //fixme - how to deal with languages like in UmbracoContentIndexer.CreateFieldValueTypes
                UmbracoExamineIndex.UmbracoIndexFieldDefinitions,
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

        public virtual IContentValueSetValidator GetContentValueSetValidator()
        {
            return new ContentValueSetValidator(false, true, PublicAccessService);
        }

        public virtual IContentValueSetValidator GetPublishedContentValueSetValidator()
        {
            return new ContentValueSetValidator(true, false, PublicAccessService);
        }

        /// <summary>
        /// Returns the <see cref="IValueSetValidator"/> for the member indexer
        /// </summary>
        /// <returns></returns>
        public virtual IValueSetValidator GetMemberValueSetValidator()
        {
            return new MemberValueSetValidator();
        }
        
    }
}
