using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Lucene.Net.Analysis.Standard;
using Examine.LuceneEngine;
using Examine;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Examine
{

    /// <summary>
    /// Creates the indexes used by Umbraco
    /// </summary>
    public class UmbracoIndexesCreator : LuceneIndexCreator, IUmbracoIndexesCreator
    {
        // TODO: we should inject the different IValueSetValidator so devs can just register them instead of overriding this class?

        public UmbracoIndexesCreator(
            ITypeFinder typeFinder,
            IProfilingLogger profilingLogger,
            ILoggerFactory loggerFactory,
            ILocalizationService languageService,
            IPublicAccessService publicAccessService,
            IMemberService memberService,
            IUmbracoIndexConfig umbracoIndexConfig,
            IHostingEnvironment hostingEnvironment,
            IRuntimeState runtimeState,
            IOptions<IndexCreatorSettings> settings,
            ILuceneDirectoryFactory directoryFactory) : base(typeFinder, hostingEnvironment, settings)
        {
            ProfilingLogger = profilingLogger ?? throw new System.ArgumentNullException(nameof(profilingLogger));
            LoggerFactory = loggerFactory;
            LanguageService = languageService ?? throw new System.ArgumentNullException(nameof(languageService));
            PublicAccessService = publicAccessService ?? throw new System.ArgumentNullException(nameof(publicAccessService));
            MemberService = memberService ?? throw new System.ArgumentNullException(nameof(memberService));
            UmbracoIndexConfig = umbracoIndexConfig;
            HostingEnvironment = hostingEnvironment ?? throw new System.ArgumentNullException(nameof(hostingEnvironment));
            RuntimeState = runtimeState ?? throw new System.ArgumentNullException(nameof(runtimeState));
            DirectoryFactory = directoryFactory;
        }

        protected IProfilingLogger ProfilingLogger { get; }
        protected ILoggerFactory LoggerFactory { get; }
        protected IHostingEnvironment HostingEnvironment { get; }
        protected IRuntimeState RuntimeState { get; }
        protected ILuceneDirectoryFactory DirectoryFactory { get; }
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
            return new[]
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
                DirectoryFactory.CreateDirectory(Constants.UmbracoIndexes.InternalIndexPath),
                new UmbracoFieldDefinitionCollection(),
                new CultureInvariantWhitespaceAnalyzer(),
                ProfilingLogger,
                LoggerFactory.CreateLogger<UmbracoContentIndex>(),
                LoggerFactory,
                HostingEnvironment,
                RuntimeState,
                LanguageService,
                UmbracoIndexConfig.GetContentValueSetValidator()
                );
            return index;
        }

        private IIndex CreateExternalIndex()
        {
            var index = new UmbracoContentIndex(
                Constants.UmbracoIndexes.ExternalIndexName,
                DirectoryFactory.CreateDirectory(Constants.UmbracoIndexes.ExternalIndexPath),
                new UmbracoFieldDefinitionCollection(),
                new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                ProfilingLogger,
                LoggerFactory.CreateLogger<UmbracoContentIndex>(),
                LoggerFactory,
                HostingEnvironment,
                RuntimeState,
                LanguageService,
                UmbracoIndexConfig.GetPublishedContentValueSetValidator());
            return index;
        }

        private IIndex CreateMemberIndex()
        {
            var index = new UmbracoMemberIndex(
                Constants.UmbracoIndexes.MembersIndexName,
                new UmbracoFieldDefinitionCollection(),
                DirectoryFactory.CreateDirectory(Constants.UmbracoIndexes.MembersIndexPath),
                new CultureInvariantWhitespaceAnalyzer(),
                ProfilingLogger,
                LoggerFactory.CreateLogger<UmbracoMemberIndex>(),
                LoggerFactory,
                HostingEnvironment,
                RuntimeState,
                UmbracoIndexConfig.GetMemberValueSetValidator()
                );
            return index;
        }
    }
}
