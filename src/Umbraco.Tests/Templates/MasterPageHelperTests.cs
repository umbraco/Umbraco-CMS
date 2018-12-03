using NUnit.Framework;
using Umbraco.Core.IO;

namespace Umbraco.Tests.Templates
{
    [TestFixture]
    public class MasterPageHelperTests
    {

        [TestCase(@"<%@ master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
        [TestCase(@"<%@ Master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
        [TestCase(@"<%@Master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
        [TestCase(@"<%@  Master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
        [TestCase(@"<%@master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
        public void IsMasterPageSyntax(string design)
        {
            Assert.IsTrue(MasterPageHelper.IsMasterPageSyntax(design));
        }

    }
}
