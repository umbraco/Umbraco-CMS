// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web.WebAssets;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.AngularIntegration
{
    [TestFixture]
    public class ServerVariablesParserTests
    {
        [Test]
        public void Parse()
        {
            var d = new Dictionary<string, object>
            {
                { "test1", "Test 1" },
                { "test2", "Test 2" },
                { "test3", "Test 3" },
                { "test4", "Test 4" },
                { "test5", "Test 5" }
            };

            var output = ServerVariablesParser.Parse(d).StripWhitespace();

            Assert.IsTrue(output.Contains(@"Umbraco.Sys.ServerVariables = {
  ""test1"": ""Test 1"",
  ""test2"": ""Test 2"",
  ""test3"": ""Test 3"",
  ""test4"": ""Test 4"",
  ""test5"": ""Test 5""
} ;".StripWhitespace()));
        }
    }
}
