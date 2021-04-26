// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.InteropServices;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the Examine.Lucene
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Umbraco preview support
        /// </summary>
        public static IUmbracoBuilder AddExamineLucene(this IUmbracoBuilder builder)
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (!isWindows)
            {
                return builder;
            }

            builder.AddNotificationHandler<UmbracoApplicationStarting, ExamineLuceneStarting>();
            builder.Services.AddUnique<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
            builder.Services.AddUnique<IUmbracoIndexesCreator, UmbracoIndexesCreator>();
            builder.Services.AddUnique<IIndexDiagnosticsFactory, LuceneIndexDiagnosticsFactory>();
            builder.Services.AddUnique<ILuceneDirectoryFactory, LuceneFileSystemDirectoryFactory>();

            return builder;
        }

        public static IUmbracoBuilder AddExamineIndexConfiguration(this IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<UmbracoApplicationStarting, ExamineLuceneConfigureIndexes>();


            return builder;
        }

    }
}
