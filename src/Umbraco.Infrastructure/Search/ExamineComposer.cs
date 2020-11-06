using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
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
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ExamineComposer : ComponentComposer<ExamineComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // populators are not a collection: one cannot remove ours, and can only add more
            // the container can inject IEnumerable<IIndexPopulator> and get them all
            composition.Services.AddSingleton<MemberIndexPopulator>();
            composition.Services.AddSingleton<ContentIndexPopulator>();
            composition.Services.AddSingleton<PublishedContentIndexPopulator>();
            composition.Services.AddSingleton<MediaIndexPopulator>();

            composition.Services.AddSingleton<IndexRebuilder>();
            composition.Services.AddUnique<IUmbracoIndexConfig, UmbracoIndexConfig>();
            composition.Services.AddUnique<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
            composition.Services.AddUnique<IPublishedContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetRequiredService<PropertyEditorCollection>(),
                    factory.GetRequiredService<UrlSegmentProviderCollection>(),
                    factory.GetRequiredService<IUserService>(),
                    factory.GetRequiredService<IShortStringHelper>(),
                    factory.GetRequiredService<IScopeProvider>(),
                    true));
            composition.Services.AddUnique<IContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetRequiredService<PropertyEditorCollection>(),
                    factory.GetRequiredService<UrlSegmentProviderCollection>(),
                    factory.GetRequiredService<IUserService>(),
                    factory.GetRequiredService<IShortStringHelper>(),
                    factory.GetRequiredService<IScopeProvider>(),
                    false));
            composition.Services.AddUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
            composition.Services.AddUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();
            composition.Services.AddUnique<BackgroundIndexRebuilder>();
        }
    }
}
