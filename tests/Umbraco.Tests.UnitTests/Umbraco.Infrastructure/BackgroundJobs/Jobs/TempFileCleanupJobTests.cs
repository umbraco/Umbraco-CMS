// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs
{
    /// <summary>
    /// Contains unit tests that verify the behavior of the <see cref="TempFileCleanupJob"/> class.
    /// </summary>
    [TestFixture]
    public class TempFileCleanupJobTests
    {
        private Mock<IIOHelper> _mockIOHelper;
        private readonly string _testPath = Path.Combine(TestContext.CurrentContext.TestDirectory.Split("bin")[0], "App_Data", "TEMP");


    /// <summary>
    /// Tests that the TempFileCleanupJob executes and cleans up files as expected.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
        [Test]
        public async Task Executes_And_Cleans_Files()
        {
            TempFileCleanupJob sut = CreateTempFileCleanupJob();
            await sut.RunJobAsync();
            VerifyFilesCleaned();
        }

        private TempFileCleanupJob CreateTempFileCleanupJob()
        {

            _mockIOHelper = new Mock<IIOHelper>();
            _mockIOHelper.Setup(x => x.GetTempFolders())
                .Returns(new DirectoryInfo[] { new(_testPath) });
            _mockIOHelper.Setup(x => x.CleanFolder(It.IsAny<DirectoryInfo>(), It.IsAny<TimeSpan>()))
                .Returns(CleanFolderResult.Success());

            var mockLogger = new Mock<ILogger<TempFileCleanupJob>>();

            return new TempFileCleanupJob(_mockIOHelper.Object,mockLogger.Object);
        }

        private void VerifyFilesNotCleaned() => VerifyFilesCleaned(Times.Never());

        private void VerifyFilesCleaned() => VerifyFilesCleaned(Times.Once());

        private void VerifyFilesCleaned(Times times) => _mockIOHelper.Verify(x => x.CleanFolder(It.Is<DirectoryInfo>(y => y.FullName == _testPath), It.IsAny<TimeSpan>()), times);
    }
}
