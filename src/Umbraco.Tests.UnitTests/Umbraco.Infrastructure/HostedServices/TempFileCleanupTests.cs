using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Infrastructure.HostedServices;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.HostedServices
{
    [TestFixture]
    public class TempFileCleanupTests
    {
        private Mock<IIOHelper> _mockIOHelper;
        private string _testPath = @"c:\test\temp\path";

        [Test]
        public void Does_Not_Execute_When_Not_Main_Dom()
        {
            var sut = CreateTempFileCleanup(isMainDom: false);
            sut.ExecuteAsync(null);
            VerifyFilesNotCleaned();
        }

        [Test]
        public void Executes_And_Cleans_Files()
        {
            var sut = CreateTempFileCleanup();
            sut.ExecuteAsync(null);
            VerifyFilesCleaned();
        }

        private TempFileCleanup CreateTempFileCleanup(bool isMainDom = true)
        {
            var mockMainDom = new Mock<IMainDom>();
            mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

            _mockIOHelper = new Mock<IIOHelper>();
            _mockIOHelper.Setup(x => x.GetTempFolders()).Returns(new DirectoryInfo[] { new DirectoryInfo(_testPath) });

            var mockLogger = new Mock<ILogger<TempFileCleanup>>();
            var mockProfilingLogger = new Mock<IProfilingLogger>();

            return new TempFileCleanup(_mockIOHelper.Object, mockMainDom.Object, mockLogger.Object);
        }

        private void VerifyFilesNotCleaned()
        {
            VerifyFilesCleaned(Times.Never());
        }

        private void VerifyFilesCleaned()
        {
            VerifyFilesCleaned(Times.Once());
        }

        private void VerifyFilesCleaned(Times times)
        {
            _mockIOHelper.Verify(x => x.CleanFolder(It.Is<DirectoryInfo>(y => y.FullName == _testPath), It.IsAny<TimeSpan>()), times);
        }
    }
}
