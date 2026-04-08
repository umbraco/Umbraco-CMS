// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Attributes;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PartialViewServiceTests : UmbracoIntegrationTest
{
    private IPartialViewService PartialViewService => GetRequiredService<IPartialViewService>();

    /// <summary>
    /// Configures the runtime mode to Production for tests decorated with [ConfigureBuilder].
    /// </summary>
    public static void ConfigureProductionMode(IUmbracoBuilder builder)
    {
        builder.Services.Configure<RuntimeSettings>(settings => settings.Mode = RuntimeMode.Production);
    }

    [SetUp]
    public void SetUp() => DeleteAllPartialViewFiles();

    [TearDown]
    public void TearDownPartialViewFiles() => DeleteAllPartialViewFiles();

    [Test]
    public async Task Can_Create_PartialView()
    {
        var createModel = new PartialViewCreateModel
        {
            Name = "TestPartialView.cshtml",
            Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Test</p>"
        };

        var result = await PartialViewService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(PartialViewOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("TestPartialView.cshtml", result.Result.Name);
    }

    [Test]
    public async Task Can_Update_PartialView()
    {
        var createModel = new PartialViewCreateModel
        {
            Name = "TestPartialView.cshtml",
            Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Original</p>"
        };
        var createResult = await PartialViewService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var updateModel = new PartialViewUpdateModel
        {
            Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Updated</p>"
        };

        var result = await PartialViewService.UpdateAsync(createResult.Result!.Path, updateModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(PartialViewOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.That(result.Result.Content, Does.Contain("Updated"));
    }

    [Test]
    public async Task Can_Delete_PartialView()
    {
        var createModel = new PartialViewCreateModel
        {
            Name = "TestPartialView.cshtml",
            Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Test</p>"
        };
        var createResult = await PartialViewService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var result = await PartialViewService.DeleteAsync(createResult.Result!.Path, Constants.Security.SuperUserKey);

        Assert.AreEqual(PartialViewOperationStatus.Success, result);

        var getResult = await PartialViewService.GetAsync(createResult.Result.Path);
        Assert.IsNull(getResult);
    }

    [Test]
    public async Task Can_Rename_PartialView()
    {
        var createModel = new PartialViewCreateModel
        {
            Name = "OriginalName.cshtml",
            Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Test</p>"
        };
        var createResult = await PartialViewService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var renameModel = new PartialViewRenameModel
        {
            Name = "RenamedPartialView.cshtml"
        };

        var result = await PartialViewService.RenameAsync(createResult.Result!.Path, renameModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(PartialViewOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("RenamedPartialView.cshtml", result.Result.Name);
    }

    private void DeleteAllPartialViewFiles()
    {
        var fileSystems = GetRequiredService<Cms.Core.IO.FileSystems>();
        var partialViewFileSystem = fileSystems.PartialViewsFileSystem!;
        foreach (var file in partialViewFileSystem.GetFiles(string.Empty).ToArray())
        {
            partialViewFileSystem.DeleteFile(file);
        }
    }
}
