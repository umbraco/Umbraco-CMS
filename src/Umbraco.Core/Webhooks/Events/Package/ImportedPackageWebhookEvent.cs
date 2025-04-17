using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Webhooks.Events;

[WebhookEvent("Package Imported")]
public class ImportedPackageWebhookEvent : WebhookEventBase<ImportedPackageNotification>
{
    public ImportedPackageWebhookEvent(
        IWebhookFiringService webhookFiringService,
        IWebhookService webHookService,
        IOptionsMonitor<WebhookSettings> webhookSettings,
        IServerRoleAccessor serverRoleAccessor)
        : base(webhookFiringService, webHookService, webhookSettings, serverRoleAccessor)
    {
    }

    public override string Alias => Constants.WebhookEvents.Aliases.PackageImported;

    public override object? ConvertNotificationToRequestPayload(ImportedPackageNotification notification)
        => new
        {
            PackageName = notification.InstallationSummary.PackageName,
            InstalledEntities = new
            {
                Content = notification.InstallationSummary.ContentInstalled.Select(x => x.Key),
                Languages = notification.InstallationSummary.LanguagesInstalled.Select(x => x.Key),
                Media = notification.InstallationSummary.MediaInstalled.Select(x => x.Key),
                Scripts = notification.InstallationSummary.ScriptsInstalled.Select(x => x.Key),
                StyleSheets = notification.InstallationSummary.StylesheetsInstalled.Select(x => x.Key),
                Templates = notification.InstallationSummary.TemplatesInstalled.Select(x => x.Key),
                DataTypes = notification.InstallationSummary.DataTypesInstalled.Select(x => x.Key),
                DictionaryItems = notification.InstallationSummary.DictionaryItemsInstalled.Select(x => x.Key),
                DocumentTypes = notification.InstallationSummary.DocumentTypesInstalled.Select(x => x.Key),
                EntityContainers = notification.InstallationSummary.EntityContainersInstalled.Select(x => x.Key),
                MediaTypes = notification.InstallationSummary.MediaTypesInstalled.Select(x => x.Key),
                PartialViews = notification.InstallationSummary.PartialViewsInstalled.Select(x => x.Key),
            },
            Warnings = new
            {
                ConflictingStylesheets = notification.InstallationSummary.Warnings.ConflictingStylesheets?.Select(x => x?.Key).Where(x => x is not null) ?? [],
                ConflictingTemplates = notification.InstallationSummary.Warnings.ConflictingTemplates?.Select(x => x.Key) ?? [],
            },
        };
}
