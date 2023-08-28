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
        analyzerOne.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
            .Returns(false);
        var analyzerTwo = new Mock<IFileStreamSecurityAnalyzer>();
        analyzerTwo.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
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
        var matchingAnalyzerOne = new Mock<IFileStreamSecurityAnalyzer>();
        matchingAnalyzerOne.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
            .Returns(true);
        matchingAnalyzerOne.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(true);

        var matchingAnalyzerTwo = new Mock<IFileStreamSecurityAnalyzer>();
        matchingAnalyzerTwo.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
            .Returns(true);
        matchingAnalyzerTwo.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(true);

        var unmatchedAnalyzer = new Mock<IFileStreamSecurityAnalyzer>();
        unmatchedAnalyzer.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
            .Returns(false);

        var sut = new FileStreamSecurityValidator(new List<IFileStreamSecurityAnalyzer>{matchingAnalyzerOne.Object,matchingAnalyzerTwo.Object});

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
        var saveMatchingAnalyzer = new Mock<IFileStreamSecurityAnalyzer>();
        saveMatchingAnalyzer.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
            .Returns(true);
        saveMatchingAnalyzer.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(true);

        var unsafeMatchingAnalyzer = new Mock<IFileStreamSecurityAnalyzer>();
        unsafeMatchingAnalyzer.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
            .Returns(true);
        unsafeMatchingAnalyzer.Setup(analyzer => analyzer.IsConsideredSafe(It.IsAny<Stream>()))
            .Returns(false);

        var unmatchedAnalyzer = new Mock<IFileStreamSecurityAnalyzer>();
        unmatchedAnalyzer.Setup(analyzer => analyzer.ShouldHandle(It.IsAny<Stream>()))
            .Returns(false);

        var sut = new FileStreamSecurityValidator(new List<IFileStreamSecurityAnalyzer>{saveMatchingAnalyzer.Object,unsafeMatchingAnalyzer.Object});

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
