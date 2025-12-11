using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class SchemaIdHandlerTests
{
    private SchemaIdHandler _handler = null!;

    [SetUp]
    public void SetUp() => _handler = new SchemaIdHandler();

    [Test]
    public void CanHandle_Returns_True_For_Umbraco_Cms_Namespace()
    {
        // Arrange
        var type = typeof(SchemaIdHandler); // Umbraco.Cms.Api.Common.OpenApi

        // Act
        var result = _handler.CanHandle(type);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CanHandle_Returns_False_For_Non_Umbraco_Namespace()
    {
        // Arrange
        var type = typeof(string); // System namespace

        // Act
        var result = _handler.CanHandle(type);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void Handle_Adds_Model_Suffix_When_Not_Present()
    {
        // Arrange - use a simple Umbraco type that doesn't end with "Model"
        var type = typeof(SchemaIdHandler);

        // Act
        var result = _handler.Handle(type);

        // Assert
        Assert.That(result, Does.EndWith("Model"));
        Assert.AreEqual("SchemaIdHandlerModel", result);
    }

    [Test]
    public void Handle_Does_Not_Add_Model_Suffix_When_Already_Present()
    {
        // Arrange - create test with a type ending in Model
        // Using the handler itself with a mock scenario - we'll test with TestModel class
        var type = typeof(TestModel);

        // Act
        var result = _handler.Handle(type);

        // Assert
        Assert.AreEqual("TestModel", result);
    }

    [Test]
    public void Handle_Removes_ViewModel_Suffix()
    {
        // Arrange
        var type = typeof(TestViewModel);

        // Act
        var result = _handler.Handle(type);

        // Assert
        Assert.AreEqual("TestModel", result);
    }

    [Test]
    public void Handle_Handles_Generic_Types()
    {
        // Arrange
        var type = typeof(GenericTestClass<TestModel>);

        // Act
        var result = _handler.Handle(type);

        // Assert
        Assert.AreEqual("GenericTestClassTestModel", result);
    }

    [Test]
    public void Handle_Handles_Generic_Types_With_ViewModel_Suffix()
    {
        // Arrange
        var type = typeof(PagedViewModel<TestViewModel>);

        // Act
        var result = _handler.Handle(type);

        // Assert
        Assert.AreEqual("PagedTestModel", result);
    }

    // Test helper classes in Umbraco.Cms namespace
    private class TestModel;

    private class TestViewModel;

    private class GenericTestClass<T>;

    private class PagedViewModel<T>;
}
