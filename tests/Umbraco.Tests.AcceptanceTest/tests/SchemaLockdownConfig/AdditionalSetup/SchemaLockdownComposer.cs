using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.FileSystem;

namespace UmbracoProject;

/// <summary>
/// Blocks the schema operations set in <c>SchemaLockdownTest:BlockedOperations</c> on every entity type schema
/// lockdown covers, and seeds the items the tests inspect.
/// </summary>
/// <remarks>
/// The items are seeded through the services, which schema lockdown doesn't block, so they exist even when the tests
/// can't create them through the Management API.
/// </remarks>
public sealed class SchemaLockdownComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.SchemaLockdownConfigurators().Add<SchemaLockdownTestConfigurator>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, SchemaLockdownTestDataSeeder>();
    }
}

public sealed class SchemaLockdownTestConfigurator : ISchemaLockdownConfigurator
{
    private static readonly string[] EntityTypes =
    [
        Constants.UdiEntityType.DataType,
        Constants.UdiEntityType.DictionaryItem,
        Constants.UdiEntityType.DocumentType,
        Constants.UdiEntityType.Language,
        Constants.UdiEntityType.MediaType,
        Constants.UdiEntityType.MemberGroup,
        Constants.UdiEntityType.MemberType,
        Constants.UdiEntityType.PartialView,
        Constants.UdiEntityType.Script,
        Constants.UdiEntityType.Stylesheet,
        Constants.UdiEntityType.Template,
        Constants.UdiEntityType.Webhook,
    ];

    private static readonly SchemaOperation[] BlockableOperations = [SchemaOperation.Create, SchemaOperation.Update, SchemaOperation.Delete];

    private readonly IConfiguration _configuration;

    public SchemaLockdownTestConfigurator(IConfiguration configuration) => _configuration = configuration;

