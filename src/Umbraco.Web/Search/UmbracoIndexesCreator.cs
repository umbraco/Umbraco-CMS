using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Lucene.Net.Analysis.Standard;
using Examine.LuceneEngine;
using Examine;
using Umbraco.Core;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Creates the indexes used by Umbraco
    /// </summary>
    public class UmbracoIndexesCreator : LuceneIndexCreator, IUmbracoIndexesCreator
    {
        // TODO: we should inject the different IValueSetValidator so devs can just register them instead of overriding this class?

        public UmbracoIndexesCreator(IProfilingLogger profilingLogger,
            ILocalizationService languageService,
            IPublicAccessService publicAccessService,
            IMemberService memberService, IUmbracoIndexConfig umbracoIndexConfig)
        {
            ProfilingLogger = profilingLogger ?? throw new System.ArgumentNullException(nameof(profilingLogger));
            LanguageService = languageService ?? throw new System.ArgumentNullException(nameof(languageService));
            PublicAccessService = publicAccessService ?? throw new System.ArgumentNullException(nameof(publicAccessService));
            MemberService = memberService ?? throw new System.ArgumentNullException(nameof(memberService));
            UmbracoIndexConfig = umbracoIndexConfig;
        }

        protected IProfilingLogger ProfilingLogger { get; }
        protected ILocalizationService LanguageService { get; }
        protected IPublicAccessService PublicAccessService { get; }
        protected IMemberService MemberService { get; }
        protected IUmbracoIndexConfig UmbracoIndexConfig { get; }

        /// <summary>
        /// Creates the Umbraco indexes
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IIndex> Create()
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
                CreateFileSystemLuceneDirectory(Constants.UmbracoIndexes.InternalIndexPath),
                new UmbracoFieldDefinitionCollection(),
                new CultureInvariantWhitespaceAnalyzer(),
                ProfilingLogger,
                LanguageService,
                UmbracoIndexConfig.GetContentValueSetValidator());
            return index;
        }

        private IIndex CreateExternalIndex()
        {
            var index = new UmbracoContentIndex(
                Constants.UmbracoIndexes.ExternalIndexName,
                CreateFileSystemLuceneDirectory(Constants.UmbracoIndexes.ExternalIndexPath),
                new UmbracoFieldDefinitionCollection(),
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                ProfilingLogger,
                LanguageService,
                UmbracoIndexConfig.GetPublishedContentValueSetValidator());
            return index;
        }

        private IIndex CreateMemberIndex()
        {
            var index = new UmbracoMemberIndex(
                Constants.UmbracoIndexes.MembersIndexName,
                new UmbracoFieldDefinitionCollection(),
                CreateFileSystemLuceneDirectory(Constants.UmbracoIndexes.MembersIndexPath),
                new CultureInvariantWhitespaceAnalyzer(),
                ProfilingLogger,
                UmbracoIndexConfig.GetMemberValueSetValidator());
            return index;
        }



    }
}
