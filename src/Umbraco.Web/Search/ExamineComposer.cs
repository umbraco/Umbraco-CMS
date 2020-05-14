using System.Collections.Generic;
using Examine;
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
            composition.Register<MemberIndexPopulator>(Lifetime.Singleton);
            composition.Register<ContentIndexPopulator>(Lifetime.Singleton);
            composition.Register<PublishedContentIndexPopulator>(Lifetime.Singleton);
            composition.Register<MediaIndexPopulator>(Lifetime.Singleton);

            composition.Register<IndexRebuilder>(Lifetime.Singleton);
            composition.RegisterUnique<IUmbracoIndexConfig, UmbracoIndexConfig>();
            composition.RegisterUnique<IUmbracoIndexesCreator, UmbracoIndexesCreator>();
            composition.RegisterUnique<IPublishedContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetInstance<PropertyEditorCollection>(),
                    factory.GetInstance<UrlSegmentProviderCollection>(),
                    factory.GetInstance<IUserService>(),
                    factory.GetInstance<IScopeProvider>(),
                    true));
            composition.RegisterUnique<IContentValueSetBuilder>(factory =>
                new ContentValueSetBuilder(
                    factory.GetInstance<PropertyEditorCollection>(),
                    factory.GetInstance<UrlSegmentProviderCollection>(),
                    factory.GetInstance<IUserService>(),
                    factory.GetInstance<IScopeProvider>(),
                    false));
            composition.RegisterUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
            composition.RegisterUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();
            composition.RegisterUnique<BackgroundIndexRebuilder>();

            //We want to manage Examine's AppDomain shutdown sequence ourselves so first we'll disable Examine's default behavior
            //and then we'll use MainDom to control Examine's shutdown - this MUST be done in Compose ie before ExamineManager
            //is instantiated, as the value is used during instantiation
            ExamineManager.DisableDefaultHostingEnvironmentRegistration();
        }
    }
}
