using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Unversion;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class UnversionServiceTests
    {
        [Test]
        public void GetVersions_Returns_Right_Based_On_Date()
        {

            var config = new UnversionConfigEntry() { MaxDays = 10 };

            List<IContent> versions = new List<IContent>()
            {
                GetVersionMock(1, new DateTime(2019, 12, 10)).Object, // should be deleted
                GetVersionMock(2, new DateTime(2019, 12, 19)).Object, // should be deleted
                GetVersionMock(3, new DateTime(2019, 12, 20)).Object, // should be kept
            };

            var logger = Mock.Of<ILogger>();
            var service = new UnversionService(null, logger, null, null);

            var res = service.GetVersionsToDelete(versions, config, new DateTime(2019, 12, 30));

            // Should have two versions to remove
            Assert.IsTrue(res.Count == 2);

            Assert.IsTrue(res.Contains(1));
            Assert.IsTrue(res.Contains(2));

            // Ensure we have not removed the version we want to keep
            Assert.IsFalse(res.Contains(3));
        }

        [Test]
        public void GetVersions_Returns_Right_Based_Max_Count()
        {
            var config = new UnversionConfigEntry() { MaxCount = 5 };

            List<IContent> versions = new List<IContent>()
            {
                GetVersionMock(10, new DateTime(2019, 12, 10)).Object, // should be kept
                GetVersionMock(20, new DateTime(2019, 12, 19)).Object, // should be kept
                GetVersionMock(30, new DateTime(2019, 12, 20)).Object, // should be kept
                GetVersionMock(40, new DateTime(2019, 12, 10)).Object, // should be kept
                GetVersionMock(50, new DateTime(2019, 12, 19)).Object, // should be kept
                GetVersionMock(60, new DateTime(2019, 12, 20)).Object, // should be deleted
                GetVersionMock(70, new DateTime(2019, 12, 10)).Object, // should be deleted
                GetVersionMock(80, new DateTime(2019, 12, 19)).Object, // should be deleted
                GetVersionMock(90, new DateTime(2019, 12, 20)).Object, // should be deleted
            };

            var logger = Mock.Of<ILogger>();
            var service = new UnversionService(null, logger, null, null);

            var res = service.GetVersionsToDelete(versions, config, new DateTime(2019, 12, 30));

            // Has 4 extra versions to remove (leaving the 5 max items we want)
            Assert.IsTrue(res.Count == 4);            

            // Ensure the list of IDs does not contain the versions we want to keep
            Assert.IsFalse(res.Contains(10));
            Assert.IsFalse(res.Contains(20));
            Assert.IsFalse(res.Contains(30));
            Assert.IsFalse(res.Contains(40));
            Assert.IsFalse(res.Contains(50));

            // These ones should be deleted
            Assert.IsTrue(res.Contains(60));
            Assert.IsTrue(res.Contains(70));
            Assert.IsTrue(res.Contains(80));
            Assert.IsTrue(res.Contains(90));            
        }

        private Mock<IContent> GetVersionMock(int versionId, DateTime updateDate)
        {
            var mock = new Mock<IContent>();
            mock.Setup(x => x.VersionId).Returns(versionId);
            mock.Setup(x => x.UpdateDate).Returns(updateDate);
            return mock;
        }
    }
}
