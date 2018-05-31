using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.ModelsBuilder.Api;
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.ModelsBuilder.Umbraco
{
    [RequiredComponent(typeof(NuCacheComponent))]
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ModelsBuilderComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Container.Register<UmbracoServices>(new PerContainerLifetime());

            var config = UmbracoConfig.For.ModelsBuilder();

            if (config.ModelsMode == ModelsMode.PureLive)
                ComposeForLiveModels(composition.Container);
            else if (config.EnableFactory)
                ComposeForDefaultModelsFactory(composition.Container);

            // always setup the dashboard
            InstallServerVars(composition.Container.GetInstance<IRuntimeState>().Level);
            composition.Container.Register(typeof(ModelsBuilderBackOfficeController), new PerRequestLifeTime());

            // setup the API if enabled (and in debug mode)
            if (config.ApiServer)
                composition.Container.Register(typeof(ModelsBuilderApiController), new PerRequestLifeTime());
        }

        public void Initialize(UmbracoServices umbracoServices)
        {
            var config = UmbracoConfig.For.ModelsBuilder();

            if (config.Enable)
                FileService.SavingTemplate += FileService_SavingTemplate;

            // fixme LiveModelsProvider should not be static
            if (config.ModelsMode.IsLiveNotPure())
                LiveModelsProvider.Install(umbracoServices);

            // fixme OutOfDateModelsStatus should not be static
            if (config.FlagOutOfDateModels)
                OutOfDateModelsStatus.Install();
        }

        private void ComposeForDefaultModelsFactory(IServiceContainer container)
        {
            container.RegisterSingleton<IPublishedModelFactory>(factory
                => new PublishedModelFactory(factory.GetInstance<TypeLoader>().GetTypes<PublishedContentModel>()));
        }

        private void ComposeForLiveModels(IServiceContainer container)
        {
            container.RegisterSingleton<IPublishedModelFactory, PureLiveModelFactory>();

            // the following would add @using statement in every view so user's don't
            // have to do it - however, then noone understands where the @using statement
            // comes from, and it cannot be avoided / removed --- DISABLED
            //
            /*
            // no need for @using in views
            // note:
            //  we are NOT using the in-code attribute here, config is required
            //  because that would require parsing the code... and what if it changes?
            //  we can AddGlobalImport not sure we can remove one anyways
            var modelsNamespace = Configuration.Config.ModelsNamespace;
            if (string.IsNullOrWhiteSpace(modelsNamespace))
                modelsNamespace = Configuration.Config.DefaultModelsNamespace;
            System.Web.WebPages.Razor.WebPageRazorHost.AddGlobalImport(modelsNamespace);
            */
        }

        private void InstallServerVars(RuntimeLevel level)
        {
            // register our url - for the backoffice api
            ServerVariablesParser.Parsing += (sender, serverVars) =>
            {
                if (!serverVars.ContainsKey("umbracoUrls"))
                    throw new Exception("Missing umbracoUrls.");
                var umbracoUrlsObject = serverVars["umbracoUrls"];
                if (umbracoUrlsObject == null)
                    throw new Exception("Null umbracoUrls");
                if (!(umbracoUrlsObject is Dictionary<string, object> umbracoUrls))
                    throw new Exception("Invalid umbracoUrls");

                if (!serverVars.ContainsKey("umbracoPlugins"))
                    throw new Exception("Missing umbracoPlugins.");
                if (!(serverVars["umbracoPlugins"] is Dictionary<string, object> umbracoPlugins))
                    throw new Exception("Invalid umbracoPlugins");

                if (HttpContext.Current == null) throw new InvalidOperationException("HttpContext is null");
                var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

                umbracoUrls["modelsBuilderBaseUrl"] = urlHelper.GetUmbracoApiServiceBaseUrl<ModelsBuilderBackOfficeController>(controller => controller.BuildModels());
                umbracoPlugins["modelsBuilder"] = GetModelsBuilderSettings(level);
            };
        }

        private Dictionary<string, object> GetModelsBuilderSettings(RuntimeLevel level)
        {
            if (level != RuntimeLevel.Run)
                return null;

            var settings = new Dictionary<string, object>
            {
                {"enabled", UmbracoConfig.For.ModelsBuilder().Enable}
            };

            return settings;
        }

        /// <summary>
        /// Used to check if a template is being created based on a document type, in this case we need to
        /// ensure the template markup is correct based on the model name of the document type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileService_SavingTemplate(IFileService sender, Core.Events.SaveEventArgs<Core.Models.ITemplate> e)
        {
            // don't do anything if the factory is not enabled
            // because, no factory = no models (even if generation is enabled)
            if (!UmbracoConfig.For.ModelsBuilder().EnableFactory) return;

            // don't do anything if this special key is not found
            if (!e.AdditionalData.ContainsKey("CreateTemplateForContentType")) return;

            // ensure we have the content type alias
            if (!e.AdditionalData.ContainsKey("ContentTypeAlias"))
                throw new InvalidOperationException("The additionalData key: ContentTypeAlias was not found");

            foreach (var template in e.SavedEntities)
            {
                // if it is in fact a new entity (not been saved yet) and the "CreateTemplateForContentType" key
                // is found, then it means a new template is being created based on the creation of a document type
                if (!template.HasIdentity && string.IsNullOrWhiteSpace(template.Content))
                {
                    // ensure is safe and always pascal cased, per razor standard
                    // + this is how we get the default model name in Umbraco.ModelsBuilder.Umbraco.Application
                    var alias = e.AdditionalData["ContentTypeAlias"].ToString();
                    var name = template.Name; // will be the name of the content type since we are creating
                    var className = UmbracoServices.GetClrName(name, alias);

                    var modelNamespace = UmbracoConfig.For.ModelsBuilder().ModelsNamespace;

                    // we do not support configuring this at the moment, so just let Umbraco use its default value
                    //var modelNamespaceAlias = ...;

                    var markup = ViewHelper.GetDefaultFileContent(
                        modelClassName: className,
                        modelNamespace: modelNamespace/*,
                        modelNamespaceAlias: modelNamespaceAlias*/);

                    //set the template content to the new markup
                    template.Content = markup;
                }
            }
        }
    }
}
