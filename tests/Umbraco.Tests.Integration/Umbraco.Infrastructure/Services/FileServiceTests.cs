// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class FileServiceTests : UmbracoIntegrationTest
{
    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();
    private IUserService UserService => GetRequiredService<IUserService>();

    private Guid _userKey;

    [SetUp]
    public void SetUp()
    {
        _userKey = UserService.GetUserById(-1).Key;
    }

    [Test]
    public void Create_Template_Then_Assign_Child()
    {
        var attempt = TemplateService.CreateAsync("Child", "child", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child template");
        }

        ITemplate child = attempt.Result;

        attempt = TemplateService.CreateAsync("Parent", "parent", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create parent template");
        }

        ITemplate parent = attempt.Result;

        child.Content = "Layout = \"Parent.cshtml\";";

        WaitBeforeUpdate();
        attempt = TemplateService.UpdateAsync(child, _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to update template");
        }

        child = TemplateService.GetAsync(child.Id).Result;

        Assert.AreEqual(parent.Alias, child.MasterTemplateAlias);
    }

    [Test]
    public void Create_Template_With_Child_Then_Unassign()
    {
        var attempt = TemplateService.CreateAsync("Parent", "parent", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create parent template");
        }

        ITemplate parent = attempt.Result;

        attempt = TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child template");
        }

        ITemplate child = attempt.Result;

        child = TemplateService.GetAsync(child.Id).Result;
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.MasterTemplateAlias);

        child.Content = "test";

        WaitBeforeUpdate();
        attempt = TemplateService.UpdateAsync(child, _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to update template");
        }

        child = TemplateService.GetAsync(child.Id).Result;
        Assert.NotNull(parent);
        Assert.NotNull(child);
        Assert.AreEqual(null, child.MasterTemplateAlias);
    }

    [Test]
    public void Create_Template_With_Child_Then_Reassign()
    {
        var attempt = TemplateService.CreateAsync("Parent", "parent", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create parent template");
        }

        ITemplate parent = attempt.Result;

        attempt = TemplateService.CreateAsync("Parent2", "parent2", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create parent2 template");
        }

        ITemplate parent2 = attempt.Result;

        attempt = TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child template");
        }

        ITemplate child = attempt.Result;

        child = TemplateService.GetAsync(child.Id).Result;
        Assert.NotNull(parent);
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.MasterTemplateAlias);

        child.Content = "Layout = \"Parent2.cshtml\";";

        WaitBeforeUpdate();
        attempt = TemplateService.UpdateAsync(child, _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to update template");
        }

        child = TemplateService.GetAsync(child.Id).Result;
        Assert.NotNull(parent2);
        Assert.NotNull(child);
        Assert.AreEqual("parent2", child.MasterTemplateAlias);
    }

    [Test]
    public void Child_Template_Paths_Are_Updated_When_Reassigning_Master()
    {
        var attempt = TemplateService.CreateAsync("Parent", "parent", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to parent child template");
        }

        ITemplate parent = attempt.Result;

        attempt = TemplateService.CreateAsync("Parent2", "parent2", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to parent2 child template");
        }

        ITemplate parent2 = attempt.Result;

        attempt = TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child template");
        }

        ITemplate child = attempt.Result;

        attempt = TemplateService.CreateAsync("Child1", "child1", "Layout = \"Child.cshtml\";", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child1 template");
        }

        ITemplate childOfChild1 = attempt.Result;

        attempt = TemplateService.CreateAsync("Child2", "child2", "Layout = \"Child.cshtml\";", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child2 template");
        }

        ITemplate childOfChild2 = attempt.Result;

        Assert.AreEqual($"child", childOfChild1.MasterTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.MasterTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);

        child.Content = "Layout = \"Parent2.cshtml\";";

        WaitBeforeUpdate();
        attempt = TemplateService.UpdateAsync(child, _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to update template");
        }

        childOfChild1 = TemplateService.GetAsync(childOfChild1.Id).Result;
        Assert.NotNull(childOfChild1);

        childOfChild2 = TemplateService.GetAsync(childOfChild2.Id).Result;
        Assert.NotNull(childOfChild2);

        Assert.AreEqual($"child", childOfChild1.MasterTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.MasterTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);
    }

    [Test]
    public void Can_Query_Template_Children()
    {
        var attempt = TemplateService.CreateAsync("Parent", "parent", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create parent template");
        }

        ITemplate parent = attempt.Result;

        attempt = TemplateService.CreateAsync("Child", "child", "Layout = \"Parent.cshtml\";", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child template");
        }

        ITemplate child1 = attempt.Result;
        attempt = TemplateService.CreateAsync("Child2", "child2", "Layout = \"Parent.cshtml\";", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create child template");
        }

        ITemplate child2 = attempt.Result;

        var childrenIds = TemplateService.GetChildrenAsync(parent.Id).Result.Select(x => x.Id).ToArray();

        Assert.IsTrue(childrenIds.Contains(child1.Id));
        Assert.IsTrue(childrenIds.Contains(child2.Id));
    }

    [Test]
    public void Create_Template_With_Custom_Alias()
    {
        var attempt = TemplateService.CreateAsync("Test template", "customTemplateAlias", "test", _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to create parent template");
        }

        ITemplate template = attempt.Result;

        WaitBeforeUpdate();
        attempt = TemplateService.UpdateAsync(template, _userKey).Result;
        if (!attempt.Success)
        {
            Assert.Fail(attempt.Exception?.Message ?? "Failed to update template");
        }

        template = TemplateService.GetAsync(template.Id).Result;

        Assert.AreEqual("Test template", template.Name);
        Assert.AreEqual("customTemplateAlias", template.Alias);
    }

    private void WaitBeforeUpdate()
    {
        Thread.Sleep(200); // Wait a bit to ensure the file system is ready for updates
    }
}
