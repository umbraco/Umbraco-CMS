// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices
{
    [TestFixture]
    public class TempFileCleanupTests
    {
        private Mock<IIOHelper> _mockIOHelper;
        private readonly string _testPath = Path.Combine(TestContext.CurrentContext.TestDirectory.Split("bin")[0], "App_Data", "TEMP");

        [Test]
        public async Task Does_Not_Execute_When_Not_Main_Dom()
        {
            TempFileCleanup sut = CreateTempFileCleanup(isMainDom: false);
            await sut.PerformExecuteAsync(null);
            VerifyFilesNotCleaned();
        }

        [Test]
        public async Task Executes_And_Cleans_Files()
        {
            TempFileCleanup sut = CreateTempFileCleanup();
            await sut.PerformExecuteAsync(null);
            VerifyFilesCleaned();
        }

        private TempFileCleanup CreateTempFileCleanup(bool isMainDom = true)
        {
            var mockMainDom = new Mock<IMainDom>();
            mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

            _mockIOHelper = new Mock<IIOHelper>();
            _mockIOHelper.Setup(x => x.GetTempFolders())
                .Returns(new DirectoryInfo[] { new(_testPath) });
            _mockIOHelper.Setup(x => x.CleanFolder(It.IsAny<DirectoryInfo>(), It.IsAny<TimeSpan>()))
                .Returns(CleanFolderResult.Success());

            var mockLogger = new Mock<ILogger<TempFileCleanup>>();

            return new TempFileCleanup(_mockIOHelper.Object, mockMainDom.Object, mockLogger.Object);
        }

        private void VerifyFilesNotCleaned() => VerifyFilesCleaned(Times.Never());

        private void VerifyFilesCleaned() => VerifyFilesCleaned(Times.Once());

        private void VerifyFilesCleaned(Times times) => _mockIOHelper.Verify(x => x.CleanFolder(It.Is<DirectoryInfo>(y => y.FullName == _testPath), It.IsAny<TimeSpan>()), times);
    }
}
