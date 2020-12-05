using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Lucene.Net.Analysis.Standard;
using Examine.LuceneEngine;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Composing;

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
            IMemberService memberService, IUmbracoIndexConfig umbracoIndexConfig,
            IFactory factory) : base(factory)
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
                GetContentValueSetValidator()
                );
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
                GetPublishedContentValueSetValidator());
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
                GetMemberValueSetValidator()
                );
            return index;
        }
        [Obsolete("This method should not be used and will be removed in future versions. GetContentValueSetValidator was moved to IUmbracoIndexConfig")]
        public virtual IContentValueSetValidator GetContentValueSetValidator()
        {
            return UmbracoIndexConfig.GetContentValueSetValidator();
        }
        [Obsolete("This method should not be used and will be removed in future versions. GetPublishedContentValueSetValidator was moved to IUmbracoIndexConfig")]
        public virtual IContentValueSetValidator GetPublishedContentValueSetValidator()
        {
            return UmbracoIndexConfig.GetPublishedContentValueSetValidator();
        }

        /// <summary>
        /// Returns the <see cref="IValueSetValidator"/> for the member indexer
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method should not be used and will be removed in future versions. GetMemberValueSetValidator was moved to IUmbracoIndexConfig")]
        public virtual IValueSetValidator GetMemberValueSetValidator()
        {
            return UmbracoIndexConfig.GetMemberValueSetValidator();
        }


    }
}
