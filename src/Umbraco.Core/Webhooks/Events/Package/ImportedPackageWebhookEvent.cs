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
                ContentIds = notification.InstallationSummary.ContentInstalled.Select(x => x.Key),
                LanguagesIds = notification.InstallationSummary.LanguagesInstalled.Select(x => x.Key),
                MediaIds = notification.InstallationSummary.MediaInstalled.Select(x => x.Key),
                ScriptsIds = notification.InstallationSummary.ScriptsInstalled.Select(x => x.Key),
                StyleSheetsIds = notification.InstallationSummary.StylesheetsInstalled.Select(x => x.Key),
                TemplatesIds = notification.InstallationSummary.TemplatesInstalled.Select(x => x.Key),
                DataTypesIds = notification.InstallationSummary.DataTypesInstalled.Select(x => x.Key),
                DictionaryItemsIds = notification.InstallationSummary.DictionaryItemsInstalled.Select(x => x.Key),
                DocumentTypesIds = notification.InstallationSummary.DocumentTypesInstalled.Select(x => x.Key),
                EntityContainersIds = notification.InstallationSummary.EntityContainersInstalled.Select(x => x.Key),
                MediaTypesIds = notification.InstallationSummary.MediaTypesInstalled.Select(x => x.Key),
                PartialViewsIds = notification.InstallationSummary.PartialViewsInstalled.Select(x => x.Key),
            },
            Warnings = new
            {
                ConflictingStylesheetsIds = notification.InstallationSummary.Warnings.ConflictingStylesheets?.Select(x => x?.Key).Where(x => x is not null) ?? [],
                ConflictingTemplatesIds = notification.InstallationSummary.Warnings.ConflictingTemplates?.Select(x => x.Key) ?? [],
            },
        };
}
