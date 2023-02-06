// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class FileServiceTests : UmbracoIntegrationTest
{
    private IFileService FileService => GetRequiredService<IFileService>();

    [SetUp]
    public void SetUp()
    {
        var fileSystems = GetRequiredService<FileSystems>();
        var viewFileSystem = fileSystems.MvcViewsFileSystem!;
        foreach (var file in viewFileSystem.GetFiles(string.Empty).ToArray())
        {
            viewFileSystem.DeleteFile(file);
        }
    }

    [Test]
    public void Create_Template_Then_Assign_Child()
    {
        var child = FileService.CreateTemplateWithIdentity("Child", "child", "test");
        var parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");

        child.Content = "Layout = \"Parent.cshtml\";";
        FileService.SaveTemplate(child);

        child = FileService.GetTemplate(child.Id);

        Assert.AreEqual(parent.Alias, child.MasterTemplateAlias);
    }

    [Test]
    public void Create_Template_With_Child_Then_Unassign()
    {
        var parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
        var child = FileService.CreateTemplateWithIdentity("Child", "child", "Layout = \"Parent.cshtml\";");

        child = FileService.GetTemplate(child.Id);
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.MasterTemplateAlias);

        child.Content = "test";
        FileService.SaveTemplate(child);

        child = FileService.GetTemplate(child.Id);
        Assert.NotNull(child);
        Assert.AreEqual(null, child.MasterTemplateAlias);
    }

    [Test]
    public void Create_Template_With_Child_Then_Reassign()
    {
        var parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
        var parent2 = FileService.CreateTemplateWithIdentity("Parent2", "parent2", "test");
        var child = FileService.CreateTemplateWithIdentity("Child", "child", "Layout = \"Parent.cshtml\";");

        child = FileService.GetTemplate(child.Id);
        Assert.NotNull(child);
        Assert.AreEqual("parent", child.MasterTemplateAlias);

        child.Content = "Layout = \"Parent2.cshtml\";";
        FileService.SaveTemplate(child);

        child = FileService.GetTemplate(child.Id);
        Assert.NotNull(child);
        Assert.AreEqual("parent2", child.MasterTemplateAlias);
    }

    [Test]
    public void Child_Template_Paths_Are_Updated_When_Reassigning_Master()
    {
        var parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
        var parent2 = FileService.CreateTemplateWithIdentity("Parent2", "parent2", "test");
        var child = FileService.CreateTemplateWithIdentity("Child", "child", "Layout = \"Parent.cshtml\";");
        var childOfChild1 = FileService.CreateTemplateWithIdentity("Child1", "child1", "Layout = \"Child.cshtml\";");
        var childOfChild2 = FileService.CreateTemplateWithIdentity("Child2", "child2", "Layout = \"Child.cshtml\";");

        Assert.AreEqual($"child", childOfChild1.MasterTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.MasterTemplateAlias);
        Assert.AreEqual($"{parent.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);

        child.Content = "Layout = \"Parent2.cshtml\";";
        FileService.SaveTemplate(child);

        childOfChild1 = FileService.GetTemplate(childOfChild1.Id);
        Assert.NotNull(childOfChild1);

        childOfChild2 = FileService.GetTemplate(childOfChild2.Id);
        Assert.NotNull(childOfChild2);

        Assert.AreEqual($"child", childOfChild1.MasterTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild1.Id}", childOfChild1.Path);
        Assert.AreEqual($"child", childOfChild2.MasterTemplateAlias);
        Assert.AreEqual($"{parent2.Path},{child.Id},{childOfChild2.Id}", childOfChild2.Path);
    }

    [Test]
    public void Can_Query_Template_Children()
    {
        var parent = FileService.CreateTemplateWithIdentity("Parent", "parent", "test");
        var child1 = FileService.CreateTemplateWithIdentity("Child1", "child1", "Layout = \"Parent.cshtml\";");
        var child2 = FileService.CreateTemplateWithIdentity("Child2", "child2", "Layout = \"Parent.cshtml\";");

        var children = FileService.GetTemplates(parent.Id).Select(x => x.Id).ToArray();

        Assert.IsTrue(children.Contains(child1.Id));
        Assert.IsTrue(children.Contains(child2.Id));
    }

    [Test]
    public void Create_Template_With_Custom_Alias()
    {
        var template = FileService.CreateTemplateWithIdentity("Test template", "customTemplateAlias", "test");

        FileService.SaveTemplate(template);

        template = FileService.GetTemplate(template.Id);

        Assert.AreEqual("Test template", template.Name);
        Assert.AreEqual("customTemplateAlias", template.Alias);
    }
}
