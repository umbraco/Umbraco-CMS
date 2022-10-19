// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class UserExtensionsTests
{
    [SetUp]
    public void SetUp() => _userBuilder = new UserBuilder();

    private UserBuilder _userBuilder;

    [TestCase(-1, "-1", "-1,1,2,3,4,5", true)] // below root start node
    [TestCase(2, "-1,1,2", "-1,1,2,3,4,5", true)] // below start node
    [TestCase(5, "-1,1,2,3,4,5", "-1,1,2,3,4,5", true)] // at start node
    [TestCase(6, "-1,1,2,3,4,5,6", "-1,1,2,3,4,5", false)] // above start node
    [TestCase(-1, "-1", "-1,-20,1,2,3,4,5", true)] // below root start node, bin
    [TestCase(1, "-1,-20,1", "-1,-20,1,2,3,4,5", false)] // below bin start node
    public void Determines_Path_Based_Access_To_Content(int startNodeId, string startNodePath, string contentPath, bool outcome)
    {
        var user = _userBuilder
            .WithStartContentIds(new[] { startNodeId })
            .Build();

        var content = Mock.Of<IContent>(c => c.Path == contentPath && c.Id == 5);

        var esmock = new Mock<IEntityService>();
        esmock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns<UmbracoObjectTypes, int[]>((type, ids) =>
                new[] { new TreeEntityPath { Id = startNodeId, Path = startNodePath } });

        Assert.AreEqual(outcome, user.HasPathAccess(content, esmock.Object, AppCaches.Disabled));
    }

    [TestCase("", "1", "1")] // single user start, top level
    [TestCase("", "4", "4")] // single user start, deeper level
    [TestCase("", "2,3", "2,3")] // many user starts
    [TestCase("", "2,3,4", "3,4")] // many user starts, de-duplicate to deepest
    [TestCase("1", "", "1")] // single group start, top level
    [TestCase("4", "", "4")] // single group start, deeper leve
    [TestCase("2,3", "", "2,3")] // many group starts
    [TestCase("2,3,4", "", "2,3")] // many group starts, de-duplicate to upmost
    [TestCase("3", "2", "3,2")] // user and group start, combine
    [TestCase("3", "2,5", "2,5")] // user and group start, restrict
    [TestCase("3", "2,1", "2,1")] // user and group start, expand
    [TestCase("3,8", "2,6", "3,2")] // exclude bin
    [TestCase("", "6", "")] // exclude bin
    [TestCase("1,-1", "1", "1")] // was an issue
    [TestCase("-1,1", "1", "1")] // was an issue
    [TestCase("-1", "", "-1")]
    [TestCase("", "-1", "-1")]
    public void CombineStartNodes(string groupSn, string userSn, string expected)
    {
        // 1
        //  3
        //   5
        // 2
        //  4
        // bin
        //  6
        //  7
        //   8
        var paths = new Dictionary<int, string>
        {
            { 1, "-1,1" },
            { 2, "-1,2" },
            { 3, "-1,1,3" },
            { 4, "-1,2,4" },
            { 5, "-1,1,3,5" },
            { 6, "-1,-20,6" },
            { 7, "-1,-20,7" },
            { 8, "-1,-20,7,8" },
        };

        var esmock = new Mock<IEntityService>();
        esmock
            .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
            .Returns<UmbracoObjectTypes, int[]>((type, ids) =>
                paths.Where(x => ids.Contains(x.Key)).Select(x => new TreeEntityPath { Id = x.Key, Path = x.Value }));

        var comma = new[] { ',' };

        var groupSnA = groupSn.Split(comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        var userSnA = userSn.Split(comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        var combinedA = UserExtensions.CombineStartNodes(UmbracoObjectTypes.Document, groupSnA, userSnA, esmock.Object)
            .OrderBy(x => x).ToArray();
        var expectedA = expected.Split(comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x, CultureInfo.InvariantCulture)).OrderBy(x => x).ToArray();

        var ok = combinedA.Length == expectedA.Length;
        if (ok)
        {
            ok = expectedA.Where((t, i) => t != combinedA[i]).Any() == false;
        }

        if (ok == false)
        {
            Assert.Fail("Expected \"" + string.Join(",", expectedA) + "\" but got \"" + string.Join(",", combinedA) +
                        "\".");
        }
    }
}
