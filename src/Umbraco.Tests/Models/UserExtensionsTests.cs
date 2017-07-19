using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class UserExtensionsTests
    {
        [TestCase(2, "-1,1,2", "-1,1,2,3,4,5", true)]
        [TestCase(6, "-1,1,2,3,4,5,6", "-1,1,2,3,4,5", false)]
        [TestCase(-1, "-1", "-1,1,2,3,4,5", true)]
        [TestCase(5, "-1,1,2,3,4,5", "-1,1,2,3,4,5", true)]
        [TestCase(-1, "-1", "-1,-20,1,2,3,4,5", true)]
        [TestCase(1, "-1,-20,1", "-1,-20,1,2,3,4,5", false)]
        public void Determines_Path_Based_Access_To_Content(int startNodeId, string startNodePath, string contentPath, bool outcome)
        {
            var userMock = new Mock<IUser>();
            userMock.Setup(u => u.StartContentIds).Returns(new[]{ startNodeId });
            var user = userMock.Object;
            var content = Mock.Of<IContent>(c => c.Path == contentPath && c.Id == 5);

            var entityServiceMock = new Mock<IEntityService>();
            entityServiceMock.Setup(x => x.GetAll(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns(new[]
                {
                    Mock.Of<IUmbracoEntity>(entity => entity.Id == startNodeId && entity.Path == startNodePath)
                });
            var entityService = entityServiceMock.Object;

            Assert.AreEqual(outcome, user.HasPathAccess(content, entityService));
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

        public void CombineStartNodes(string groupSn, string userSn, string expected)
        {
            // 1
            //  3
            //   5
            // 2
            //  4

            var paths = new Dictionary<int, string>
            {
                { 1, "-1, 1" },
                { 2, "-1, 2" },
                { 3, "-1, 1, 3" },
                { 4, "-1, 2, 4" },
                { 5, "-1, 1, 3, 5" },
            };
            var esmock = new Mock<IEntityService>();
            esmock
                .Setup(x => x.GetAllPaths(It.IsAny<UmbracoObjectTypes>(), It.IsAny<int[]>()))
                .Returns<UmbracoObjectTypes, int[]>((type, ids) => paths.Where(x => ids.Contains(x.Key)).Select(x => new EntityPath { Id = x.Key, Path = x.Value }));

            var comma = new[] { ',' };

            var groupSnA = groupSn.Split(comma, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            var userSnA = userSn.Split(comma, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            var combinedA = UserExtensions.CombineStartNodes(UmbracoObjectTypes.Document, groupSnA, userSnA, esmock.Object).OrderBy(x => x).ToArray();
            var expectedA = expected.Split(comma, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).OrderBy(x => x).ToArray();

            var ok = combinedA.Length == expectedA.Length;
            if (ok) ok = expectedA.Where((t, i) => t != combinedA[i]).Any() == false;

            if (ok == false)
                Assert.Fail("Expected \"" + string.Join(",", expectedA) + "\" but got \"" + string.Join(",", combinedA) + "\".");
        }
    }
}