using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.ModelsBuilder.Embedded.BackOffice;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Common.ModelBinders;
using Umbraco.Web.WebAssets;

namespace Umbraco.ModelsBuilder.Embedded
{

    /// <summary>
    /// Handles <see cref="UmbracoApplicationStarting"/> and <see cref="ServerVariablesParsing"/> notifications to initialize MB
    /// </summary>
    internal class ModelsBuilderNotificationHandler : INotificationHandler<UmbracoApplicationStarting>, INotificationHandler<ServerVariablesParsing>
    {
        private readonly ModelsBuilderSettings _config;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly LiveModelsProvider _liveModelsProvider;
        private readonly OutOfDateModelsStatus _outOfDateModels;
        private readonly LinkGenerator _linkGenerator;
        private readonly IUmbracoRequestLifetime _umbracoRequestLifetime;

        public ModelsBuilderNotificationHandler(
            IOptions<ModelsBuilderSettings> config,
            IShortStringHelper shortStringHelper,
            LiveModelsProvider liveModelsProvider,
            OutOfDateModelsStatus outOfDateModels,
            LinkGenerator linkGenerator,
            IUmbracoRequestLifetime umbracoRequestLifetime)
        {
            _config = config.Value;
            _shortStringHelper = shortStringHelper;
            _liveModelsProvider = liveModelsProvider;
            _outOfDateModels = outOfDateModels;
            _shortStringHelper = shortStringHelper;
            _linkGenerator = linkGenerator;
            _umbracoRequestLifetime = umbracoRequestLifetime;
        }

        /// <summary>
        /// Handles the <see cref="UmbracoApplicationStarting"/> notification
        /// </summary>
        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            // always setup the dashboard
            // note: UmbracoApiController instances are automatically registered
            _umbracoRequestLifetime.RequestEnd += (sender, context) => _liveModelsProvider.AppEndRequest(context);

            ContentModelBinder.ModelBindingException += ContentModelBinder_ModelBindingException;

            if (_config.Enable)
            {
                FileService.SavingTemplate += FileService_SavingTemplate;
            }

            if (_config.ModelsMode.IsLiveNotPure())
            {
                _liveModelsProvider.Install();
            }

            if (_config.FlagOutOfDateModels)
            {
                _outOfDateModels.Install();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the <see cref="ServerVariablesParsing"/> notification
        /// </summary>
        public Task HandleAsync(ServerVariablesParsing notification, CancellationToken cancellationToken)
        {
            var serverVars = notification.ServerVariables;

            if (!serverVars.ContainsKey("umbracoUrls"))
            {
                throw new ArgumentException("Missing umbracoUrls.");
            }

            var umbracoUrlsObject = serverVars["umbracoUrls"];
            if (umbracoUrlsObject == null)
            {
                throw new ArgumentException("Null umbracoUrls");
            }

            if (!(umbracoUrlsObject is Dictionary<string, object> umbracoUrls))
            {
                throw new ArgumentException("Invalid umbracoUrls");
            }

            if (!serverVars.ContainsKey("umbracoPlugins"))
            {
                throw new ArgumentException("Missing umbracoPlugins.");
            }

            if (!(serverVars["umbracoPlugins"] is Dictionary<string, object> umbracoPlugins))
            {
                throw new ArgumentException("Invalid umbracoPlugins");
            }

            umbracoUrls["modelsBuilderBaseUrl"] = _linkGenerator.GetUmbracoApiServiceBaseUrl<ModelsBuilderDashboardController>(controller => controller.BuildModels());
            umbracoPlugins["modelsBuilder"] = GetModelsBuilderSettings();

            return Task.CompletedTask;
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
        private void FileService_SavingTemplate(IFileService sender, SaveEventArgs<ITemplate> e)
        {
            // don't do anything if the factory is not enabled
            // because, no factory = no models (even if generation is enabled)
            if (!_config.EnableFactory)
            {
                return;
            }

            // don't do anything if this special key is not found
            if (!e.AdditionalData.ContainsKey("CreateTemplateForContentType"))
            {
                return;
            }

            // ensure we have the content type alias
            if (!e.AdditionalData.ContainsKey("ContentTypeAlias"))
            {
                throw new InvalidOperationException("The additionalData key: ContentTypeAlias was not found");
            }

            foreach (ITemplate template in e.SavedEntities)
            {
                // if it is in fact a new entity (not been saved yet) and the "CreateTemplateForContentType" key
                // is found, then it means a new template is being created based on the creation of a document type
                if (!template.HasIdentity && string.IsNullOrWhiteSpace(template.Content))
                {
                    // ensure is safe and always pascal cased, per razor standard
                    // + this is how we get the default model name in Umbraco.ModelsBuilder.Umbraco.Application
                    var alias = e.AdditionalData["ContentTypeAlias"].ToString();
                    var name = template.Name; // will be the name of the content type since we are creating
                    var className = UmbracoServices.GetClrName(_shortStringHelper, name, alias);

                    var modelNamespace = _config.ModelsNamespace;

                    // we do not support configuring this at the moment, so just let Umbraco use its default value
                    // var modelNamespaceAlias = ...;
                    var markup = ViewHelper.GetDefaultFileContent(
                        modelClassName: className,
                        modelNamespace: modelNamespace/*,
                        modelNamespaceAlias: modelNamespaceAlias*/);

                    // set the template content to the new markup
                    template.Content = markup;
                }
            }
        }

        private void ContentModelBinder_ModelBindingException(object sender, ContentModelBinder.ModelBindingArgs args)
        {
            ModelsBuilderAssemblyAttribute sourceAttr = args.SourceType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();
            ModelsBuilderAssemblyAttribute modelAttr = args.ModelType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();

            // if source or model is not a ModelsBuider type...
            if (sourceAttr == null || modelAttr == null)
            {
                // if neither are ModelsBuilder types, give up entirely
                if (sourceAttr == null && modelAttr == null)
                {
                    return;
                }

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
            {
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
}
