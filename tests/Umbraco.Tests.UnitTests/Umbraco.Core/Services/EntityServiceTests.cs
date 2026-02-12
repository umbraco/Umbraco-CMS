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

[TestFixture]
public class EntityServiceTests
{
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
