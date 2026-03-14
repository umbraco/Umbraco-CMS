using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence;

    /// <summary>
    /// Unit tests for the <see cref="DefaultDatabaseAvailabilityCheck"/> class.
    /// </summary>
[TestFixture]
public class DefaultDatabaseAvailabilityCheckTests
{
    /// <summary>
    /// Tests that IsDatabaseAvailable returns false when the database is unavailable.
    /// </summary>
    [Test]
    public void IsDatabaseAvailable_WithDatabaseUnavailable_ReturnsFalse()
    {
        var mockDatabaseFactory = new Mock<IUmbracoDatabaseFactory>();
        mockDatabaseFactory
            .Setup(x => x.CanConnect)
            .Returns(false);

        var sut = CreateDefaultDatabaseAvailabilityCheck();
        var result = sut.IsDatabaseAvailable(mockDatabaseFactory.Object);
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that IsDatabaseAvailable returns true when the database is immediately available.
    /// </summary>
    [Test]
    public void IsDatabaseAvailable_WithDatabaseImmediatelyAvailable_ReturnsTrue()
    {
        var mockDatabaseFactory = new Mock<IUmbracoDatabaseFactory>();
        mockDatabaseFactory
            .Setup(x => x.CanConnect)
            .Returns(true);

        var sut = CreateDefaultDatabaseAvailabilityCheck();
        var result = sut.IsDatabaseAvailable(mockDatabaseFactory.Object);
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that the database availability check returns the expected result when the database becomes available on a specific attempt after several failed attempts.
    /// </summary>
    /// <param name="attemptsUntilConnection">The number of attempts before the database connection succeeds (all previous attempts fail).</param>
    /// <param name="expectedResult">The expected result indicating whether the database is considered available.</param>
    [TestCase(5, true)]
    [TestCase(6, false)]
    public void IsDatabaseAvailable_WithDatabaseImmediatelyAvailableAfterMultipleAttempts_ReturnsExpectedResult(int attemptsUntilConnection, bool expectedResult)
    {
        if (attemptsUntilConnection < 1)
        {
            throw new ArgumentException($"{nameof(attemptsUntilConnection)} must be greater than or equal to 1.", nameof(attemptsUntilConnection));
        }

        var attemptResults = new Queue<bool>();
        for (var i = 0; i < attemptsUntilConnection - 1; i++)
        {
            attemptResults.Enqueue(false);
        }

        attemptResults.Enqueue(true);

        var mockDatabaseFactory = new Mock<IUmbracoDatabaseFactory>();
        mockDatabaseFactory
            .Setup(x => x.CanConnect)
            .Returns(attemptResults.Dequeue);

        var sut = CreateDefaultDatabaseAvailabilityCheck();
        var result = sut.IsDatabaseAvailable(mockDatabaseFactory.Object);
        Assert.AreEqual(expectedResult, result);
    }

    private static DefaultDatabaseAvailabilityCheck CreateDefaultDatabaseAvailabilityCheck()
        => new(new NullLogger<DefaultDatabaseAvailabilityCheck>())
        {
            AttemptDelayMilliseconds = 1 // Set to 1 ms for faster tests.
        };
}
