using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.ModelsBuilder.Embedded.BackOffice;
using Umbraco.ModelsBuilder.Embedded.Configuration;
using Umbraco.Web;
using Umbraco.Web.JavaScript;
using Umbraco.Web.Mvc;

namespace Umbraco.ModelsBuilder.Embedded.Compose
{

    internal class ModelsBuilderComponent : IComponent
    {

        private readonly IModelsBuilderConfig _config;
        private readonly LiveModelsProvider _liveModelsProvider;
        private readonly OutOfDateModelsStatus _outOfDateModels;

        public ModelsBuilderComponent(IModelsBuilderConfig config, LiveModelsProvider liveModelsProvider, OutOfDateModelsStatus outOfDateModels)
        {
            _config = config;
            _liveModelsProvider = liveModelsProvider;
            _outOfDateModels = outOfDateModels;
        }

        public void Initialize()
        {
            // always setup the dashboard
            // note: UmbracoApiController instances are automatically registered
            InstallServerVars();

            ContentModelBinder.ModelBindingException += ContentModelBinder_ModelBindingException;

            if (_config.Enable)
                FileService.SavingTemplate += FileService_SavingTemplate;

            if (_config.ModelsMode.IsLiveNotPure())
                _liveModelsProvider.Install();

            if (_config.FlagOutOfDateModels)
                _outOfDateModels.Install();
        }

        public void Terminate()
        {
            ServerVariablesParser.Parsing -= ServerVariablesParser_Parsing;
            ContentModelBinder.ModelBindingException -= ContentModelBinder_ModelBindingException;
            FileService.SavingTemplate -= FileService_SavingTemplate;
        }

        private void InstallServerVars()
        {
            // register our URL - for the backoffice API
            ServerVariablesParser.Parsing += ServerVariablesParser_Parsing;
        }

        private void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> serverVars)
        {
            if (!serverVars.ContainsKey("umbracoUrls"))
                throw new ArgumentException("Missing umbracoUrls.");
            var umbracoUrlsObject = serverVars["umbracoUrls"];
            if (umbracoUrlsObject == null)
                throw new ArgumentException("Null umbracoUrls");
            if (!(umbracoUrlsObject is Dictionary<string, object> umbracoUrls))
                throw new ArgumentException("Invalid umbracoUrls");

            if (!serverVars.ContainsKey("umbracoPlugins"))
                throw new ArgumentException("Missing umbracoPlugins.");
            if (!(serverVars["umbracoPlugins"] is Dictionary<string, object> umbracoPlugins))
                throw new ArgumentException("Invalid umbracoPlugins");

            if (HttpContext.Current == null) throw new InvalidOperationException("HttpContext is null");
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            umbracoUrls["modelsBuilderBaseUrl"] = urlHelper.GetUmbracoApiServiceBaseUrl<ModelsBuilderDashboardController>(controller => controller.BuildModels());
            umbracoPlugins["modelsBuilder"] = GetModelsBuilderSettings();
        }

        private Dictionary<string, object> GetModelsBuilderSettings()
        {
            var settings = new Dictionary<string, object>
            {
                {"enabled", _config.Enable}
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
            if (!_config.EnableFactory) return;

            // don't do anything if this special key is not found
            if (!e.AdditionalData.ContainsKey("CreateTemplateForContentType")) return;

            // ensure we have the content type alias
            if (!e.AdditionalData.ContainsKey("ContentTypeAlias"))
                throw new InvalidOperationException("The additionalData key: ContentTypeAlias was not found");

            foreach (var template in e.SavedEntities)
                // if it is in fact a new entity (not been saved yet) and the "CreateTemplateForContentType" key
                // is found, then it means a new template is being created based on the creation of a document type
                if (!template.HasIdentity && string.IsNullOrWhiteSpace(template.Content))
                {
                    // ensure is safe and always pascal cased, per razor standard
                    // + this is how we get the default model name in Umbraco.ModelsBuilder.Umbraco.Application
                    var alias = e.AdditionalData["ContentTypeAlias"].ToString();
                    var name = template.Name; // will be the name of the content type since we are creating
                    var className = UmbracoServices.GetClrName(name, alias);

                    var modelNamespace = _config.ModelsNamespace;

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

        private void ContentModelBinder_ModelBindingException(object sender, ContentModelBinder.ModelBindingArgs args)
        {
            var sourceAttr = args.SourceType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();
            var modelAttr = args.ModelType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();

            // if source or model is not a ModelsBuider type...
            if (sourceAttr == null || modelAttr == null)
            {
                // if neither are ModelsBuilder types, give up entirely
                if (sourceAttr == null && modelAttr == null)
                    return;

                // else report, but better not restart (loops?)
                args.Message.Append(" The ");
                args.Message.Append(sourceAttr == null ? "view model" : "source");
                args.Message.Append(" is a ModelsBuilder type, but the ");
                args.Message.Append(sourceAttr != null ? "view model" : "source");
                args.Message.Append(" is not. The application is in an unstable state and should be restarted.");
                return;
            }

            // both are ModelsBuilder types
            var pureSource = sourceAttr.PureLive;
            var pureModel = modelAttr.PureLive;

            if (sourceAttr.PureLive || modelAttr.PureLive)
                if (pureSource == false || pureModel == false)
                {
                    // only one is pure - report, but better not restart (loops?)
                    args.Message.Append(pureSource
                        ? " The content model is PureLive, but the view model is not."
                        : " The view model is PureLive, but the content model is not.");
                    args.Message.Append(" The application is in an unstable state and should be restarted.");
                }
                else
                {
                    // both are pure - report, and if different versions, restart
                    // if same version... makes no sense... and better not restart (loops?)
                    var sourceVersion = args.SourceType.Assembly.GetName().Version;
                    var modelVersion = args.ModelType.Assembly.GetName().Version;
                    args.Message.Append(" Both view and content models are PureLive, with ");
                    args.Message.Append(sourceVersion == modelVersion
                        ? "same version. The application is in an unstable state and should be restarted."
                        : "different versions. The application is in an unstable state and is going to be restarted.");
                    args.Restart = sourceVersion != modelVersion;
                }
        }
    }
}
