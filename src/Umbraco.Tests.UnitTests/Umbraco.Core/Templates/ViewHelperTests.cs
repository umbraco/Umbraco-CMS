// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Templates
{
    [TestFixture]
    public class ViewHelperTests
    {
        [Test]
        public void NoOptions()
        {
            var view = ViewHelper.GetDefaultFileContent();
            Assert.AreEqual(
                FixView(@"@inherits UmbracoViewPage
@{
    Layout = null;
}"), FixView(view));
        }

        [Test]
        public void Layout()
        {
            var view = ViewHelper.GetDefaultFileContent(layoutPageAlias: "Master");
            Assert.AreEqual(
                FixView(@"@using UmbracoViewPage
@{
    Layout = ""Master.cshtml"";
}"), FixView(view));
        }

        [Test]
        public void ClassName()
        {
            var view = ViewHelper.GetDefaultFileContent(modelClassName: "ClassName");
            Assert.AreEqual(
                FixView(@"@inherits UmbracoViewPage<ClassName>
@{
    Layout = null;
}"), FixView(view));
        }

        [Test]
        public void Namespace()
        {
            var view = ViewHelper.GetDefaultFileContent(modelNamespace: "Models");
            Assert.AreEqual(
                FixView(@"@inherits UmbracoViewPage
@{
    Layout = null;
}"), FixView(view));
        }

        [Test]
        public void ClassNameAndNamespace()
        {
            var view = ViewHelper.GetDefaultFileContent(modelClassName: "ClassName", modelNamespace: "My.Models");
            Assert.AreEqual(
                FixView(@"@inherits UmbracoViewPage<ContentModels.ClassName>
@using ContentModels = My.Models;
@{
    Layout = null;
}"), FixView(view));
        }

        [Test]
        public void ClassNameAndNamespaceAndAlias()
        {
            var view = ViewHelper.GetDefaultFileContent(modelClassName: "ClassName", modelNamespace: "My.Models", modelNamespaceAlias: "MyModels");
            Assert.AreEqual(
                FixView(@"@inherits UmbracoViewPage<MyModels.ClassName>
@using MyModels = My.Models;
@{
    Layout = null;
}"), FixView(view));
        }

        [Test]
        public void Combined()
        {
            var view = ViewHelper.GetDefaultFileContent(layoutPageAlias: "Master", modelClassName: "ClassName", modelNamespace: "My.Models", modelNamespaceAlias: "MyModels");
            Assert.AreEqual(
                FixView(@"@inherits UmbracoViewPage<MyModels.ClassName>
@using MyModels = My.Models;
@{
    Layout = ""Master.cshtml"";
}"), FixView(view));
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
}
