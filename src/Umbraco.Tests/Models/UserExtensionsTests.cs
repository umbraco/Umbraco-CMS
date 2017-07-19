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
    }
}