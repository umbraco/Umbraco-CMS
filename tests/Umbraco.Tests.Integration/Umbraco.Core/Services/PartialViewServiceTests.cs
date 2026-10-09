// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.FileSystem;
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

    private IPartialViewFolderService PartialViewFolderService => GetRequiredService<IPartialViewFolderService>();

    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

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

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureProductionMode))]
    public async Task Cannot_Create_PartialView_In_Production_Mode()
    {
        var createModel = new PartialViewCreateModel
        {
            Name = "TestPartialView.cshtml",
            Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Test</p>"
        };

        var result = await PartialViewService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PartialViewOperationStatus.NotAllowedInProductionMode, result.Status);
        Assert.IsNull(result.Result);
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureProductionMode))]
    public async Task Cannot_Update_PartialView_In_Production_Mode()
    {
        // Create file directly via filesystem since service blocks creation in production mode
        var fileSystems = GetRequiredService<Cms.Core.IO.FileSystems>();
        var partialViewFileSystem = fileSystems.PartialViewsFileSystem!;
        const string fileName = "ExistingPartialView.cshtml";
        const string originalContent = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Original</p>";

        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(originalContent)))
        {
            partialViewFileSystem.AddFile(fileName, stream);
        }

        var updateModel = new PartialViewUpdateModel
        {
            Content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Updated</p>"
        };

        var result = await PartialViewService.UpdateAsync(fileName, updateModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PartialViewOperationStatus.NotAllowedInProductionMode, result.Status);

        // Verify the file was not changed
        using var fileStream = partialViewFileSystem.OpenFile(fileName);
        using var reader = new StreamReader(fileStream);
        var content = await reader.ReadToEndAsync();
        Assert.That(content, Does.Contain("Original"));
        Assert.That(content, Does.Not.Contain("Updated"));
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureProductionMode))]
    public async Task Cannot_Delete_PartialView_In_Production_Mode()
    {
        var fileSystems = GetRequiredService<Cms.Core.IO.FileSystems>();
        var partialViewFileSystem = fileSystems.PartialViewsFileSystem!;
        const string fileName = "PartialViewToDelete.cshtml";
        const string content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Test</p>";

        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
        {
            partialViewFileSystem.AddFile(fileName, stream);
        }

        Assert.IsTrue(partialViewFileSystem.FileExists(fileName), "File should exist before delete attempt");

        var result = await PartialViewService.DeleteAsync(fileName, Constants.Security.SuperUserKey);

        Assert.AreEqual(PartialViewOperationStatus.NotAllowedInProductionMode, result);
        Assert.IsTrue(partialViewFileSystem.FileExists(fileName), "File should still exist after blocked delete");
    }

    [Test]
    [ConfigureBuilder(ActionName = nameof(ConfigureProductionMode))]
    public async Task Cannot_Rename_PartialView_In_Production_Mode()
    {
        var fileSystems = GetRequiredService<Cms.Core.IO.FileSystems>();
        var partialViewFileSystem = fileSystems.PartialViewsFileSystem!;
        const string originalFileName = "OriginalName.cshtml";
        const string content = "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n<p>Test</p>";

        using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
        {
            partialViewFileSystem.AddFile(originalFileName, stream);
        }

        Assert.IsTrue(partialViewFileSystem.FileExists(originalFileName), "Original file should exist");

        var renameModel = new PartialViewRenameModel
        {
            Name = "RenamedFile.cshtml"
        };

        var result = await PartialViewService.RenameAsync(originalFileName, renameModel, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(PartialViewOperationStatus.NotAllowedInProductionMode, result.Status);
        Assert.IsTrue(partialViewFileSystem.FileExists(originalFileName), "Original file should still exist");
        Assert.IsFalse(partialViewFileSystem.FileExists("RenamedFile.cshtml"), "Renamed file should not exist");
    }

    [Test]
    public async Task PartialViewFolderService_GetAsync_For_Missing_Folder_Does_Not_Roll_Back_Outer_Scope()
    {
        // Regression: FolderServiceOperationBase.HandleGetAsync used to open a scope without ever
        // calling scope.Complete() when the folder didn't exist (early-return null path). When
        // called inside an outer scope, the un-completed child propagated Completed=false up to
        // the parent on dispose, silently rolling back the entire outer transaction including any
        // unrelated writes made alongside it.
        const string canaryKey = "Tests.PartialViewFolderService.OuterScopeCanary";

        using (ICoreScope outer = ScopeProvider.CreateCoreScope())
        {
            // GetAsync against a path that does not exist takes the read-miss code path.
            var result = await PartialViewFolderService.GetAsync("does-not-exist");
            Assert.IsNull(result);

            // Persist a canary in the same outer scope - this should survive scope.Complete().
            KeyValueService.SetValue(canaryKey, "ok");
            outer.Complete();
        }

        // If the outer scope was poisoned by the un-completed child, this returns null.
        Assert.AreEqual("ok", KeyValueService.GetValue(canaryKey));
    }

    [Test]
    public async Task PartialViewService_UpdateAsync_For_Missing_File_Does_Not_Roll_Back_Outer_Scope()
    {
        // Regression: FileServiceOperationBase.HandleUpdateAsync (and the other Handle* methods)
        // used to open a scope and return early without calling scope.Complete() on validation
        // failures (here: NotFound when Repository.Get returns null). When wrapped in an outer
        // scope, the un-completed child rolled the parent transaction back.
        const string canaryKey = "Tests.PartialViewService.OuterScopeCanary";

        using (ICoreScope outer = ScopeProvider.CreateCoreScope())
        {
            var result = await PartialViewService.UpdateAsync(
                "does-not-exist.cshtml",
                new PartialViewUpdateModel { Content = string.Empty },
                Constants.Security.SuperUserKey);
            Assert.AreEqual(PartialViewOperationStatus.NotFound, result.Status);

            KeyValueService.SetValue(canaryKey, "ok");
            outer.Complete();
        }

        Assert.AreEqual("ok", KeyValueService.GetValue(canaryKey));
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
