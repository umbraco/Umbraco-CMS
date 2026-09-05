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

    [TearDown]
    public void TearDownTemplateFiles() => DeleteAllTemplateViewFiles();

    [Test]
    public async Task Can_Create_Template_Then_Assign_Child()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Child", "child", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.Success));
        var child = result.Result;

        result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.Success));
        var parent = result.Result;

        child.Content = "Layout = \"Parent.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.Success));

        child = await TemplateService.GetAsync(child.Key);
        Assert.That(child, Is.Not.Null);

        Assert.That(child.LayoutTemplateAlias, Is.EqualTo(parent.Alias));
    }

    [Test]
    public async Task Can_Create_Template_With_Child_Then_Unassign()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var child = result.Result;

        child = await TemplateService.GetAsync(child.Key);
        Assert.That(child, Is.Not.Null);
        Assert.That(child.LayoutTemplateAlias, Is.EqualTo("parent"));

        child.Content = "test";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        child = await TemplateService.GetAsync(child.Key);
        Assert.That(child, Is.Not.Null);
        Assert.That(child.LayoutTemplateAlias, Is.EqualTo(null));
    }

    [Test]
    public async Task Can_Create_Template_With_Child_Then_Reassign()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        result = await TemplateService.CreateAsync("Parent2", "parent2", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var child = result.Result;

        child = await TemplateService.GetAsync(child.Key);
        Assert.That(child, Is.Not.Null);
        Assert.That(child.LayoutTemplateAlias, Is.EqualTo("parent"));

        child.Content = "Layout = \"Parent2.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        child = await TemplateService.GetAsync(child.Key);
        Assert.That(child, Is.Not.Null);
        Assert.That(child.LayoutTemplateAlias, Is.EqualTo("parent2"));
    }

    [Test]
    public async Task Child_Template_Paths_Are_Updated_When_Reassigning_Layout()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Parent2", "parent2", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var parent2 = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var child = result.Result;

        result = await TemplateService.CreateAsync("Child1", "child1", "Layout = \"Child.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var childOfChild1 = result.Result;

        result = await TemplateService.CreateAsync("Child2", "child2", "Layout = \"Child.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var childOfChild2 = result.Result;

        Assert.That(childOfChild1.LayoutTemplateAlias, Is.EqualTo($"child"));
        Assert.That(childOfChild1.Path, Is.EqualTo($"{parent.Path},{child.Id},{childOfChild1.Id}"));
        Assert.That(childOfChild2.LayoutTemplateAlias, Is.EqualTo($"child"));
        Assert.That(childOfChild2.Path, Is.EqualTo($"{parent.Path},{child.Id},{childOfChild2.Id}"));

        child.Content = "Layout = \"Parent2.cshtml\";";
        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        childOfChild1 = await TemplateService.GetAsync(childOfChild1.Key);
        Assert.That(childOfChild1, Is.Not.Null);

        childOfChild2 = await TemplateService.GetAsync(childOfChild2.Key);
        Assert.That(childOfChild2, Is.Not.Null);

        Assert.That(childOfChild1.LayoutTemplateAlias, Is.EqualTo($"child"));
        Assert.That(childOfChild1.Path, Is.EqualTo($"{parent2.Path},{child.Id},{childOfChild1.Id}"));
        Assert.That(childOfChild2.LayoutTemplateAlias, Is.EqualTo($"child"));
        Assert.That(childOfChild2.Path, Is.EqualTo($"{parent2.Path},{child.Id},{childOfChild2.Id}"));
    }

    [Test]
    public async Task Can_Query_Template_Children()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child1", "child1", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var child1 = result.Result;

        result = await TemplateService.CreateAsync("Child2", "child2", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var child2 = result.Result;

        var children = await TemplateService.GetChildrenAsync(parent.Id);

        Assert.That(children.Count(), Is.EqualTo(2));
        Assert.That(children.FirstOrDefault(t => t.Id == child1.Id), Is.Not.Null);
        Assert.That(children.FirstOrDefault(t => t.Id == child2.Id), Is.Not.Null);
    }

    [Test]
    public async Task Can_Update_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var parent = result.Result;
        parent.Name = "Parent Updated";

        result = await TemplateService.UpdateAsync(parent, Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);

        parent = await TemplateService.GetAsync(parent.Key);
        Assert.That(parent, Is.Not.Null);
        Assert.That(parent.Name, Is.EqualTo("Parent Updated"));
        Assert.That(parent.Alias, Is.EqualTo("parent"));
    }

    [Test]
    public async Task Can_Delete_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var parent = result.Result;

        result = await TemplateService.DeleteAsync(parent.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        parent = await TemplateService.GetAsync(parent.Key);
        Assert.That(parent, Is.Null);
    }

    [Test]
    public async Task Layout_Template_Cannot_Be_Deleted()
    {
        Attempt<ITemplate, TemplateOperationStatus> result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var parent = result.Result;

        result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        var child = result.Result;
        Assert.That(child.LayoutTemplateAlias, Is.EqualTo("parent"));

        result = await TemplateService.DeleteAsync(parent.Key, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.LayoutTemplateCannotBeDeleted));
    }

    [Test]
    public async Task Cannot_Update_Non_Existing_Template()
    {
        var result = await TemplateService.CreateAsync("Parent", "parent", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var parent = result.Result;

        result = await TemplateService.DeleteAsync("parent", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        parent.Name = "Parent Updated";

        result = await TemplateService.UpdateAsync(parent, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.TemplateNotFound));
    }

    [Test]
    public async Task Cannot_Create_Child_Template_Without_Layout_Template()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.LayoutTemplateNotFound));
    }

    [Test]
    public async Task Cannot_Update_Child_Template_Without_Layout_Template()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var child = result.Result;
        child.Content = "Layout = \"Parent.cshtml\";";

        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.LayoutTemplateNotFound));
    }

    [Test]
    public async Task Cannot_Create_Template_With_Invalid_Alias()
    {
        var invalidAlias = new string('a', 256);
        var result = await TemplateService.CreateAsync("Child", invalidAlias, "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.InvalidAlias));
    }

    [Test]
    public async Task Cannot_Update_Template_With_Invalid_Alias()
    {
        var result = await TemplateService.CreateAsync("Child", "child", "test", Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);

        var child = result.Result;
        var invalidAlias = new string('a', 256);
        child.Alias = invalidAlias;

        result = await TemplateService.UpdateAsync(child, Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(TemplateOperationStatus.InvalidAlias));
    }

    [Test]
    public async Task Can_Create_Template_With_Key()
    {
        var key = Guid.NewGuid();
        var result = await TemplateService.CreateAsync("Template", "template", "test", Constants.Security.SuperUserKey, key);
        Assert.That(result.Success, Is.True);

        var template = await TemplateService.GetAsync(key);

        Assert.Multiple(() =>
        {
            Assert.That(template, Is.Not.Null);
            Assert.That(template.Key, Is.EqualTo(key));
        });
    }

}
