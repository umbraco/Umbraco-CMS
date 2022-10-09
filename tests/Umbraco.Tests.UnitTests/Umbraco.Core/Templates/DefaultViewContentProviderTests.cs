// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Templates;

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
