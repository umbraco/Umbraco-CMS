using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search
{
    /// <summary>
    /// Configures and installs Examine.
    /// </summary>
    public sealed class ExamineComposer : ComponentComposer<ExamineComponent>, ICoreComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            // populators are not a collection: one cannot remove ours, and can only add more
            // the container can inject IEnumerable<IIndexPopulator> and get them all
            builder.Services.AddSingleton<MemberIndexPopulator>();
            builder.Services.AddSingleton<ContentIndexPopulator>();
            builder.Services.AddSingleton<PublishedContentIndexPopulator>();
            builder.Services.AddSingleton<MediaIndexPopulator>();

            builder.Services.AddSingleton<IndexRebuilder>();
            builder.Services.AddUnique<IUmbracoIndexConfig, UmbracoIndexConfig>();
            builder.Services.AddUnique<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
            builder.Services.AddUnique<IPublishedContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetRequiredService<PropertyEditorCollection>(),
                    factory.GetRequiredService<UrlSegmentProviderCollection>(),
                    factory.GetRequiredService<IUserService>(),
                    factory.GetRequiredService<IShortStringHelper>(),
                    factory.GetRequiredService<IScopeProvider>(),
                    true));
            builder.Services.AddUnique<IContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetRequiredService<PropertyEditorCollection>(),
                    factory.GetRequiredService<UrlSegmentProviderCollection>(),
                    factory.GetRequiredService<IUserService>(),
                    factory.GetRequiredService<IShortStringHelper>(),
                    factory.GetRequiredService<IScopeProvider>(),
                    false));
            builder.Services.AddUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
            builder.Services.AddUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();
            builder.Services.AddUnique<BackgroundIndexRebuilder>();
        }
    }
}
