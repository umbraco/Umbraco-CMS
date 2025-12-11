using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class SchemaIdSelectorTests
{
    [Test]
    public void SchemaId_Uses_Handler_When_CanHandle_Returns_True()
    {
        // Arrange
        var type = typeof(string);
        const string expectedSchemaId = "CustomSchemaId";

        var mockHandler = new Mock<ISchemaIdHandler>();
        mockHandler.Setup(h => h.CanHandle(type)).Returns(true);
        mockHandler.Setup(h => h.Handle(type)).Returns(expectedSchemaId);

        var selector = new SchemaIdSelector([mockHandler.Object]);

        // Act
        var result = selector.SchemaId(type);

        // Assert
        Assert.AreEqual(expectedSchemaId, result);
        mockHandler.Verify(h => h.Handle(type), Times.Once);
    }

    [Test]
    public void SchemaId_Falls_Back_To_Type_Name_When_No_Handler_Matches()
    {
        // Arrange
        var type = typeof(string);

        var mockHandler = new Mock<ISchemaIdHandler>();
        mockHandler.Setup(h => h.CanHandle(type)).Returns(false);

        var selector = new SchemaIdSelector([mockHandler.Object]);

        // Act
        var result = selector.SchemaId(type);

        // Assert
        Assert.AreEqual("String", result);
        mockHandler.Verify(h => h.Handle(It.IsAny<Type>()), Times.Never);
    }

    [Test]
    public void SchemaId_Falls_Back_To_Type_Name_When_No_Handlers_Registered()
    {
        // Arrange
        var type = typeof(int);
        var selector = new SchemaIdSelector([]);

        // Act
        var result = selector.SchemaId(type);

        // Assert
        Assert.AreEqual("Int32", result);
    }

    [Test]
    public void SchemaId_Unwraps_Nullable_Types()
    {
        // Arrange
        var nullableType = typeof(int?);
        const string expectedSchemaId = "Int32";

        var mockHandler = new Mock<ISchemaIdHandler>();
        mockHandler.Setup(h => h.CanHandle(typeof(int))).Returns(true);
        mockHandler.Setup(h => h.Handle(typeof(int))).Returns(expectedSchemaId);

        var selector = new SchemaIdSelector([mockHandler.Object]);

        // Act
        var result = selector.SchemaId(nullableType);

        // Assert
        Assert.AreEqual(expectedSchemaId, result);
        mockHandler.Verify(h => h.CanHandle(typeof(int)), Times.Once);
        mockHandler.Verify(h => h.Handle(typeof(int)), Times.Once);
    }

    [Test]
    public void SchemaId_Uses_First_Matching_Handler()
    {
        // Arrange
        var type = typeof(string);
        const string firstHandlerSchemaId = "FirstHandlerSchemaId";
        const string secondHandlerSchemaId = "SecondHandlerSchemaId";

        var firstHandler = new Mock<ISchemaIdHandler>();
        firstHandler.Setup(h => h.CanHandle(type)).Returns(true);
        firstHandler.Setup(h => h.Handle(type)).Returns(firstHandlerSchemaId);

        var secondHandler = new Mock<ISchemaIdHandler>();
        secondHandler.Setup(h => h.CanHandle(type)).Returns(true);
        secondHandler.Setup(h => h.Handle(type)).Returns(secondHandlerSchemaId);

        var selector = new SchemaIdSelector([firstHandler.Object, secondHandler.Object]);

        // Act
        var result = selector.SchemaId(type);

        // Assert
        Assert.AreEqual(firstHandlerSchemaId, result);
        firstHandler.Verify(h => h.Handle(type), Times.Once);
        secondHandler.Verify(h => h.Handle(type), Times.Never);
    }

    [Test]
    public void SchemaId_Skips_Non_Matching_Handlers()
    {
        // Arrange
        var type = typeof(string);
        const string expectedSchemaId = "SecondHandlerSchemaId";

        var firstHandler = new Mock<ISchemaIdHandler>();
        firstHandler.Setup(h => h.CanHandle(type)).Returns(false);

        var secondHandler = new Mock<ISchemaIdHandler>();
        secondHandler.Setup(h => h.CanHandle(type)).Returns(true);
        secondHandler.Setup(h => h.Handle(type)).Returns(expectedSchemaId);

        var selector = new SchemaIdSelector([firstHandler.Object, secondHandler.Object]);

        // Act
        var result = selector.SchemaId(type);

        // Assert
        Assert.AreEqual(expectedSchemaId, result);
        firstHandler.Verify(h => h.Handle(type), Times.Never);
        secondHandler.Verify(h => h.Handle(type), Times.Once);
    }
}
