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
    [TestCase(-1, 1)]
    [TestCase(1, -1)]
    [TestCase(-1, -1)]
    public void Siblings_Invalid_Index(int before, int after)
    {
        var sut = CreateEntityService();

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.GetSiblings(Guid.NewGuid(), UmbracoObjectTypes.Document, before, after));
    }


    private EntityService CreateEntityService() =>
        new(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IEventMessagesFactory>(),
            Mock.Of<IIdKeyMap>(),
            Mock.Of<IEntityRepository>());
}
