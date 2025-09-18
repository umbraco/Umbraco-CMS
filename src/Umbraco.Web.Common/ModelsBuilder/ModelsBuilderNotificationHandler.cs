using System.Reflection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Web.Common.ModelsBuilder;

/// <summary>
///     Handles <see cref="UmbracoApplicationStartingNotification" /> and <see cref="ServerVariablesParsingNotification" />
///     notifications to initialize MB
/// </summary>
internal sealed class ModelsBuilderNotificationHandler :
    INotificationHandler<ServerVariablesParsingNotification>,
    INotificationHandler<TemplateSavingNotification>
{
    private readonly ModelsBuilderSettings _config;
    private readonly IDefaultViewContentProvider _defaultViewContentProvider;
    private readonly IModelsBuilderDashboardProvider _modelsBuilderDashboardProvider;
    private readonly IShortStringHelper _shortStringHelper;

    public ModelsBuilderNotificationHandler(
        IOptions<ModelsBuilderSettings> config,
        IShortStringHelper shortStringHelper,
        IModelsBuilderDashboardProvider modelsBuilderDashboardProvider,
        IDefaultViewContentProvider defaultViewContentProvider)
    {
        _config = config.Value;
        _shortStringHelper = shortStringHelper;
        _modelsBuilderDashboardProvider = modelsBuilderDashboardProvider;
        _defaultViewContentProvider = defaultViewContentProvider;
    }

    /// <summary>
    ///     Handles the <see cref="ServerVariablesParsingNotification" /> notification to add custom urls and MB mode
    /// </summary>
    public void Handle(ServerVariablesParsingNotification notification)
    {
        IDictionary<string, object> serverVars = notification.ServerVariables;

        if (!serverVars.TryGetValue("umbracoUrls", out object? umbracoUrlsObject))
        {
            throw new ArgumentException("Missing umbracoUrls.");
        }

        if (umbracoUrlsObject == null)
        {
            throw new ArgumentException("Null umbracoUrls");
        }

        if (!(umbracoUrlsObject is Dictionary<string, object?> umbracoUrls))
        {
            throw new ArgumentException("Invalid umbracoUrls");
        }

        if (!serverVars.TryGetValue("umbracoPlugins", out object? umbracoPluginsObject))
        {
            throw new ArgumentException("Missing umbracoPlugins.");
        }

        if (!(umbracoPluginsObject is Dictionary<string, object> umbracoPlugins))
        {
            throw new ArgumentException("Invalid umbracoPlugins");
        }

        umbracoUrls["modelsBuilderBaseUrl"] = _modelsBuilderDashboardProvider.GetUrl();
        umbracoPlugins["modelsBuilder"] = GetModelsBuilderSettings();
    }

    /// <summary>
    ///     Used to check if a template is being created based on a document type, in this case we need to
    ///     ensure the template markup is correct based on the model name of the document type
    /// </summary>
    public void Handle(TemplateSavingNotification notification)
    {
        if (_config.ModelsMode == Core.Constants.ModelsBuilder.ModelsModes.Nothing)
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
                var markup = _defaultViewContentProvider.GetDefaultFileContent(
                    modelClassName: className,
                    modelNamespace: modelNamespace); /*,
                        modelNamespaceAlias: modelNamespaceAlias*/

                // set the template content to the new markup
                template.Content = markup;
            }
        }
    }

    private Dictionary<string, object> GetModelsBuilderSettings()
    {
        var settings = new Dictionary<string, object> { { "mode", _config.ModelsMode } };

        return settings;
    }
}
