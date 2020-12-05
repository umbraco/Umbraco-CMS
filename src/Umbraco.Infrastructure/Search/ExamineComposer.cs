using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Examine;

namespace Umbraco.Web.Search
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
