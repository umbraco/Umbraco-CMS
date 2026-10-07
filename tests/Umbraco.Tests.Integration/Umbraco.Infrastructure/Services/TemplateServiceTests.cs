// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TemplateServiceTests : UmbracoIntegrationTest
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IFileSystem ViewsFileSystem => GetRequiredService<FileSystems>().MvcViewsFileSystem!;

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<TemplateSavingNotification, TemplateNotificationHandler>();
        builder.AddNotificationHandler<TemplateDeletingNotification, TemplateNotificationHandler>();
    }

    [SetUp]
    public void SetUp()
    {
        DeleteAllTemplateViewFiles();
        TemplateNotificationHandler.Reset();
    }

    [TearDown]
    public void TearDownTemplateFiles() => DeleteAllTemplateViewFiles();

    [Test]
    public async Task Can_Create_Template_Then_Assign_Child()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Child", "child", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(TemplateOperationStatus.Success, result.Status);
        var child = result.Result;

        result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(TemplateOperationStatus.Success, result.Status);
        var parent = result.Result;

        child.Content = "Layout = \"Parent.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(TemplateOperationStatus.Success, result.Status);

        child = await TemplateService.GetAsync(child.Key, CancellationToken.None);
        Assert.NotNull(child);

        Assert.AreEqual(parent.Alias, child.LayoutTemplateAlias);
    }

    [Test]
    public async Task Can_Create_Template_With_Child_Then_Unassign()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var child = result.Result;

        child = await TemplateService.GetAsync(child.Key, CancellationToken.None);
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.LayoutTemplateAlias);

        child.Content = "test";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        child = await TemplateService.GetAsync(child.Key, CancellationToken.None);
        Assert.NotNull(child);
        Assert.AreEqual(null, child.LayoutTemplateAlias);
    }

    [Test]
    public async Task Can_Create_Template_With_Child_Then_Reassign()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        result = await TemplateService.CreateAsync("Parent2", "parent2", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var child = result.Result;

        child = await TemplateService.GetAsync(child.Key, CancellationToken.None);
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.LayoutTemplateAlias);

        child.Content = "Layout = \"Parent2.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        child = await TemplateService.GetAsync(child.Key, CancellationToken.None);
        Assert.NotNull(child);
        Assert.AreEqual("parent2", child.LayoutTemplateAlias);
    }

    [Test]
    public async Task Child_Template_Paths_Are_Updated_When_Reassigning_Layout()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Parent2", "parent2", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var parent2 = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var child = result.Result;

        result = await TemplateService.CreateAsync("Child1", "child1", "Layout = \"Child.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var childOfChild1 = result.Result;

        result = await TemplateService.CreateAsync("Child2", "child2", "Layout = \"Child.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var childOfChild2 = result.Result;

        Assert.AreEqual($"child", childOfChild1.LayoutTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.LayoutTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);

        child.Content = "Layout = \"Parent2.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        childOfChild1 = await TemplateService.GetAsync(childOfChild1.Key, CancellationToken.None);
        Assert.NotNull(childOfChild1);

        childOfChild2 = await TemplateService.GetAsync(childOfChild2.Key, CancellationToken.None);
        Assert.NotNull(childOfChild2);

        Assert.AreEqual($"child", childOfChild1.LayoutTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.LayoutTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);
    }

    [Test]
    public async Task Child_Templates_Reference_Their_Layout_Template()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child1", "child1", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var child1 = result.Result;

        result = await TemplateService.CreateAsync("Child2", "child2", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var child2 = result.Result;

        var children = (await TemplateService.GetAllAsync(CancellationToken.None))
            .Where(template => template.LayoutTemplateAlias == parent.Alias)
            .ToArray();

        Assert.AreEqual(2, children.Count());
        Assert.NotNull(children.FirstOrDefault(t => t.Id == child1.Id));
        Assert.NotNull(children.FirstOrDefault(t => t.Id == child2.Id));
    }

    [Test]
    public async Task Can_Update_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var parent = result.Result;
        parent.Name = "Parent Updated";

        result = await TemplateService.UpdateAsync(parent, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsTrue(result.Success);

        parent = await TemplateService.GetAsync(parent.Key, CancellationToken.None);
        Assert.IsNotNull(parent);
        Assert.AreEqual("Parent Updated", parent.Name);
        Assert.AreEqual("parent", parent.Alias);
    }

    [Test]
    public async Task Can_Delete_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var parent = result.Result;

        result = await TemplateService.DeleteAsync(parent.Key, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        parent = await TemplateService.GetAsync(parent.Key, CancellationToken.None);
        Assert.IsNull(parent);
    }

    [Test]
    public async Task Layout_Template_Cannot_Be_Deleted()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        var child = result.Result;
        Assert.AreEqual("parent", child.LayoutTemplateAlias);

        result = await TemplateService.DeleteAsync(parent.Key, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.LayoutTemplateCannotBeDeleted));
    }

    [Test]
    public async Task Cannot_Update_Non_Existing_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var parent = result.Result;

        result = await TemplateService.DeleteAsync(parent.Key, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        parent.Name = "Parent Updated";

        result = await TemplateService.UpdateAsync(parent, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.TemplateNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Child_Template_Without_Layout_Template()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.LayoutTemplateNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Update_Child_Template_Without_Layout_Template()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var child = result.Result;
        child.Content = "Layout = \"Parent.cshtml\";";

        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.LayoutTemplateNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Template_With_Invalid_Alias()
    {
        var invalidAlias = new string('a', 256);
        var result = await TemplateService.CreateAsync("Child", invalidAlias, "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.InvalidAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Update_Template_With_Invalid_Alias()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var child = result.Result;
        var invalidAlias = new string('a', 256);
        child.Alias = invalidAlias;

        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.InvalidAlias, result.Status);
    }

    [Test]
    public async Task Can_Create_Template_With_Key()
    {
        var key = Guid.NewGuid();
        var result = await TemplateService.CreateAsync("Template", "template", "test", key, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var template = await TemplateService.GetAsync(key, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(template);
            Assert.AreEqual(key, template.Key);
        });
    }

    [Test]
    public async Task Create_Writes_View_File()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("View File", "viewFile", "view-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        Assert.IsTrue(ViewsFileSystem.FileExists("viewFile.cshtml"));
        Assert.AreEqual("view-content", ReadViewFile("viewFile.cshtml"));
    }

    [Test]
    public async Task Create_Uses_Content_Of_Existing_View_File()
    {
        WriteViewFile("existingView.cshtml", "existing-content");

        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Existing View", "existingView", "new-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        Assert.AreEqual("existing-content", result.Result.Content);
        Assert.AreEqual("existing-content", ReadViewFile("existingView.cshtml"));
    }

    [Test]
    public async Task Update_Writes_Changed_Content_To_View_File()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Update View", "updateView", "original-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        ITemplate template = (await TemplateService.GetAsync(result.Result.Key, CancellationToken.None))!;
        template.Content = "updated-content";
        result = await TemplateService.UpdateAsync(template, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        Assert.AreEqual("updated-content", ReadViewFile("updateView.cshtml"));
    }

    [Test]
    public async Task Update_Renames_View_File_When_Alias_Changes()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Rename View", "renameView", "rename-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        ITemplate template = (await TemplateService.GetAsync(result.Result.Key, CancellationToken.None))!;
        template.Alias = "renamedView";
        result = await TemplateService.UpdateAsync(template, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        Assert.IsFalse(ViewsFileSystem.FileExists("renameView.cshtml"));
        Assert.IsTrue(ViewsFileSystem.FileExists("renamedView.cshtml"));
        Assert.AreEqual("rename-content", ReadViewFile("renamedView.cshtml"));
    }

    [Test]
    public async Task Delete_Removes_View_File()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Delete View", "deleteView", "content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.IsTrue(ViewsFileSystem.FileExists("deleteView.cshtml"));

        Attempt<ITemplate?, TemplateOperationStatus> deleteResult = await TemplateService.DeleteAsync(result.Result.Key, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(deleteResult.Success);

        Assert.IsFalse(ViewsFileSystem.FileExists("deleteView.cshtml"));
        Assert.IsNull(await TemplateService.GetAsync(result.Result.Key, CancellationToken.None));
    }

    [Test]
    public async Task Read_Loads_Content_From_View_File()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Read View", "readView", "original-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        WriteViewFile("readView.cshtml", "changed-on-disk");

        ITemplate? byKey = await TemplateService.GetAsync(result.Result.Key, CancellationToken.None);
        ITemplate? byAlias = await TemplateService.GetAsync("readView", CancellationToken.None);
        ITemplate fromAll = (await TemplateService.GetAllAsync(CancellationToken.None)).Single(template => template.Key == result.Result.Key);
        ITemplate fromMany = (await TemplateService.GetManyAsync([result.Result.Key], CancellationToken.None)).Single();

        Assert.AreEqual("changed-on-disk", byKey!.Content);
        Assert.AreEqual("changed-on-disk", byAlias!.Content);
        Assert.AreEqual("changed-on-disk", fromAll.Content);
        Assert.AreEqual("changed-on-disk", fromMany.Content);
    }

    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    public async Task Create_Writes_View_File_Outside_Production_Mode(RuntimeMode runtimeMode)
    {
        ITemplateService templateService = CreateTemplateService(runtimeMode);

        Attempt<ITemplate, TemplateOperationStatus> result = await templateService.CreateAsync("Mode View", "modeView", "mode-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        Assert.IsTrue(ViewsFileSystem.FileExists("modeView.cshtml"));
    }

    [Test]
    public async Task Create_In_Production_Mode_Does_Not_Write_View_File()
    {
        ITemplateService templateService = CreateTemplateService(RuntimeMode.Production);

        Attempt<ITemplate, TemplateOperationStatus> result = await templateService.CreateAsync("Production View", "productionView", "content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        Assert.IsNotNull(await templateService.GetAsync(result.Result.Key, CancellationToken.None));
        Assert.IsFalse(ViewsFileSystem.FileExists("productionView.cshtml"));
    }

    [Test]
    public async Task Update_In_Production_Mode_Does_Not_Change_View_File()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Production Update", "productionUpdate", "original-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        ITemplateService productionTemplateService = CreateTemplateService(RuntimeMode.Production);
        ITemplate template = (await productionTemplateService.GetAsync(result.Result.Key, CancellationToken.None))!;
        template.Content = "modified-content";
        result = await productionTemplateService.UpdateAsync(template, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        Assert.AreEqual("original-content", ReadViewFile("productionUpdate.cshtml"));
    }

    [Test]
    public async Task Delete_In_Production_Mode_Does_Not_Remove_View_File()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Production Delete", "productionDelete", "content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        ITemplateService productionTemplateService = CreateTemplateService(RuntimeMode.Production);
        Attempt<ITemplate?, TemplateOperationStatus> deleteResult = await productionTemplateService.DeleteAsync(result.Result.Key, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(deleteResult.Success);

        Assert.IsNull(await productionTemplateService.GetAsync(result.Result.Key, CancellationToken.None));
        Assert.IsTrue(ViewsFileSystem.FileExists("productionDelete.cshtml"));
    }

    [Test]
    public async Task Templates_Read_Through_A_Content_Type_Do_Not_Load_View_Content()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Content Type View", "contentTypeView", "view-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("viewContentType", "View Content Type");
        contentType.AllowedTemplates = [result.Result];
        contentType.SetDefaultTemplate(result.Result);
        await GetRequiredService<IContentTypeService>().CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContentType? persistedContentType = await GetRequiredService<IContentTypeService>().GetAsync(contentType.Key);
        ITemplate templateViaContentType = persistedContentType!.AllowedTemplates!.Single();
        ITemplate? templateViaService = await TemplateService.GetAsync(result.Result.Key, CancellationToken.None);

        // Template content lives in the view file, which only the template service reads.
        Assert.AreEqual(result.Result.Key, templateViaContentType.Key);
        Assert.IsNull(templateViaContentType.Content);
        Assert.AreEqual("view-content", templateViaService!.Content);
    }

    [Test]
    public async Task Saving_A_Template_Read_Through_A_Content_Type_Keeps_Its_View()
    {
        ITemplate templateViaContentType = await CreateTemplateReadThroughContentType("keepView", "kept-content");

        templateViaContentType.Name = "Renamed Name";
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.UpdateAsync(templateViaContentType, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.AreEqual("kept-content", ReadViewFile("keepView.cshtml"));
        Assert.AreEqual("kept-content", (await TemplateService.GetAsync(templateViaContentType.Key, CancellationToken.None))!.Content);
    }

    [Test]
    public async Task Renaming_A_Template_Read_Through_A_Content_Type_Moves_Its_View()
    {
        ITemplate templateViaContentType = await CreateTemplateReadThroughContentType("moveView", "moved-content");

        templateViaContentType.Alias = "movedView";
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.UpdateAsync(templateViaContentType, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.IsFalse(ViewsFileSystem.FileExists("moveView.cshtml"));
        Assert.AreEqual("moved-content", ReadViewFile("movedView.cshtml"));
    }

    [Test]
    public async Task Create_Without_Content_Writes_Default_View()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Default View", "defaultView", null, null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var viewContent = ReadViewFile("defaultView.cshtml");
        Assert.That(viewContent, Does.Contain("@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage"));
        Assert.That(viewContent, Does.Contain("Layout = null;"));
        Assert.AreEqual(viewContent, result.Result.Content);
    }

    [Test]
    public async Task Create_With_Over_Long_Alias_Names_The_View_After_The_Truncated_Alias()
    {
        var longAlias = new string('a', 120);
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Long Alias", longAlias, "long-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var truncatedAlias = new string('a', 95);
        Assert.AreEqual(truncatedAlias, result.Result.Alias);
        Assert.IsFalse(ViewsFileSystem.FileExists($"{longAlias}.cshtml"));
        Assert.AreEqual("long-content", ReadViewFile($"{truncatedAlias}.cshtml"));
        Assert.AreEqual("long-content", (await TemplateService.GetAsync(truncatedAlias, CancellationToken.None))!.Content);
    }

    [Test]
    public async Task Cannot_Create_Template_With_Duplicate_Alias()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Original", "duplicate", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        result = await TemplateService.CreateAsync("Duplicate", "Duplicate", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.DuplicateAlias, result.Status);
        Assert.AreEqual(1, (await TemplateService.GetAllAsync(CancellationToken.None)).Count());
    }

    [Test]
    public async Task Cannot_Update_Template_To_Duplicate_Alias()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("First", "first", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        result = await TemplateService.CreateAsync("Second", "second", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        ITemplate second = result.Result;
        second.Alias = "first";
        result = await TemplateService.UpdateAsync(second, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.DuplicateAlias, result.Status);
        Assert.AreEqual("second", (await TemplateService.GetAsync(second.Key, CancellationToken.None))!.Alias);
    }

    [Test]
    public async Task Cannot_Update_Template_To_Create_A_Circular_Layout_Reference()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);
        ITemplate parent = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        parent.Content = "Layout = \"Child.cshtml\";";
        result = await TemplateService.UpdateAsync(parent, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.CircularLayoutTemplateReference, result.Status);
        Assert.IsNull((await TemplateService.GetAsync(parent.Key, CancellationToken.None))!.LayoutTemplateAlias);
    }

    [Test]
    public async Task Create_Cancelled_By_Notification_Does_Not_Persist_The_Template()
    {
        TemplateNotificationHandler.CancelAlias = "cancelledCreate";

        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Cancelled Create", "cancelledCreate", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.CancelledByNotification, result.Status);
        Assert.IsNull(await TemplateService.GetAsync("cancelledCreate", CancellationToken.None));
        Assert.IsFalse(ViewsFileSystem.FileExists("cancelledCreate.cshtml"));
    }

    [Test]
    public async Task Update_Cancelled_By_Notification_Does_Not_Change_The_Template()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Cancelled Update", "cancelledUpdate", "original-content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        TemplateNotificationHandler.CancelAlias = "cancelledUpdate";
        ITemplate template = (await TemplateService.GetAsync(result.Result.Key, CancellationToken.None))!;
        template.Name = "Changed Name";
        template.Content = "changed-content";
        result = await TemplateService.UpdateAsync(template, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.CancelledByNotification, result.Status);
        Assert.AreEqual("Cancelled Update", (await TemplateService.GetAsync(template.Key, CancellationToken.None))!.Name);
        Assert.AreEqual("original-content", ReadViewFile("cancelledUpdate.cshtml"));
    }

    [Test]
    public async Task Delete_Cancelled_By_Notification_Keeps_The_Template_And_Its_View()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Cancelled Delete", "cancelledDelete", "content", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        TemplateNotificationHandler.CancelAlias = "cancelledDelete";
        Attempt<ITemplate?, TemplateOperationStatus> deleteResult = await TemplateService.DeleteAsync(result.Result.Key, Constants.Security.SuperUserKey, CancellationToken.None);

        Assert.IsFalse(deleteResult.Success);
        Assert.AreEqual(TemplateOperationStatus.CancelledByNotification, deleteResult.Status);
        Assert.IsNotNull(await TemplateService.GetAsync(result.Result.Key, CancellationToken.None));
        Assert.IsTrue(ViewsFileSystem.FileExists("cancelledDelete.cshtml"));
    }

    [Test]
    public async Task Create_Publishes_Saving_Notification_Not_Flagged_For_A_Content_Type()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Plain", "plain", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        TemplateSavingNotification savingNotification = TemplateNotificationHandler.SavingNotifications.Single();
        Assert.IsFalse(savingNotification.CreateTemplateForContentType);
        Assert.AreEqual("plain", savingNotification.SavedEntities.Single().Alias);
    }

    [Test]
    public async Task Creating_A_Template_For_A_Content_Type_Assigns_It_And_Flags_The_Saving_Notification()
    {
        IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateSimpleContentType("templatedType", "Templated Type");
        await contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Attempt<Guid?, ContentTypeOperationStatus> result = await contentTypeService.CreateTemplateAsync(contentType.Key, "Templated", "templated", true, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        ITemplate? template = await TemplateService.GetAsync(result.Result!.Value, CancellationToken.None);
        Assert.IsNotNull(template);
        Assert.AreEqual("templated", template!.Alias);
        Assert.IsTrue(ViewsFileSystem.FileExists("templated.cshtml"));

        IContentType? persistedContentType = await contentTypeService.GetAsync(contentType.Key);
        Assert.That(persistedContentType!.AllowedTemplates!.Select(allowed => allowed.Key), Does.Contain(template.Key));
        Assert.AreEqual(template.Id, persistedContentType.DefaultTemplateId);

        TemplateSavingNotification savingNotification = TemplateNotificationHandler.SavingNotifications.Single();
        Assert.IsTrue(savingNotification.CreateTemplateForContentType);
        Assert.AreEqual("templatedType", savingNotification.ContentTypeAlias);
    }

    [Test]
    public async Task Creating_A_Template_For_A_Content_Type_With_A_Taken_Alias_Reports_A_Duplicate_Template_Alias()
    {
        Attempt<ITemplate, TemplateOperationStatus> existing = await TemplateService.CreateAsync("Taken", "taken", "test", null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(existing.Success);

        IContentTypeService contentTypeService = GetRequiredService<IContentTypeService>();
        var contentType = ContentTypeBuilder.CreateSimpleContentType("takenType", "Taken Type");
        await contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        Attempt<Guid?, ContentTypeOperationStatus> result = await contentTypeService.CreateTemplateAsync(contentType.Key, "Taken", "taken", true, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentTypeOperationStatus.DuplicateTemplateAlias, result.Status);
        Assert.AreEqual(1, (await TemplateService.GetAllAsync(CancellationToken.None)).Count());
    }

    private async Task<ITemplate> CreateTemplateReadThroughContentType(string alias, string content)
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync(alias, alias, content, null, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.IsTrue(result.Success);

        var contentType = ContentTypeBuilder.CreateSimpleContentType(alias + "Type", alias + " Type");
        contentType.AllowedTemplates = [result.Result];
        await GetRequiredService<IContentTypeService>().CreateAsync(contentType, Constants.Security.SuperUserKey);

        IContentType? persistedContentType = await GetRequiredService<IContentTypeService>().GetAsync(contentType.Key);
        ITemplate templateViaContentType = persistedContentType!.AllowedTemplates!.Single();
        Assert.IsNull(templateViaContentType.Content);
        return templateViaContentType;
    }

    private TemplateService CreateTemplateService(RuntimeMode runtimeMode)
    {
        var runtimeSettings = new Mock<IOptionsMonitor<RuntimeSettings>>();
        runtimeSettings.Setup(x => x.CurrentValue).Returns(new RuntimeSettings { Mode = runtimeMode });

        return new TemplateService(
            GetRequiredService<global::Umbraco.Cms.Core.Scoping.EFCore.IScopeProvider>(),
            GetRequiredService<ILoggerFactory>(),
            GetRequiredService<IEventMessagesFactory>(),
            ShortStringHelper,
            GetRequiredService<ITemplateRepository>(),
            GetRequiredService<IAuditService>(),
            GetRequiredService<ITemplateContentParserService>(),
            GetRequiredService<IViewHelper>(),
            GetRequiredService<FileSystems>(),
            runtimeSettings.Object);
    }

    private string ReadViewFile(string path)
    {
        using Stream stream = ViewsFileSystem.OpenFile(path);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        return reader.ReadToEnd();
    }

    private void WriteViewFile(string path, string content)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        ViewsFileSystem.AddFile(path, stream, true);
    }

    private sealed class TemplateNotificationHandler :
        INotificationHandler<TemplateSavingNotification>,
        INotificationHandler<TemplateDeletingNotification>
    {
        public static string? CancelAlias { get; set; }

        public static List<TemplateSavingNotification> SavingNotifications { get; } = [];

        public static void Reset()
        {
            CancelAlias = null;
            SavingNotifications.Clear();
        }

        public void Handle(TemplateSavingNotification notification)
        {
            SavingNotifications.Add(notification);
            if (notification.SavedEntities.Any(template => template.Alias == CancelAlias))
            {
                notification.Cancel = true;
            }
        }

        public void Handle(TemplateDeletingNotification notification)
        {
            if (notification.DeletedEntities.Any(template => template.Alias == CancelAlias))
            {
                notification.Cancel = true;
            }
        }
    }
}