    public void Configure(ISchemaRestrictionsBuilder builder)
    {
        SchemaOperation[] operations = (_configuration["SchemaLockdownTest:BlockedOperations"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseOperation)
            .ToArray();
        if (operations.Length == 0)
        {
            throw new InvalidOperationException("Set SchemaLockdownTest:BlockedOperations to a comma-separated list of Create, Update and Delete.");
        }

        foreach (var entityType in EntityTypes)
        {
            builder.Block(entityType, operations);
        }
    }

    private static SchemaOperation ParseOperation(string value)
    {
        SchemaOperation operation = BlockableOperations.FirstOrDefault(x => x.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
        return operation is SchemaOperation.Unknown
            ? throw new InvalidOperationException($"'{value}' in SchemaLockdownTest:BlockedOperations isn't Create, Update or Delete.")
            : operation;
    }
}

/// <summary>
/// Seeds one item of each entity type schema lockdown covers, plus a folder for the entity types that have folders.
/// Keep the names in step with the ones the tests use.
/// </summary>
public sealed class SchemaLockdownTestDataSeeder : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
{
    private static readonly Guid UserKey = Constants.Security.SuperUserKey;

    private readonly IServiceProvider _services;

    public SchemaLockdownTestDataSeeder(IServiceProvider services) => _services = services;

    public async Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
    {
        if (notification.RuntimeLevel != RuntimeLevel.Run)
        {
            return;
        }

        var dataTypes = Service<IDataTypeService>();
        var dataTypeContainers = Service<IDataTypeContainerService>();
        await SeedAsync(
            "LockedDataTypeFolder",
            async () => await dataTypeContainers.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0001")) is not null,
            async () => (await dataTypeContainers.CreateAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0001"), "LockedDataTypeFolder", null, UserKey)).Success);
        await SeedAsync(
            "LockedDataType",
            async () => await dataTypes.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0002")) is not null,
            async () => (await dataTypes.CreateAsync(
                new DataType(Service<PropertyEditorCollection>()[Constants.PropertyEditors.Aliases.TextBox], Service<IConfigurationEditorJsonSerializer>())
                {
                    Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0002"),
                    Name = "LockedDataType",
                    EditorUiAlias = "Umb.PropertyEditorUi.TextBox",
                },
                UserKey)).Success);

        var dictionaryItems = Service<IDictionaryItemService>();
        await SeedAsync(
            "LockedDictionaryItem",
            async () => await dictionaryItems.GetAsync("LockedDictionaryItem") is not null,
            async () => (await dictionaryItems.CreateAsync(new DictionaryItem("LockedDictionaryItem"), UserKey)).Success);

        var contentTypes = Service<IContentTypeService>();
        var contentTypeContainers = Service<IContentTypeContainerService>();
        await SeedAsync(
            "LockedDocumentTypeFolder",
            async () => await contentTypeContainers.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0d01")) is not null,
            async () => (await contentTypeContainers.CreateAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0d01"), "LockedDocumentTypeFolder", null, UserKey)).Success);
        await SeedAsync(
            "LockedDocumentType",
            async () => await contentTypes.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0d02")) is not null,
            async () => (await Service<IContentTypeEditingService>().CreateAsync(
                new ContentTypeCreateModel
                {
                    Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0d02"),
                    Name = "LockedDocumentType",
                    Alias = "lockedDocumentType",
                    Icon = "icon-document",
                    Containers = [new ContentTypePropertyContainerModel { Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0d03"), Name = "LockedGroup", Type = "Group" }],
                    Properties =
                    [
                        new ContentTypePropertyTypeModel
                        {
                            Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0d04"),
                            ContainerKey = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0d03"),
                            Name = "LockedProperty",
                            Alias = "lockedProperty",
                            DataTypeKey = Constants.DataTypes.Guids.TextstringGuid,
                        },
                    ],
                },
                UserKey)).Success);

        var languages = Service<ILanguageService>();
        await SeedAsync(
            "da-DK",
            async () => await languages.GetAsync("da-DK") is not null,
            async () => (await languages.CreateAsync(new Language("da-DK", "Danish (Denmark)"), UserKey)).Success);

        var mediaTypes = Service<IMediaTypeService>();
        var mediaTypeContainers = Service<IMediaTypeContainerService>();
        await SeedAsync(
            "LockedMediaTypeFolder",
            async () => await mediaTypeContainers.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0e01")) is not null,
            async () => (await mediaTypeContainers.CreateAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0e01"), "LockedMediaTypeFolder", null, UserKey)).Success);
        await SeedAsync(
            "LockedMediaType",
            async () => await mediaTypes.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0e02")) is not null,
            async () => (await Service<IMediaTypeEditingService>().CreateAsync(
                new MediaTypeCreateModel
                {
                    Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0e02"),
                    Name = "LockedMediaType",
                    Alias = "lockedMediaType",
                    Icon = "icon-picture",
                    Containers = [new MediaTypePropertyContainerModel { Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0e03"), Name = "LockedGroup", Type = "Group" }],
                    Properties =
                    [
                        new MediaTypePropertyTypeModel
                        {
                            Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0e04"),
                            ContainerKey = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0e03"),
                            Name = "LockedProperty",
                            Alias = "lockedProperty",
                            DataTypeKey = Constants.DataTypes.Guids.TextstringGuid,
                        },
                    ],
                },
                UserKey)).Success);

        var memberGroups = Service<IMemberGroupService>();
        await SeedAsync(
            "LockedMemberGroup",
            async () => await memberGroups.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0f01")) is not null,
            async () => (await memberGroups.CreateAsync(new MemberGroup { Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e0f01"), Name = "LockedMemberGroup" })).Success);

        var memberTypes = Service<IMemberTypeService>();
        var memberTypeContainers = Service<IMemberTypeContainerService>();
        await SeedAsync(
            "LockedMemberTypeFolder",
            async () => await memberTypeContainers.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1001")) is not null,
            async () => (await memberTypeContainers.CreateAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1001"), "LockedMemberTypeFolder", null, UserKey)).Success);
        await SeedAsync(
            "LockedMemberType",
            async () => await memberTypes.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1002")) is not null,
            async () => (await Service<IMemberTypeEditingService>().CreateAsync(
                new MemberTypeCreateModel
                {
                    Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1002"),
                    Name = "LockedMemberType",
                    Alias = "lockedMemberType",
                    Icon = "icon-user",
                    Containers = [new MemberTypePropertyContainerModel { Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1003"), Name = "LockedGroup", Type = "Group" }],
                    Properties =
                    [
                        new MemberTypePropertyTypeModel
                        {
                            Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1004"),
                            ContainerKey = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1003"),
                            Name = "LockedProperty",
                            Alias = "lockedProperty",
                            DataTypeKey = Constants.DataTypes.Guids.TextstringGuid,
                        },
                    ],
                },
                UserKey)).Success);

        var partialViews = Service<IPartialViewService>();
        var partialViewFolders = Service<IPartialViewFolderService>();
        await SeedAsync(
            "LockedPartialViewFolder",
            async () => await partialViewFolders.GetAsync("LockedPartialViewFolder") is not null,
            async () => (await partialViewFolders.CreateAsync(new PartialViewFolderCreateModel { Name = "LockedPartialViewFolder" })).Success);
        await SeedAsync(
            "LockedPartialView.cshtml",
            async () => await partialViews.GetAsync("LockedPartialView.cshtml") is not null,
            async () => (await partialViews.CreateAsync(new PartialViewCreateModel { Name = "LockedPartialView.cshtml" }, UserKey)).Success);

        var scripts = Service<IScriptService>();
        var scriptFolders = Service<IScriptFolderService>();
        await SeedAsync(
            "LockedScriptFolder",
            async () => await scriptFolders.GetAsync("LockedScriptFolder") is not null,
            async () => (await scriptFolders.CreateAsync(new ScriptFolderCreateModel { Name = "LockedScriptFolder" })).Success);
        await SeedAsync(
            "LockedScript.js",
            async () => await scripts.GetAsync("LockedScript.js") is not null,
            async () => (await scripts.CreateAsync(new ScriptCreateModel { Name = "LockedScript.js" }, UserKey)).Success);

        var stylesheets = Service<IStylesheetService>();
        var stylesheetFolders = Service<IStylesheetFolderService>();
        await SeedAsync(
            "LockedStylesheetFolder",
            async () => await stylesheetFolders.GetAsync("LockedStylesheetFolder") is not null,
            async () => (await stylesheetFolders.CreateAsync(new StylesheetFolderCreateModel { Name = "LockedStylesheetFolder" })).Success);
        await SeedAsync(
            "LockedStylesheet.css",
            async () => await stylesheets.GetAsync("LockedStylesheet.css") is not null,
            async () => (await stylesheets.CreateAsync(new StylesheetCreateModel { Name = "LockedStylesheet.css" }, UserKey)).Success);

        var templates = Service<ITemplateService>();
        await SeedAsync(
            "LockedTemplate",
            async () => await templates.GetAsync("lockedTemplate") is not null,
            async () => (await templates.CreateAsync("LockedTemplate", "lockedTemplate", null, UserKey)).Success);

        var webhooks = Service<IWebhookService>();
        await SeedAsync(
            "LockedWebhook",
            async () => await webhooks.GetAsync(new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1101")) is not null,
            async () => (await webhooks.CreateAsync(
                new Webhook("https://example.com/locked-webhook", events: [Constants.WebhookEvents.Aliases.ContentPublish])
                {
                    Key = new Guid("6f4b2f6e-2c1a-4f37-9d55-0b7a6c1e1101"),
                    Name = "LockedWebhook",
                })).Success);
    }

    private T Service<T>()
        where T : notnull
        => _services.GetRequiredService<T>();

    private static async Task SeedAsync(string name, Func<Task<bool>> exists, Func<Task<bool>> create)
    {
        if (await exists())
        {
            return;
        }

        if (await create() is false)
        {
            throw new InvalidOperationException($"Couldn't seed {name} for the schema lockdown tests.");
        }
    }
}
