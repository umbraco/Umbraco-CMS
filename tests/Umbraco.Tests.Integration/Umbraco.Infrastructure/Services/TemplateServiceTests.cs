// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TemplateServiceTests : UmbracoIntegrationTest
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    [SetUp]
    public void SetUp() => DeleteAllTemplateViewFiles();

    [Test]
    public async Task Can_Create_Template_Then_Assign_Child()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Child", "child", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(TemplateOperationStatus.Success, result.Status);
        var child = result.Result;

        result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(TemplateOperationStatus.Success, result.Status);
        var parent = result.Result;

        child.Content = "Layout = \"Parent.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(TemplateOperationStatus.Success, result.Status);

        child = await TemplateService.GetAsync(child.Key);
        Assert.NotNull(child);

        Assert.AreEqual(parent.Alias, child.MasterTemplateAlias);
    }

    [Test]
    public async Task Can_Create_Template_With_Child_Then_Unassign()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var child = result.Result;

        child = await TemplateService.GetAsync(child.Key);
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.MasterTemplateAlias);

        child.Content = "test";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        child = await TemplateService.GetAsync(child.Key);
        Assert.NotNull(child);
        Assert.AreEqual(null, child.MasterTemplateAlias);
    }

    [Test]
    public async Task Can_Create_Template_With_Child_Then_Reassign()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        result = await TemplateService.CreateAsync("Parent2", "parent2", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var child = result.Result;

        child = await TemplateService.GetAsync(child.Key);
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.MasterTemplateAlias);

        child.Content = "Layout = \"Parent2.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        child = await TemplateService.GetAsync(child.Key);
        Assert.NotNull(child);
        Assert.AreEqual("parent2", child.MasterTemplateAlias);
    }

    [Test]
    public async Task Child_Template_Paths_Are_Updated_When_Reassigning_Master()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Parent2", "parent2", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var parent2 = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var child = result.Result;

        result = await TemplateService.CreateAsync("Child1", "child1", "Layout = \"Child.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var childOfChild1 = result.Result;

        result = await TemplateService.CreateAsync("Child2", "child2", "Layout = \"Child.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var childOfChild2 = result.Result;

        Assert.AreEqual($"child", childOfChild1.MasterTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.MasterTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);

        child.Content = "Layout = \"Parent2.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        childOfChild1 = await TemplateService.GetAsync(childOfChild1.Key);
        Assert.NotNull(childOfChild1);

        childOfChild2 = await TemplateService.GetAsync(childOfChild2.Key);
        Assert.NotNull(childOfChild2);

        Assert.AreEqual($"child", childOfChild1.MasterTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.MasterTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);
    }

    [Test]
    public async Task Can_Query_Template_Children()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child1", "child1", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var child1 = result.Result;

        result = await TemplateService.CreateAsync("Child2", "child2", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var child2 = result.Result;

        var children = await TemplateService.GetChildrenAsync(parent.Id);

        Assert.AreEqual(2, children.Count());
        Assert.NotNull(children.FirstOrDefault(t => t.Id == child1.Id));
        Assert.NotNull(children.FirstOrDefault(t => t.Id == child2.Id));
    }

    [Test]
    public async Task Can_Update_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var parent = result.Result;
        parent.Name = "Parent Updated";

        result = await TemplateService.UpdateAsync(parent, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        parent = await TemplateService.GetAsync(parent.Key);
        Assert.IsNotNull(parent);
        Assert.AreEqual("Parent Updated", parent.Name);
        Assert.AreEqual("parent", parent.Alias);
    }

    [Test]
    public async Task Can_Delete_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var parent = result.Result;

        result = await TemplateService.DeleteAsync(parent.Key, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        parent = await TemplateService.GetAsync(parent.Key);
        Assert.IsNull(parent);
    }

    [Test]
    public async Task Master_Template_Cannot_Be_Deleted()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        var child = result.Result;
        Assert.AreEqual("parent", child.MasterTemplateAlias);

        result = await TemplateService.DeleteAsync(parent.Key, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.MasterTemplateCannotBeDeleted));
    }

    [Test]
    public async Task Cannot_Update_Non_Existing_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var parent = result.Result;

        result = await TemplateService.DeleteAsync("parent", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        parent.Name = "Parent Updated";

        result = await TemplateService.UpdateAsync(parent, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.TemplateNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Child_Template_Without_Master_Template()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.MasterTemplateNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Update_Child_Template_Without_Master_Template()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var child = result.Result;
        child.Content = "Layout = \"Parent.cshtml\";";

        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.MasterTemplateNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Create_Template_With_Invalid_Alias()
    {
        var invalidAlias = new string('a', 256);
        var result = await TemplateService.CreateAsync("Child", invalidAlias, "test", Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.InvalidAlias, result.Status);
    }

    [Test]
    public async Task Cannot_Update_Template_With_Invalid_Alias()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "test", Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        var child = result.Result;
        var invalidAlias = new string('a', 256);
        child.Alias = invalidAlias;

        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(TemplateOperationStatus.InvalidAlias, result.Status);
    }

    [Test]
    public async Task Can_Create_Template_With_Key()
    {
        var key = Guid.NewGuid();
        var result = await TemplateService.CreateAsync("Template", "template", "test", Constants.Security.SuperUserKey, key);
        Assert.IsTrue(result.Success);

        var template = await TemplateService.GetAsync(key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(template);
            Assert.AreEqual(key, template.Key);
        });

    }
}
