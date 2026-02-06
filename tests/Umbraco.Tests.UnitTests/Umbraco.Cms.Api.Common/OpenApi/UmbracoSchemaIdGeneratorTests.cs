using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class UmbracoSchemaIdGeneratorTests
{
    [Test]
    public void Generate_Adds_Model_Suffix_When_Not_Present()
    {
        // Arrange - use a simple Umbraco type that doesn't end with "Model"
        var type = typeof(UmbracoSchemaIdGenerator);

        // Act
        var result = UmbracoSchemaIdGenerator.Generate(type);

        // Assert
        Assert.That(result, Does.EndWith("Model"));
        Assert.AreEqual("UmbracoSchemaIdGeneratorModel", result);
    }

    [Test]
    public void Generate_Does_Not_Add_Model_Suffix_When_Already_Present()
    {
        // Arrange - create test with a type ending in Model
        var type = typeof(TestModel);

        // Act
        var result = UmbracoSchemaIdGenerator.Generate(type);

        // Assert
        Assert.AreEqual("TestModel", result);
    }

    [Test]
    public void Generate_Removes_ViewModel_Suffix()
    {
        // Arrange
        var type = typeof(TestViewModel);

        // Act
        var result = UmbracoSchemaIdGenerator.Generate(type);

        // Assert
        Assert.AreEqual("TestModel", result);
    }

    [Test]
    public void Generate_Handles_Generic_Types()
    {
        // Arrange
        var type = typeof(GenericTestClass<TestModel>);

        // Act
        var result = UmbracoSchemaIdGenerator.Generate(type);

        // Assert
        Assert.AreEqual("GenericTestClassTestModel", result);
    }

    [Test]
    public void Generate_Handles_Generic_Types_With_ViewModel_Suffix()
    {
        // Arrange
        var type = typeof(PagedViewModel<TestViewModel>);

        // Act
        var result = UmbracoSchemaIdGenerator.Generate(type);

        // Assert
        Assert.AreEqual("PagedTestModel", result);
    }

    // Test helper classes in Umbraco.Cms namespace
    private class TestModel;

    private class TestViewModel;

    private class GenericTestClass<T>;

    private class PagedViewModel<T>;
}
