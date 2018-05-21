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
using Umbraco.ModelsBuilder.Configuration;
using Umbraco.Web;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.ModelsBuilder.Umbraco
{
    /// <summary>
    /// Installs ModelsBuilder into the Umbraco site.
    /// </summary>
    /// <remarks>
    /// <para>Don't bother installing at all, if not RuntimeLevel.Run.</para>
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ModelsBuilderApplication : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private IRuntimeState _runtimeState;

        public override void Compose(Composition composition)
        {
            var config = UmbracoConfig.For.ModelsBuilder();

            if (config.ModelsMode == ModelsMode.PureLive)
                InstallLiveModels(composition.Container);
            else if (config.EnableFactory)
                InstallDefaultModelsFactory(composition.Container);

            // always setup the dashboard
            InstallServerVars();
        }

        public void Initialize(IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;

            var config = UmbracoConfig.For.ModelsBuilder();

            if (config.Enable)
                FileService.SavingTemplate += FileService_SavingTemplate;

            if (config.ModelsMode.IsLiveNotPure())
                LiveModelsProvider.Install();

            if (config.FlagOutOfDateModels)
                OutOfDateModelsStatus.Install();
        }

        private void InstallDefaultModelsFactory(IServiceContainer container)
        {
            var types = Current.TypeLoader.GetTypes<PublishedContentModel>();
            var factory = new PublishedModelFactory(types);
            container.RegisterSingleton<IPublishedModelFactory>(_ => factory);
        }

        private void InstallLiveModels(IServiceContainer container)
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

        private void InstallServerVars()
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
                umbracoPlugins["modelsBuilder"] = GetModelsBuilderSettings();
            };
        }

        private Dictionary<string, object> GetModelsBuilderSettings()
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
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
                if (!template.HasIdentity && template.Content.IsNullOrWhiteSpace())
                {
                    // ensure is safe and always pascal cased, per razor standard
                    // + this is how we get the default model name in Umbraco.ModelsBuilder.Umbraco.Application
                    var alias = e.AdditionalData["ContentTypeAlias"].ToString();
                    var name = template.Name; // will be the name of the content type since we are creating
                    var className = Application.GetClrName(name, alias);

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
