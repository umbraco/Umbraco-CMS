using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Mapping;

/// <summary>
/// Provides unit tests for verifying the common mapping functionality within the Umbraco Core model mappings.
/// </summary>
[TestFixture]
public class CommonMapperTests
{
    private Mock<IUserService> _userService = null!;
    private CommonMapper _commonMapper = null!;
    private Mock<IUmbracoMapper> _umbracoMapper = null!;

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>();
        _commonMapper = new CommonMapper(_userService.Object);
        _umbracoMapper = new Mock<IUmbracoMapper>();
    }

    /// <summary>
    /// Tests that GetOwnerName uses the provided dictionary to resolve the owner's name when present,
    /// avoiding calls to the user service.
    /// </summary>
    [Test]
    public void GetOwnerName_Uses_Dictionary_When_Present()
    {
        // Arrange
        var context = new MapperContext(_umbracoMapper.Object);
        var userNames = new Dictionary<int, string?> { { 42, "Alice" } };
        context.SetUserNameDictionary(userNames);

        var content = new Mock<IContentBase>();
        content.Setup(c => c.CreatorId).Returns(42);

        // Act
        var result = _commonMapper.GetOwnerName(content.Object, context);

        // Assert
        Assert.AreEqual("Alice", result);
        _userService.Verify(x => x.GetProfileById(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Tests that GetOwnerName falls back to using the user service to retrieve the owner's name when the dictionary is missing.
    /// </summary>
    [Test]
    public void GetOwnerName_Falls_Back_To_Service_When_Dictionary_Missing()
    {
        // Arrange
        var context = new MapperContext(_umbracoMapper.Object);

        var profile = new Mock<IProfile>();
        profile.Setup(p => p.Name).Returns("Bob");
        _userService.Setup(x => x.GetProfileById(99)).Returns(profile.Object);

        var content = new Mock<IContentBase>();
        content.Setup(c => c.CreatorId).Returns(99);

        // Act
        var result = _commonMapper.GetOwnerName(content.Object, context);

        // Assert
        Assert.AreEqual("Bob", result);
        _userService.Verify(x => x.GetProfileById(99), Times.Once);
    }

    /// <summary>
    /// Tests that GetOwnerName falls back to retrieving the user name from the user service when the user is not found in the provided dictionary.
    /// </summary>
    [Test]
    public void GetOwnerName_Falls_Back_To_Service_When_User_Not_In_Dictionary()
    {
        // Arrange
        var context = new MapperContext(_umbracoMapper.Object);
        var userNames = new Dictionary<int, string?> { { 1, "Alice" } };
        context.SetUserNameDictionary(userNames);

        var profile = new Mock<IProfile>();
        profile.Setup(p => p.Name).Returns("Charlie");
        _userService.Setup(x => x.GetProfileById(99)).Returns(profile.Object);

        var content = new Mock<IContentBase>();
        content.Setup(c => c.CreatorId).Returns(99);

        // Act
        var result = _commonMapper.GetOwnerName(content.Object, context);

        // Assert
        Assert.AreEqual("Charlie", result);
        _userService.Verify(x => x.GetProfileById(99), Times.Once);
    }

    /// <summary>
    /// Verifies that <c>GetCreatorName</c> retrieves the creator's name from the provided user name dictionary
    /// in the <see cref="MapperContext"/> when the dictionary contains the writer's ID, and does not call the user service.
    /// </summary>
    [Test]
    public void GetCreatorName_Uses_Dictionary_When_Present()
    {
        // Arrange
        var context = new MapperContext(_umbracoMapper.Object);
        var userNames = new Dictionary<int, string?> { { 55, "Writer Person" } };
        context.SetUserNameDictionary(userNames);

        var content = new Mock<IContent>();
        content.Setup(c => c.WriterId).Returns(55);

        // Act
        var result = _commonMapper.GetCreatorName(content.Object, context);

        // Assert
        Assert.AreEqual("Writer Person", result);
        _userService.Verify(x => x.GetProfileById(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Tests that GetCreatorName falls back to the user service to retrieve the creator's name when the dictionary is missing.
    /// </summary>
    [Test]
    public void GetCreatorName_Falls_Back_To_Service_When_Dictionary_Missing()
    {
        // Arrange
        var context = new MapperContext(_umbracoMapper.Object);

        var profile = new Mock<IProfile>();
        profile.Setup(p => p.Name).Returns("Dave");
        _userService.Setup(x => x.GetProfileById(77)).Returns(profile.Object);

        var content = new Mock<IContent>();
        content.Setup(c => c.WriterId).Returns(77);

        // Act
        var result = _commonMapper.GetCreatorName(content.Object, context);

        // Assert
        Assert.AreEqual("Dave", result);
        _userService.Verify(x => x.GetProfileById(77), Times.Once);
    }

    /// <summary>
    /// Tests that GetCreatorName falls back to the user service to retrieve the creator's name when the user is not found in the provided dictionary.
    /// </summary>
    [Test]
    public void GetCreatorName_Falls_Back_To_Service_When_User_Not_In_Dictionary()
    {
        // Arrange
        var context = new MapperContext(_umbracoMapper.Object);
        var userNames = new Dictionary<int, string?> { { 1, "Alice" } };
        context.SetUserNameDictionary(userNames);

        var profile = new Mock<IProfile>();
        profile.Setup(p => p.Name).Returns("Eve");
        _userService.Setup(x => x.GetProfileById(88)).Returns(profile.Object);

        var content = new Mock<IContent>();
        content.Setup(c => c.WriterId).Returns(88);

        // Act
        var result = _commonMapper.GetCreatorName(content.Object, context);

        // Assert
        Assert.AreEqual("Eve", result);
        _userService.Verify(x => x.GetProfileById(88), Times.Once);
    }

    /// <summary>
    /// Tests that GetOwnerName returns null when the user name in the dictionary is null.
    /// </summary>
    [Test]
    public void GetOwnerName_Returns_Null_From_Dictionary_When_User_Name_Is_Null()
    {
        // Arrange
        var context = new MapperContext(_umbracoMapper.Object);
        var userNames = new Dictionary<int, string?> { { 42, null } };
        context.SetUserNameDictionary(userNames);

        var content = new Mock<IContentBase>();
        content.Setup(c => c.CreatorId).Returns(42);

        // Act
        var result = _commonMapper.GetOwnerName(content.Object, context);

        // Assert
        Assert.IsNull(result);
        _userService.Verify(x => x.GetProfileById(It.IsAny<int>()), Times.Never);
    }
}
