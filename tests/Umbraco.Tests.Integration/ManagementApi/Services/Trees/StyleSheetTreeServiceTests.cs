using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Services.Trees;

public class StyleSheetTreeServiceTests : FileSystemTreeServiceTestsBase
{
    protected override string path => GlobalSettings.UmbracoCssPath;

    [Test]
    public void Can_Get_Siblings_From_StyleSheet_Tree_Service()
    {
        var service = new StyleSheetTreeService(_fileSystems);

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
