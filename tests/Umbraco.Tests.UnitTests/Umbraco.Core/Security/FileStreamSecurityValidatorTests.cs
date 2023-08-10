using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Security;

public class FileStreamSecurityValidatorTests
{
    [Test]
    public void IsConsideredSafe_True_NoAnalyzersPresent()
    {
        // Arrange
        var sut = new FileStreamSecurityValidator(Enumerable.Empty<IFileStreamSecurityAnalyzer>());

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write("TestContent");
        streamWriter.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Act
        var validationResult = sut.IsConsideredSafe(memoryStream);

        // Assert
        Assert.IsTrue(validationResult);
    }

    [Test]
    public void IsConsideredSafe_True_NoAnalyzerMatchesType()
    {
        // Arrange
        var analyzerOne = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerOne.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(false);
        var analyzerTwo = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerTwo.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(false);

        var sut = new FileStreamSecurityValidator(new List<IFileStreamSecurityAnalyzer>{analyzerOne.Object,analyzerTwo.Object});

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write("TestContent");
        streamWriter.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Act
        var validationResult = sut.IsConsideredSafe(memoryStream);

        // Assert
        Assert.IsTrue(validationResult);
    }

    [Test]
    public void IsConsideredSafe_True_AllMatchingAnalyzersReturnTrue()
    {
        // Arrange
        var analyzerOne = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerOne.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(true);
        analyzerOne.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(true);

        var analyzerTwo = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerTwo.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(true);
        analyzerTwo.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(true);

        var analyzerThree = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerThree.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(false);

        var sut = new FileStreamSecurityValidator(new List<IFileStreamSecurityAnalyzer>{analyzerOne.Object,analyzerTwo.Object});

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write("TestContent");
        streamWriter.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Act
        var validationResult = sut.IsConsideredSafe(memoryStream);

        // Assert
        Assert.IsTrue(validationResult);
    }

    [Test]
    public void IsConsideredSafe_False_AnyMatchingAnalyzersReturnFalse()
    {
        // Arrange
        var analyzerOne = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerOne.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(true);
        analyzerOne.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(true);

        var analyzerTwo = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerTwo.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(true);
        analyzerTwo.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(false);

        var analyzerThree = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerThree.Setup(analyzer => analyzer.FileContentMatchesFileType(It.IsAny<Stream>()))
            .Returns(false);

        var sut = new FileStreamSecurityValidator(new List<IFileStreamSecurityAnalyzer>{analyzerOne.Object,analyzerTwo.Object});

        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write("TestContent");
        streamWriter.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);

        // Act
        var validationResult = sut.IsConsideredSafe(memoryStream);

        // Assert
        Assert.IsFalse(validationResult);
    }
}
