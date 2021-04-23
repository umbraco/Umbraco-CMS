using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Notifications;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace Umbraco.Cms.Web.Common.ModelsBuilder
{
    /// <summary>
    /// Handles <see cref="UmbracoApplicationStarting"/> and <see cref="ServerVariablesParsing"/> notifications to initialize MB
    /// </summary>
    internal class ModelsBuilderNotificationHandler :
        INotificationHandler<ServerVariablesParsing>,
        INotificationHandler<ModelBindingError>,
        INotificationHandler<TemplateSavingNotification>
    {
        private readonly ModelsBuilderSettings _config;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IModelsBuilderDashboardProvider _modelsBuilderDashboardProvider;

        public ModelsBuilderNotificationHandler(
            IOptions<ModelsBuilderSettings> config,
            IShortStringHelper shortStringHelper,
            IModelsBuilderDashboardProvider modelsBuilderDashboardProvider)
        {
            _config = config.Value;
            _shortStringHelper = shortStringHelper;
            _modelsBuilderDashboardProvider = modelsBuilderDashboardProvider;
        }

        /// <summary>
        /// Handles the <see cref="ServerVariablesParsing"/> notification to add custom urls and MB mode
        /// </summary>
        public void Handle(ServerVariablesParsing notification)
        {
            IDictionary<string, object> serverVars = notification.ServerVariables;

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

            umbracoUrls["modelsBuilderBaseUrl"] = _modelsBuilderDashboardProvider.GetUrl();
            umbracoPlugins["modelsBuilder"] = GetModelsBuilderSettings();
        }

        private Dictionary<string, object> GetModelsBuilderSettings()
        {
            var settings = new Dictionary<string, object>
            {
                {"mode", _config.ModelsMode.ToString() }
            };

            return settings;
        }

        /// <summary>
        /// Used to check if a template is being created based on a document type, in this case we need to
        /// ensure the template markup is correct based on the model name of the document type
        /// </summary>
        public void Handle(TemplateSavingNotification notification)
        {
            if (_config.ModelsMode == ModelsMode.Nothing)
            {
                return;
            }

            // Don't do anything if we're not requested to create a template for a content type
            if (notification.CreateTemplateForContentType is false)
            {
                return;
            }

            // ensure we have the content type alias
            if (notification.ContentTypeAlias is null)
            {
                throw new InvalidOperationException("ContentTypeAlias was not found on the notification");
            }

            foreach (ITemplate template in notification.SavedEntities)
            {
                // if it is in fact a new entity (not been saved yet) and the "CreateTemplateForContentType" key
                // is found, then it means a new template is being created based on the creation of a document type
                if (!template.HasIdentity && string.IsNullOrWhiteSpace(template.Content))
                {
                    // ensure is safe and always pascal cased, per razor standard
                    // + this is how we get the default model name in Umbraco.ModelsBuilder.Umbraco.Application
                    var alias = notification.ContentTypeAlias;
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

        /// <summary>
        /// Handles when a model binding error occurs
        /// </summary>
        public void Handle(ModelBindingError notification)
        {
            ModelsBuilderAssemblyAttribute sourceAttr = notification.SourceType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();
            ModelsBuilderAssemblyAttribute modelAttr = notification.ModelType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();

            // if source or model is not a ModelsBuider type...
            if (sourceAttr == null || modelAttr == null)
            {
                // if neither are ModelsBuilder types, give up entirely
                if (sourceAttr == null && modelAttr == null)
                {
                    return;
                }

                // else report, but better not restart (loops?)
                notification.Message.Append(" The ");
                notification.Message.Append(sourceAttr == null ? "view model" : "source");
                notification.Message.Append(" is a ModelsBuilder type, but the ");
                notification.Message.Append(sourceAttr != null ? "view model" : "source");
                notification.Message.Append(" is not. The application is in an unstable state and should be restarted.");
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
                    notification.Message.Append(pureSource
                        ? " The content model is PureLive, but the view model is not."
                        : " The view model is PureLive, but the content model is not.");
                    notification.Message.Append(" The application is in an unstable state and should be restarted.");
                }
                else
                {
                    // both are pure - report, and if different versions, restart
                    // if same version... makes no sense... and better not restart (loops?)
                    Version sourceVersion = notification.SourceType.Assembly.GetName().Version;
                    Version modelVersion = notification.ModelType.Assembly.GetName().Version;
                    notification.Message.Append(" Both view and content models are PureLive, with ");
                    notification.Message.Append(sourceVersion == modelVersion
                        ? "same version. The application is in an unstable state and should be restarted."
                        : "different versions. The application is in an unstable state and should be restarted.");
                }
            }
        }
    }
}
