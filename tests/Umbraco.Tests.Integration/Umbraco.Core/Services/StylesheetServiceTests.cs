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
    public Task SetUp() => DeleteAllStylesheetFilesAsync();

    [TearDown]
    public Task TearDownStylesheetFiles() => DeleteAllStylesheetFilesAsync();

    [Test]
    public async Task Can_Create_Stylesheet()
    {
        var createModel = new StylesheetCreateModel
        {
            Name = "TestStylesheet.css",
            Content = "body { color: red; }"
        };

        var result = await StylesheetService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(StylesheetOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Name, Is.EqualTo("TestStylesheet.css"));
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
        Assert.That(createResult.Success, Is.True);

        var updateModel = new StylesheetUpdateModel
        {
            Content = "body { color: blue; }"
        };

        var result = await StylesheetService.UpdateAsync(createResult.Result!.Path, updateModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(StylesheetOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
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
        Assert.That(createResult.Success, Is.True);

        var result = await StylesheetService.DeleteAsync(createResult.Result!.Path, Constants.Security.SuperUserKey);

        Assert.That(result, Is.EqualTo(StylesheetOperationStatus.Success));

        var getResult = await StylesheetService.GetAsync(createResult.Result.Path);
        Assert.That(getResult, Is.Null);
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
        Assert.That(createResult.Success, Is.True);

        var renameModel = new StylesheetRenameModel
        {
            Name = "RenamedStylesheet.css"
        };

        var result = await StylesheetService.RenameAsync(createResult.Result!.Path, renameModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(StylesheetOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Name, Is.EqualTo("RenamedStylesheet.css"));
    }

    // Cleans up via the service so DeleteAsync is exercised between tests; any deletion regression
    // (notification handlers, audit, repository) surfaces as a teardown failure.
    private async Task DeleteAllStylesheetFilesAsync()
    {
        IEnumerable<IStylesheet> stylesheets = await StylesheetService.GetAllAsync();
        foreach (IStylesheet stylesheet in stylesheets)
        {
            await StylesheetService.DeleteAsync(stylesheet.Path, Constants.Security.SuperUserKey);
        }
    }
}
