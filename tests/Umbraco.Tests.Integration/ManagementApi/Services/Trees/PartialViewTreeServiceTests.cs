using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class PartialViewTreeServiceTests : FileSystemTreeServiceTestsBase
{
    protected override string path => Constants.SystemDirectories.PartialViews;

    [Test]
    public void Can_Get_Siblings_From_PartialView_Tree_Service()
    {
        var service = new PartialViewTreeService(_fileSystems);

        FileSystemTreeItemPresentationModel[] treeModel = service.GetSiblingsViewModels("file5", 1, 1, out long before, out var after);
        int index = Array.FindIndex(treeModel, item => item.Name == "file5");

        Assert.AreEqual(treeModel[index].Name, "file5");
        Assert.AreEqual(treeModel[index - 1].Name, "file4");
        Assert.AreEqual(treeModel[index + 1].Name, "file6");
        Assert.That(treeModel.Length == 3);
        Assert.AreEqual(after, 3);
        Assert.AreEqual(before, 4);
    }
}
