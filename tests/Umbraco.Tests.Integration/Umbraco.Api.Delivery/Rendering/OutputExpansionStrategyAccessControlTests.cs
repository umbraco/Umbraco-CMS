// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
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

    private IApiContentResponseBuilder ApiContentResponseBuilder => GetRequiredService<IApiContentResponseBuilder>();

    private IPublicAccessService PublicAccessService => GetRequiredService<IPublicAccessService>();

    private IVariationContextAccessor VariationContextAccessor => GetRequiredService<IVariationContextAccessor>();

    private IUmbracoContextAccessor UmbracoContextAccessor => GetRequiredService<IUmbracoContextAccessor>();

    private IUmbracoContextFactory UmbracoContextFactory => GetRequiredService<IUmbracoContextFactory>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IFileService FileService => GetRequiredService<IFileService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditorCollection => GetRequiredService<PropertyEditorCollection>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer => GetRequiredService<IConfigurationEditorJsonSerializer>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNuCache();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.AddDeliveryApi();

        if (TestContext.CurrentContext.Test.Name is (nameof(ContentPicker_Disallowed_Reference_Content_Type_Is_Rendered_As_Null) or nameof(MultiNodeTreePicker_Disallowed_Reference_Content_Type_Is_Excluded)))
        {
            builder.Services.Configure<DeliveryApiSettings>(settings => settings.DisallowedContentTypeAliases = [TargetContentTypeAlias]);
        }

        if (TestContext.CurrentContext.Test.Name is (nameof(ContentPicker_Protected_Reference_Retains_Properties_When_Member_Has_Access) or nameof(MultiNodeTreePicker_Protected_Reference_Retains_Properties_When_Member_Has_Access)))
        {
            var mock = new Mock<IRequestMemberAccessService>();
            mock
                .Setup(m => m.MemberHasAccessToAsync(It.IsAny<IPublishedContent>()))
                .Returns(Task.FromResult(PublicAccessStatus.AccessAccepted));

            builder.Services.AddUnique<IRequestMemberAccessService>(_ => mock.Object);
        }
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
    public void ContentPicker_Disallowed_Reference_Content_Type_Is_Rendered_As_Null()
    {
        (_, IContent host) = ArrangeContentPickerScenario();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        Assert.IsTrue(response!.Properties.ContainsKey("contentPicker"));
        Assert.IsNull(response.Properties["contentPicker"], "Reference to a disallowed content type should render as an explicit null.");
    }

    [Test]
    public void ContentPicker_Allowed_Reference_Content_Type_Is_Rendered()
    {
        (IContent target, IContent host) = ArrangeContentPickerScenario();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference, "Reference to an allowed content type should be rendered.");
        Assert.AreEqual(target.Key, reference!.Id);
    }

    [Test]
    public void ContentPicker_Protected_Reference_Has_Properties_Cleared_When_Member_Has_No_Access()
    {
        (IContent target, IContent host) = ArrangeContentPickerScenario();
        ProtectContent(target);

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference, "Protected content should still be rendered as a reference.");
        Assert.IsEmpty(reference!.Properties, "Property values of protected content must not leak when the request has no access.");
    }

    [Test]
    public void ContentPicker_Protected_Reference_Retains_Properties_When_Member_Has_Access()
    {
        (IContent target, IContent host) = ArrangeContentPickerScenario();
        ProtectContent(target);

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference);
        Assert.IsNotEmpty(reference!.Properties, "Protected content should expose its property values when member has access.");
        Assert.IsTrue(reference.Properties.ContainsKey("title"));
    }

    [Test]
    public void ContentPicker_Unprotected_Reference_Retains_Properties_When_Expanded()
    {
        (_, IContent host) = ArrangeContentPickerScenario();

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var reference = response!.Properties["contentPicker"] as IApiContent;
        Assert.IsNotNull(reference);
        Assert.IsNotEmpty(reference!.Properties, "Unprotected content should expose its property values when expanded.");
        Assert.IsTrue(reference.Properties.ContainsKey("title"));
    }

    [Test]
    public void MultiNodeTreePicker_Disallowed_Reference_Content_Type_Is_Excluded()
    {
        (_, IContent host) = ArrangeMultiNodeTreePickerScenario();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references, "The multi-node tree picker value should be a collection of content references.");
        Assert.IsEmpty(references!, "A reference to a disallowed content type should be excluded from the collection.");
    }

    [Test]
    public void MultiNodeTreePicker_Allowed_Reference_Content_Type_Is_Included()
    {
        (IContent target, IContent host) = ArrangeMultiNodeTreePickerScenario();

        IApiContentResponse? response = BuildHostResponse(host, expand: string.Empty);

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references);
        Assert.AreEqual(1, references!.Length);
        Assert.AreEqual(target.Key, references[0].Id);
    }

    [Test]
    public void MultiNodeTreePicker_Protected_Reference_Has_Properties_Cleared_When_Member_Has_No_Access()
    {
        (IContent target, IContent host) = ArrangeMultiNodeTreePickerScenario();
        ProtectContent(target);

        IApiContentResponse? response = BuildHostResponse(host, expand: "properties[$all]");

        Assert.IsNotNull(response);
        var references = (response!.Properties["multiNodeTreePicker"] as IEnumerable<IApiContent>)?.ToArray();
        Assert.IsNotNull(references);
        Assert.AreEqual(1, references!.Length, "Protected content should still be rendered as a reference.");
        Assert.IsEmpty(references[0].Properties, "Property values of protected content must not leak when the request has no access.");
    }

    [Test]
    public void MultiNodeTreePicker_Protected_Reference_Retains_Properties_When_Member_Has_Access()
    {
        (IContent target, IContent host) = ArrangeMultiNodeTreePickerScenario();
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
    public void MultiNodeTreePicker_Unprotected_Reference_Retains_Properties_When_Expanded()
    {
        (_, IContent host) = ArrangeMultiNodeTreePickerScenario();

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

    private (IContent Target, IContent Host) ArrangeContentPickerScenario()
    {
        var target = CreateAndPublishTarget();
        var host = CreateAndPublishHost(
            ContentTypeBuilder.CreateAllTypesContentType("host", "Host"),
            "contentPicker",
            new GuidUdi(Constants.UdiEntityType.Document, target.Key));

        RefreshContentCache();

        return (target, host);
    }

    private (IContent Target, IContent Host) ArrangeMultiNodeTreePickerScenario()
    {
        var target = CreateAndPublishTarget();
        IDataType pickerDataType = CreateMultiNodeTreePickerDataType();

        var hostContentType = new ContentTypeBuilder()
            .WithAlias("mntpHost")
            .WithName("MNTP Host")
            .AddPropertyType()
                .WithValueStorageType(ValueStorageType.Ntext)
                .WithDataTypeId(pickerDataType.Id)
                .WithAlias("multiNodeTreePicker")
                .WithName("Multi Node Tree Picker")
            .Done()
            .Build();

        var host = CreateAndPublishHost(
            hostContentType,
            "multiNodeTreePicker",
            new GuidUdi(Constants.UdiEntityType.Document, target.Key));

        RefreshContentCache();

        return (target, host);
    }

    private IContent CreateAndPublishTarget()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var targetContentType = ContentTypeBuilder.CreateSimpleContentType(alias: TargetContentTypeAlias, defaultTemplateId: template.Id);
        ContentTypeService.Save(targetContentType);

        var targetCreateModel = ContentBuilder.CreateSimpleContent(targetContentType);
        var targetResult = ContentService.SaveAndPublish(targetCreateModel);
        Assert.IsTrue(targetResult.Success);
        Assert.IsNotNull(targetResult.Content);

        return targetResult.Content;
    }

    private IContent CreateAndPublishHost(IContentType hostContentType, string propertyAlias, object propertyValue)
    {
        ContentTypeService.Save(hostContentType);

        var hostCreateModel = new ContentBuilder()
            .WithName("Host")
            .WithContentType(hostContentType)
            .Build();
        hostCreateModel.SetValue(propertyAlias, propertyValue);

        var hostResult = ContentService.SaveAndPublish(hostCreateModel);
        Assert.IsTrue(hostResult.Success);
        Assert.IsNotNull(hostResult.Content);

        return hostResult.Content;
    }

    private IDataType CreateMultiNodeTreePickerDataType()
    {
        IDataEditor editor = PropertyEditorCollection[Constants.PropertyEditors.Aliases.MultiNodeTreePicker]
                             ?? throw new InvalidOperationException("The multi-node tree picker property editor was not registered.");

        var dataType = new DataType(editor, ConfigurationEditorJsonSerializer)
        {
            Configuration = new MultiNodePickerConfiguration
            {
                TreeSource = new MultiNodePickerConfigurationTreeSource { ObjectType = "content" },
            },
            Name = "Test Multi Node Tree Picker",
            DatabaseType = ValueStorageType.Ntext,
            ParentId = Constants.System.Root,
            CreateDate = DateTime.UtcNow,
        };

        DataTypeService.Save(dataType);

        GetRequiredService<IPublishedContentTypeFactory>().NotifyDataTypeChanges();

        return dataType;
    }

    private void RefreshContentCache()
    {
        var refresher = GetRequiredService<ContentCacheRefresher>();
        ContentCacheRefresher.JsonPayload[] payloads =
        [
            new() { ChangeTypes = TreeChangeTypes.RefreshAll }
        ];
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
