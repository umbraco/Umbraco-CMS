// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class StylesheetServiceTests : UmbracoIntegrationTest
{
    private IStylesheetService StylesheetService => GetRequiredService<IStylesheetService>();

    [SetUp]
    public void SetUp() => DeleteAllStylesheetFiles();

    [TearDown]
    public void TearDownStylesheetFiles() => DeleteAllStylesheetFiles();

    [Test]
    public async Task Can_Create_Stylesheet()
    {
        var createModel = new StylesheetCreateModel
        {
            Name = "TestStylesheet.css",
            Content = "body { color: red; }"
        };

        var result = await StylesheetService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(StylesheetOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("TestStylesheet.css", result.Result.Name);
    }

    [Test]
    public async Task Can_Update_Stylesheet()
    {
        var createModel = new StylesheetCreateModel
        {
            Name = "TestStylesheet.css",
            Content = "body { color: red; }"
        };
        var createResult = await StylesheetService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var updateModel = new StylesheetUpdateModel
        {
            Content = "body { color: blue; }"
        };

        var result = await StylesheetService.UpdateAsync(createResult.Result!.Path, updateModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(StylesheetOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.That(result.Result.Content, Does.Contain("blue"));
    }

    [Test]
    public async Task Can_Delete_Stylesheet()
    {
        var createModel = new StylesheetCreateModel
        {
            Name = "TestStylesheet.css",
            Content = "body { color: red; }"
        };
        var createResult = await StylesheetService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var result = await StylesheetService.DeleteAsync(createResult.Result!.Path, Constants.Security.SuperUserKey);

        Assert.AreEqual(StylesheetOperationStatus.Success, result);

        var getResult = await StylesheetService.GetAsync(createResult.Result.Path);
        Assert.IsNull(getResult);
    }

    [Test]
    public async Task Can_Rename_Stylesheet()
    {
        var createModel = new StylesheetCreateModel
        {
            Name = "OriginalName.css",
            Content = "body { color: red; }"
        };
        var createResult = await StylesheetService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.IsTrue(createResult.Success);

        var renameModel = new StylesheetRenameModel
        {
            Name = "RenamedStylesheet.css"
        };

        var result = await StylesheetService.RenameAsync(createResult.Result!.Path, renameModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(StylesheetOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("RenamedStylesheet.css", result.Result.Name);
    }

    private void DeleteAllStylesheetFiles()
    {
        var fileSystems = GetRequiredService<Cms.Core.IO.FileSystems>();
        var stylesheetFileSystem = fileSystems.StylesheetsFileSystem!;
        foreach (var file in stylesheetFileSystem.GetFiles(string.Empty).ToArray())
        {
            stylesheetFileSystem.DeleteFile(file);
        }
    }
}
