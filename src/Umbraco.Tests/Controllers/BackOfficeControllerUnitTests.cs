using System.Linq;
using NUnit.Framework;
using Umbraco.Web.Editors;

namespace Umbraco.Tests.Controllers
{
    [TestFixture]
    public class BackOfficeControllerUnitTests
    {
        public static object[] TestLegacyJsActionPaths = new object[] {
            new string[]
            {
                "alert('hello');",
                "function test() { window.location = 'http://www.google.com'; }",
                "function openCourierSecurity(userid){ UmbClientMgr.contentFrame('page?userid=123); }",
                @"function openRepository(repo, folder){ UmbClientMgr.contentFrame('page?repo=repo&folder=folder); }
                  function openTransfer(revision, repo, folder){ UmbClientMgr.contentFrame('page?revision=revision&repo=repo&folder=folder); }",
                "umbraco/js/test.js",
                "/umbraco/js/test.js",
                "~/umbraco/js/test.js"
            }
        };


        [TestCaseSource("TestLegacyJsActionPaths")]
        public void Separates_Legacy_JsActions_By_Block_Or_Url(object[] jsActions)
        {
            var jsBlocks =
                BackOfficeController.GetLegacyActionJsForActions(BackOfficeController.LegacyJsActionType.JsBlock,
                    jsActions.Select(n => n.ToString()));

            var jsUrls =
                BackOfficeController.GetLegacyActionJsForActions(BackOfficeController.LegacyJsActionType.JsUrl,
                    jsActions.Select(n => n.ToString()));

            Assert.That(jsBlocks.Count() == 4);
            Assert.That(jsUrls.Count() == 3);
            Assert.That(jsUrls.Last().StartsWith("~/") == false);
        }
    }
}