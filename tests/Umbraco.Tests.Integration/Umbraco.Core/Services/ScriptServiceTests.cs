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
    public void SetUp() => DeleteAllScriptFiles();

    [TearDown]
    public void TearDownScriptFiles() => DeleteAllScriptFiles();

    [Test]
    public async Task Can_Create_Script()
    {
        var createModel = new ScriptCreateModel
        {
            Name = "TestScript.js",
            Content = "console.log('test');"
        };

        var result = await ScriptService.CreateAsync(createModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ScriptOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("TestScript.js", result.Result.Name);
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
        Assert.IsTrue(createResult.Success);

        var updateModel = new ScriptUpdateModel
        {
            Content = "console.log('updated');"
        };

        var result = await ScriptService.UpdateAsync(createResult.Result!.Path, updateModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ScriptOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
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
        Assert.IsTrue(createResult.Success);

        var result = await ScriptService.DeleteAsync(createResult.Result!.Path, Constants.Security.SuperUserKey);

        Assert.AreEqual(ScriptOperationStatus.Success, result);

        var getResult = await ScriptService.GetAsync(createResult.Result.Path);
        Assert.IsNull(getResult);
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
        Assert.IsTrue(createResult.Success);

        var renameModel = new ScriptRenameModel
        {
            Name = "RenamedScript.js"
        };

        var result = await ScriptService.RenameAsync(createResult.Result!.Path, renameModel, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ScriptOperationStatus.Success, result.Status);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual("RenamedScript.js", result.Result.Name);
    }

    private void DeleteAllScriptFiles()
    {
        var fileSystems = GetRequiredService<Cms.Core.IO.FileSystems>();
        var scriptFileSystem = fileSystems.ScriptsFileSystem!;
        foreach (var file in scriptFileSystem.GetFiles(string.Empty).ToArray())
        {
            scriptFileSystem.DeleteFile(file);
        }
    }
}
