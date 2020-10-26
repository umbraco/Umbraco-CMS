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
            composition.RegisterUnique<IUmbracoIndexConfig, UmbracoIndexConfig>();
            composition.RegisterUnique<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
            composition.RegisterUnique<IPublishedContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetInstance<PropertyEditorCollection>(),
                    factory.GetInstance<UrlSegmentProviderCollection>(),
                    factory.GetInstance<IUserService>(),
                    factory.GetInstance<IShortStringHelper>(),
                    factory.GetInstance<IScopeProvider>(),
                    true));
            composition.RegisterUnique<IContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetInstance<PropertyEditorCollection>(),
                    factory.GetInstance<UrlSegmentProviderCollection>(),
                    factory.GetInstance<IUserService>(),
                    factory.GetInstance<IShortStringHelper>(),
                    factory.GetInstance<IScopeProvider>(),
                    false));
            composition.RegisterUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
            composition.RegisterUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();
            composition.RegisterUnique<BackgroundIndexRebuilder>();
        }
    }
}
