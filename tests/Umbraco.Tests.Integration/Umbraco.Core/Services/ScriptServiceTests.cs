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
internal sealed class ScriptServiceTests : UmbracoIntegrationTest
{
    private IScriptService ScriptService => GetRequiredService<IScriptService>();

    [SetUp]
    public Task SetUp() => DeleteAllScriptFilesAsync();

    [TearDown]
    public Task TearDownScriptFiles() => DeleteAllScriptFilesAsync();

    [Test]
    public async Task Can_Create_Script()
    {
        var createModel = new ScriptCreateModel
        {
            Name = "TestScript.js",
            Content = "console.log('test');"
        };

        var result = await ScriptService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ScriptOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Name, Is.EqualTo("TestScript.js"));
    }

    [Test]
    public async Task Can_Update_Script()
    {
        var createModel = new ScriptCreateModel
        {
            Name = "TestScript.js",
            Content = "console.log('original');"
        };
        var createResult = await ScriptService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        var updateModel = new ScriptUpdateModel
        {
            Content = "console.log('updated');"
        };

        var result = await ScriptService.UpdateAsync(createResult.Result!.Path, updateModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ScriptOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Content, Does.Contain("updated"));
    }

    [Test]
    public async Task Can_Delete_Script()
    {
        var createModel = new ScriptCreateModel
        {
            Name = "TestScript.js",
            Content = "console.log('test');"
        };
        var createResult = await ScriptService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        var result = await ScriptService.DeleteAsync(createResult.Result!.Path, Constants.Security.SuperUserKey);

        Assert.That(result, Is.EqualTo(ScriptOperationStatus.Success));

        var getResult = await ScriptService.GetAsync(createResult.Result.Path);
        Assert.That(getResult, Is.Null);
    }

    [Test]
    public async Task Can_Rename_Script()
    {
        var createModel = new ScriptCreateModel
        {
            Name = "OriginalName.js",
            Content = "console.log('test');"
        };
        var createResult = await ScriptService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        Assert.That(createResult.Success, Is.True);

        var renameModel = new ScriptRenameModel
        {
            Name = "RenamedScript.js"
        };

        var result = await ScriptService.RenameAsync(createResult.Result!.Path, renameModel, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ScriptOperationStatus.Success));
        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result.Name, Is.EqualTo("RenamedScript.js"));
    }

    // Cleans up via the service so DeleteAsync is exercised between tests; any deletion regression
    // (notification handlers, audit, repository) surfaces as a teardown failure.
    private async Task DeleteAllScriptFilesAsync()
    {
        IEnumerable<IScript> scripts = await ScriptService.GetAllAsync();
        foreach (IScript script in scripts)
        {
            await ScriptService.DeleteAsync(script.Path, Constants.Security.SuperUserKey);
        }
    }
}
