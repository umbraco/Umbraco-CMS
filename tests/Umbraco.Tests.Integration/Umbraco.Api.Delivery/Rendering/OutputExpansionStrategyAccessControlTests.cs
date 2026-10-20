// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Api.Delivery.Rendering;

/// <summary>
///     Covers the two access guard rails applied while expanding content references in the
///     Delivery API output (see <c>ElementOnlyOutputExpansionStrategy.GetPropertyValue</c>):
///     a reference to a disallowed content type is rendered as null, and a reference to
///     public-access protected content has its property values cleared when the current
///     request has no access.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class OutputExpansionStrategyAccessControlTests : UmbracoIntegrationTest
{
    private const string TargetContentTypeAlias = "targetPage";

    private IContentTypeEditingService ContentTypeEditingService => GetRequiredService<IContentTypeEditingService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    private IApiContentResponseBuilder ApiContentResponseBuilder => GetRequiredService<IApiContentResponseBuilder>();

    private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    private IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddDeliveryApi();
    }

    public static void ConfigureDisallowTargetContentType(IUmbracoBuilder builder)
        => builder.Services.Configure<DeliveryApiSettings>(settings =>
            settings.DisallowedContentTypeAliases = new HashSet<string> { TargetContentTypeAlias });

    public static void ConfigureMemberHasAccess(IUmbracoBuilder builder)
    {
        var mock = new Mock<IRequestMemberAccessService>();
        mock
            .Setup(m => m.MemberHasAccessToAsync(It.IsAny<IPublishedContent>()))
            .Returns(Task.FromResult(PublicAccessStatus.AccessAccepted));

        builder.Services.AddUnique<IRequestMemberAccessService>(_ => mock.Object);
    }

    [TearDown]
    public void ClearHttpContext()
    {
        // The expansion strategy is resolved from the request's service provider; clearing the
        // request avoids leaking a disposed provider into subsequent fixtures via IHttpContextAccessor.
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = null;
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisallowTargetContentType))]
    public async Task ContentPicker_Disallowed_Reference_Content_Type_Is_Rendered_As_Null()
    {
        (_, IContent host) = await ArrangeContentPickerScenarioAsync();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        Assert.IsTrue(response!.Properties.ContainsKey("contentPicker"));
        Assert.IsNull(response.Properties["contentPicker"], "Reference to a disallowed content type should render as an explicit null.");
    }

    [Test]
    public async Task ContentPicker_Allowed_Reference_Content_Type_Is_Rendered()
    {
        (IContent target, IContent host) = await ArrangeContentPickerScenarioAsync();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference, "Reference to an allowed content type should be rendered.");
        Assert.AreEqual(target.Key, reference!.Id);
    }

    [Test]
    public async Task ContentPicker_Protected_Reference_Has_Properties_Cleared_When_Member_Has_No_Access()
    {
        (IContent target, IContent host) = await ArrangeContentPickerScenarioAsync();
        ProtectContent(target);

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference, "Protected content should still be rendered as a reference.");
        Assert.IsEmpty(reference!.Properties, "Property values of protected content must not leak when the request has no access.");
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureMemberHasAccess))]
    public async Task ContentPicker_Protected_Reference_Retains_Properties_When_Member_Has_Access()
    {
        (IContent target, IContent host) = await ArrangeContentPickerScenarioAsync();
        ProtectContent(target);

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference);
        Assert.IsNotEmpty(reference!.Properties, "Protected content should expose its property values when member has access.");
        Assert.IsTrue(reference.Properties.ContainsKey("title"));
    }

    [Test]
    public async Task ContentPicker_Unprotected_Reference_Retains_Properties_When_Expanded()
    {
        (_, IContent host) = await ArrangeContentPickerScenarioAsync();

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference);
        Assert.IsNotEmpty(reference!.Properties, "Unprotected content should expose its property values when expanded.");
        Assert.IsTrue(reference.Properties.ContainsKey("title"));
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureDisallowTargetContentType))]
    public async Task MultiNodeTreePicker_Disallowed_Reference_Content_Type_Is_Excluded()
    {
        (_, IContent host) = await ArrangeMultiNodeTreePickerScenarioAsync();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references, "The multi-node tree picker value should be a collection of content references.");
        Assert.IsEmpty(references!, "A reference to a disallowed content type should be excluded from the collection.");
    }

    [Test]
    public async Task MultiNodeTreePicker_Allowed_Reference_Content_Type_Is_Included()
    {
        (IContent target, IContent host) = await ArrangeMultiNodeTreePickerScenarioAsync();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references);
        Assert.AreEqual(1, references!.Length);
        Assert.AreEqual(target.Key, references[0].Id);
    }

    [Test]
    public async Task MultiNodeTreePicker_Protected_Reference_Has_Properties_Cleared_When_Member_Has_No_Access()
    {
        (IContent target, IContent host) = await ArrangeMultiNodeTreePickerScenarioAsync();
        ProtectContent(target);

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references);
        Assert.AreEqual(1, references!.Length, "Protected content should still be rendered as a reference.");
        Assert.IsEmpty(references[0].Properties, "Property values of protected content must not leak when the request has no access.");
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureMemberHasAccess))]
    public async Task MultiNodeTreePicker_Protected_Reference_Retains_Properties_When_Member_Has_Access()
    {
        (IContent target, IContent host) = await ArrangeMultiNodeTreePickerScenarioAsync();
        ProtectContent(target);

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references);
        Assert.AreEqual(1, references!.Length);
        Assert.IsNotEmpty(references[0].Properties, "Protected content should expose its property values when member has access.");
        Assert.IsTrue(references[0].Properties.ContainsKey("title"));
    }

    [Test]
    public async Task MultiNodeTreePicker_Unprotected_Reference_Retains_Properties_When_Expanded()
    {
        (_, IContent host) = await ArrangeMultiNodeTreePickerScenarioAsync();

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references);
        Assert.AreEqual(1, references!.Length);
        Assert.IsNotEmpty(references[0].Properties, "Unprotected content should expose its property values when expanded.");
        Assert.IsTrue(references[0].Properties.ContainsKey("title"));
    }

    private IApiContentResponse? BuildHostResponse(IContent host, string expand)
    {
        SetRequest(expand);

        UmbracoContextAccessor.Clear();
        IUmbracoContext umbracoContext = UmbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
        IPublishedContent? publishedHost = umbracoContext.Content?.GetById(host.Key);
        Assert.IsNotNull(publishedHost);

        return ApiContentResponseBuilder.Build(publishedHost!);
    }

    private async Task<(IContent Target, IContent Host)> ArrangeContentPickerScenarioAsync()
    {
        IContent target = await CreateAndPublishTargetAsync();
        IContent host = await CreateAndPublishHostAsync(
            ContentTypeEditingBuilder.CreateContentTypeWithContentPicker(alias: "host"),
            "contentPicker",
            target.Key);

        RefreshContentCache();

        return (target, host);
    }

    private async Task<(IContent Target, IContent Host)> ArrangeMultiNodeTreePickerScenarioAsync()
    {
        IContent target = await CreateAndPublishTargetAsync();
        IDataType pickerDataType = await CreateMultiNodeTreePickerDataTypeAsync();

        // The multi-node tree picker editor expects its value as an array of entity references.
        var pickerValue = new JsonArray
        {
            new JsonObject
            {
                ["type"] = Constants.UdiEntityType.Document,
                ["unique"] = target.Key.ToString(),
            },
        };

        var containerKey = Guid.NewGuid();
        var hostContentType = new ContentTypeCreateModel
        {
            Key = Guid.NewGuid(),
            Alias = "mntpHost",
            Name = "MNTP Host",
            AllowedAsRoot = true,
            Containers =
            [
                new ContentTypePropertyContainerModel
                {
                    Key = containerKey,
                    Name = "Content",
                    Type = "Group",
                },
            ],
            Properties =
            [
                new ContentTypePropertyTypeModel
                {
                    Key = Guid.NewGuid(),
                    Alias = "multiNodeTreePicker",
                    Name = "Multi Node Tree Picker",
                    DataTypeKey = pickerDataType.Key,
                    ContainerKey = containerKey,
                },
            ],
        };

        IContent host = await CreateAndPublishHostAsync(hostContentType, "multiNodeTreePicker", pickerValue);

        RefreshContentCache();

        return (target, host);
    }

    private async Task<IContent> CreateAndPublishTargetAsync()
    {
        var targetContentType = ContentTypeEditingBuilder.CreateSimpleContentType(alias: TargetContentTypeAlias, name: "Target Page");
        var targetContentTypeResult = await ContentTypeEditingService.CreateAsync(targetContentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(targetContentTypeResult.Success);

        var targetCreateModel = ContentEditingBuilder.CreateContentWithOneInvariantProperty(
            targetContentType.Key!.Value,
            "Target",
            "title",
            "The title value");
        var targetResult = await ContentEditingService.CreateAsync(targetCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(targetResult.Success);
        IContent target = targetResult.Result.Content!;

        var targetPublish = await ContentPublishingService.PublishAsync(
            target.Key,
            [new CulturePublishScheduleModel { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(targetPublish.Success);

        return target;
    }

    private async Task<IContent> CreateAndPublishHostAsync(ContentTypeCreateModel hostContentType, string propertyAlias, object propertyValue)
    {
        var hostContentTypeResult = await ContentTypeEditingService.CreateAsync(hostContentType, Constants.Security.SuperUserKey);
        Assert.IsTrue(hostContentTypeResult.Success);

        var hostCreateModel = ContentEditingBuilder.CreateContentWithOneInvariantProperty(
            hostContentType.Key!.Value,
            "Host",
            propertyAlias,
            propertyValue);
        var hostResult = await ContentEditingService.CreateAsync(hostCreateModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(hostResult.Success);
        IContent host = hostResult.Result.Content!;

        var hostPublish = await ContentPublishingService.PublishAsync(
            host.Key,
            [new CulturePublishScheduleModel { Culture = "*" }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(hostPublish.Success);

        return host;
    }

    private async Task<IDataType> CreateMultiNodeTreePickerDataTypeAsync()
    {
        IDataEditor editor = PropertyEditorCollection[Constants.PropertyEditors.Aliases.MultiNodeTreePicker]
                             ?? throw new InvalidOperationException("The multi-node tree picker property editor was not registered.");

        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            ConfigurationData = new Dictionary<string, object>
            {
                { "startNode", new MultiNodePickerConfigurationTreeSource { ObjectType = "content" } },
            },
            Name = "Test Multi Node Tree Picker",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        return dataType;
    }

    private void RefreshContentCache()
    {
        var refresher = GetRequiredService<ContentCacheRefresher>();
        ContentCacheRefresher.JsonPayload[] payloads =
        [
            new() { ChangeTypes = TreeChangeTypes.RefreshAll }
        ];
        refresher.RefreshInternal(payloads);
        refresher.Refresh(payloads);
    }

    private void ProtectContent(IContent content)
    {
        PublicAccessRule[] rules = [new PublicAccessRule { RuleType = "TestType", RuleValue = "TestVal" }];
        var result = PublicAccessService.Save(new PublicAccessEntry(content, content, content, rules));
        Assert.IsTrue(result.Success);
    }

    private void SetRequest(string expand)
    {
        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
                QueryString = expand.Length == 0 ? new QueryString(string.Empty) : new QueryString($"?expand={expand}"),
            },
            RequestServices = Services,
        };

        VariationContextAccessor.VariationContext = new VariationContext(culture: null);
    }
}
