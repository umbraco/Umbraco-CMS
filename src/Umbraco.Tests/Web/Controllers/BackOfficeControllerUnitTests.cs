using System.Linq;
using NUnit.Framework;
using Umbraco.Web.Editors;

namespace Umbraco.Tests.Web.Controllers
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


    }
}
