// tests/Umbraco.Tests.UnitTests/Umbraco.Core/Services/ContentQueryOperationServiceInterfaceTests.cs
using NUnit.Framework;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentQueryOperationServiceInterfaceTests
{
    [Test]
    public void IContentQueryOperationService_Interface_Exists()
    {
        // Arrange & Act
        var interfaceType = typeof(IContentQueryOperationService);

        // Assert
        Assert.That(interfaceType, Is.Not.Null);
        Assert.That(interfaceType.IsInterface, Is.True);
    }

    [Test]
    public void IContentQueryOperationService_Extends_IService()
    {
        // Arrange
        var interfaceType = typeof(IContentQueryOperationService);

        // Act & Assert
        Assert.That(typeof(IService).IsAssignableFrom(interfaceType), Is.True);
    }
}
