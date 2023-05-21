// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.AngularIntegration;

[TestFixture]
public class ServerVariablesParserTests
{
    [Test]
    public async Task Parse()
    {
        var parser = new ServerVariablesParser(Mock.Of<IEventAggregator>());

        var d = new Dictionary<string, object>
        {
            { "test1", "Test 1" },
            { "test2", "Test 2" },
            { "test3", "Test 3" },
            { "test4", "Test 4" },
            { "test5", "Test 5" },
        };

        var output = (await parser.ParseAsync(d)).StripWhitespace();

        Assert.IsTrue(output.Contains(@"Umbraco.Sys.ServerVariables = {
  ""test1"": ""Test 1"",
  ""test2"": ""Test 2"",
  ""test3"": ""Test 3"",
  ""test4"": ""Test 4"",
  ""test5"": ""Test 5""
} ;".StripWhitespace()));
    }
}
