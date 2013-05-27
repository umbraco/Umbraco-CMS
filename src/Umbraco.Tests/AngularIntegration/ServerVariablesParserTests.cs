using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Web.UI.JavaScript;

namespace Umbraco.Tests.AngularIntegration
{
    [TestFixture]
    public class ServerVariablesParserTests
    {
       

        [Test]
        public void Parse()
        {
            var d = new Dictionary<string, object>();
            d.Add("test1", "Test 1");
            d.Add("test2", "Test 2");
            d.Add("test3", "Test 3");
            d.Add("test4", "Test 4");
            d.Add("test5", "Test 5");

            var output = ServerVariablesParser.Parse(d);

            Assert.IsTrue(output.Contains(@"Umbraco.Sys.ServerVariables = {
  ""test1"": ""Test 1"",
  ""test2"": ""Test 2"",
  ""test3"": ""Test 3"",
  ""test4"": ""Test 4"",
  ""test5"": ""Test 5""
} ;"));
        }
    }
}