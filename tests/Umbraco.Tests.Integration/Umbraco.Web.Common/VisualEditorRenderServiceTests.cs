using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.Common;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class VisualEditorRenderServiceTests : UmbracoIntegrationTest
{
    private static readonly CapturingViewEngine Captured = new();

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IVisualEditorRenderService RenderService => GetRequiredService<IVisualEditorRenderService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        builder.Services.AddUnique<ICompositeViewEngine>(Captured);
    }

    [Test]
    public async Task RenderAsync_Passes_Overridden_Content_With_Tracking_Enabled()
    {
        Attempt<ITemplate, TemplateOperationStatus> templateAttempt = await TemplateService.CreateAsync(
            "Spike Template",
            "spikeTemplate",
            "@inherit Umbraco.Cms.Web.Common.Views.UmbracoViewPage",
            Constants.Security.SuperUserKey);
        Assert.IsTrue(templateAttempt.Success);
        ITemplate template = templateAttempt.Result;

        var contentType = new ContentTypeBuilder()
            .WithAlias("spikePage")
            .WithName("Spike Page")
            .WithDefaultTemplateId(template.Id)
            .AddAllowedTemplate().WithId(template.Id).WithAlias(template.Alias).Done()
            .AddPropertyGroup().WithName("Content").WithSupportsPublishing(true)
                .AddPropertyType()
                    .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                    .WithDataTypeId(Constants.DataTypes.Textbox)
                    .WithValueStorageType(ValueStorageType.Nvarchar)
                    .WithAlias("title").WithName("Title").Done()
                .Done()
            .Build();
        Assert.IsTrue((await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey)).Success);

        var content = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Spike Doc")
            .WithPropertyValues(new { title = "Original title" })
            .Build();
        Assert.AreEqual(template.Id, content.TemplateId, "The seeded document must carry the template id.");
        Assert.IsTrue(ContentService.Save(content).Success);
        Assert.IsTrue(ContentService.Publish(content, []).Success);

        HttpContext httpContext = GetRequiredService<IHttpContextAccessor>().HttpContext!;
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = "/";
        httpContext.Request.QueryString = new QueryString(string.Empty);
        httpContext.RequestServices = Services;

        var overrides = new[] { new VisualEditorPropertyOverride("title", "Overridden title", null, null) };

        await RenderService.RenderAsync(content.Key, null, null, overrides);

        Assert.IsNotNull(Captured.LastModel, "The render service did not pass a model to the view engine.");
        var model = Captured.LastModel as IPublishedContent;
        Assert.IsNotNull(model);
        Assert.AreEqual("Overridden title", model!.Value("title"));
        Assert.IsTrue(Captured.TrackerEnabledDuringRender, "VisualEditorPropertyTracker must be enabled while rendering.");
    }

    private sealed class CapturingViewEngine : ICompositeViewEngine
    {
        public object? LastModel { get; private set; }

        public bool TrackerEnabledDuringRender { get; private set; }

        public IReadOnlyList<IViewEngine> ViewEngines => [];

        public ViewEngineResult FindView(Microsoft.AspNetCore.Mvc.ActionContext context, string viewName, bool isMainPage)
            => ViewEngineResult.Found(viewName, new CapturingView(this));

        public ViewEngineResult GetView(string? executingFilePath, string viewPath, bool isMainPage)
            => ViewEngineResult.Found(viewPath, new CapturingView(this));

        private void Capture(ViewContext viewContext)
        {
            LastModel = viewContext.ViewData.Model;
            TrackerEnabledDuringRender = VisualEditorPropertyTracker.IsEnabled;
        }

        private sealed class CapturingView : IView
        {
            private readonly CapturingViewEngine _owner;

            public CapturingView(CapturingViewEngine owner) => _owner = owner;

            public string Path => "captured";

            public Task RenderAsync(ViewContext context)
            {
                _owner.Capture(context);
                return Task.CompletedTask;
            }
        }
    }
}
