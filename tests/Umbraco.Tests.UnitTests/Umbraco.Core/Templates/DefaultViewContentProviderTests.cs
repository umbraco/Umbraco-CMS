// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Templates;

/// <summary>
/// Contains unit tests for the <see cref="DefaultViewContentProvider"/> class.
/// </summary>
[TestFixture]
public class DefaultViewContentProviderTests
{
    private IDefaultViewContentProvider DefaultViewContentProvider => new DefaultViewContentProvider();

    [Test]
    public void NoOptions()
    {
        var view = DefaultViewContentProvider.GetDefaultFileContent();
        Assert.AreEqual(
            FixView(@"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
    Layout = null;
}"),
            FixView(view));
    }

    /// <summary>
    /// Verifies that <see cref="DefaultViewContentProvider.GetDefaultFileContent"/> returns the expected default layout content
    /// when provided with a layout name. Ensures the generated view includes the correct layout directive referencing the specified layout file.
    /// </summary>
    [Test]
    public void Layout()
    {
        var view = DefaultViewContentProvider.GetDefaultFileContent("Dharznoik");
        Assert.AreEqual(
            FixView(@"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
    Layout = ""Dharznoik.cshtml"";
}"),
            FixView(view));
    }

    /// <summary>
    /// Verifies that <see cref="DefaultViewContentProvider.GetDefaultFileContent"/> generates the expected view content
    /// when provided with a specific model class name.
    /// Ensures the generated view inherits from <c>UmbracoViewPage&lt;ClassName&gt;</c> and sets <c>Layout</c> to <c>null</c>.
    /// </summary>
    [Test]
    public void ClassName()
    {
        var view = DefaultViewContentProvider.GetDefaultFileContent(modelClassName: "ClassName");
        Assert.AreEqual(
            FixView(@"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ClassName>
@{
    Layout = null;
}"),
            FixView(view));
    }

    /// <summary>
    /// Tests that the default view content provider generates the expected content when a model namespace is provided.
    /// </summary>
    [Test]
    public void Namespace()
    {
        var view = DefaultViewContentProvider.GetDefaultFileContent(modelNamespace: "Models");
        Assert.AreEqual(
            FixView(@"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@{
    Layout = null;
}"),
            FixView(view));
    }

    /// <summary>
    /// Tests that the default view content includes the correct class name and namespace.
    /// </summary>
    [Test]
    public void ClassNameAndNamespace()
    {
        var view = DefaultViewContentProvider.GetDefaultFileContent(modelClassName: "ClassName", modelNamespace: "My.Models");
        Assert.AreEqual(
            FixView(@"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.ClassName>
@using ContentModels = My.Models;
@{
    Layout = null;
}"),
            FixView(view));
    }

    /// <summary>
    /// Tests that the default view content includes the correct class name, namespace, and alias.
    /// </summary>
    [Test]
    public void ClassNameAndNamespaceAndAlias()
    {
        var view = DefaultViewContentProvider.GetDefaultFileContent(
            modelClassName: "ClassName",
            modelNamespace: "My.Models",
            modelNamespaceAlias: "MyModels");
        Assert.AreEqual(
            FixView(@"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<MyModels.ClassName>
@using MyModels = My.Models;
@{
    Layout = null;
}"),
            FixView(view));
    }

    /// <summary>
    /// Tests that the default view content provider generates the expected combined view content.
    /// </summary>
    [Test]
    public void Combined()
    {
        var view = DefaultViewContentProvider.GetDefaultFileContent("Dharznoik", "ClassName", "My.Models", "MyModels");
        Assert.AreEqual(
            FixView(@"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<MyModels.ClassName>
@using MyModels = My.Models;
@{
    Layout = ""Dharznoik.cshtml"";
}"),
            FixView(view));
    }

    private static string FixView(string view)
    {
        view = view.Replace("\r\n", "\n");
        view = view.Replace("\r", "\n");
        view = view.Replace("\n", "\r\n");
        view = view.Replace("\t", "    ");
        return view;
    }
}
