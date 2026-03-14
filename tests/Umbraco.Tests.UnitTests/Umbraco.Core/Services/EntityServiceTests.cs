using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

/// <summary>
/// Contains unit tests for the <see cref="EntityService"/> class in the Umbraco Core Services namespace.
/// These tests verify the functionality and behavior of the EntityService.
/// </summary>
[TestFixture]
public class EntityServiceTests
{
    /// <summary>
    /// Tests validation of the sibling index parameters for the entity service, ensuring that invalid indices cause an exception as expected.
    /// </summary>
    /// <param name="before">The index before the current entity; invalid values should trigger an exception.</param>
    /// <param name="after">The index after the current entity; invalid values should trigger an exception.</param>
    /// <param name="shouldThrow">If true, the test expects an <see cref="ArgumentOutOfRangeException"/> to be thrown; otherwise, no exception is expected.</param>
    [TestCase(1, 1, false, TestName = "Siblings_Index_Validation_Valid")]
    [TestCase(-1, 1, true, TestName = "Siblings_Index_Validation_InvalidBefore")]
    [TestCase(1, -1, true, TestName = "Siblings_Index_Validation_InvalidAfter")]
    [TestCase(-1, -1, true, TestName = "Siblings_Index_Validation_InvalidBeforeAndAfter")]
    public void Siblings_Index_Validation(int before, int after, bool shouldThrow)
    {
        var sut = CreateEntityService();

        if (shouldThrow)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetSiblings(Guid.NewGuid(), [UmbracoObjectTypes.Document], before, after, out _, out _));
        }
    }


    private EntityService CreateEntityService() =>
        new(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IEventMessagesFactory>(),
            Mock.Of<IIdKeyMap>(),
            Mock.Of<IEntityRepository>());
}
