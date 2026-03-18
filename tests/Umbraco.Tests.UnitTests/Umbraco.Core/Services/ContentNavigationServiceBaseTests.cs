using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

/// <summary>
/// Contains unit tests that verify the functionality of the <see cref="ContentNavigationServiceBase"/> class.
/// </summary>
[TestFixture]
public class ContentNavigationServiceBaseTests
{
    private TestContentNavigationService _navigationService;

    private Guid ContentType { get; set; }

    private Guid Root { get; set; }

    private Guid Child1 { get; set; }

    private Guid Grandchild1 { get; set; }

    private Guid Grandchild2 { get; set; }

    private Guid Child2 { get; set; }

    private Guid Grandchild3 { get; set; }

    private Guid GreatGrandchild1 { get; set; }

    private Guid Child3 { get; set; }

    private Guid Grandchild4 { get; set; }

    /// <summary>
    /// Initializes the test environment for <see cref="ContentNavigationServiceBaseTests"/>,
    /// including setting up the test navigation service and creating the content hierarchy test data.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IContentTypeService>());

        // Root - E48DD82A-7059-418E-9B82-CDD5205796CF
        //    - Child 1 - C6173927-0C59-4778-825D-D7B9F45D8DDE
        //      - Grandchild 1 - E856AC03-C23E-4F63-9AA9-681B42A58573
        //      - Grandchild 2 - A1B1B217-B02F-4307-862C-A5E22DB729EB
        //    - Child 2 - 60E0E5C4-084E-4144-A560-7393BEAD2E96
        //      - Grandchild 3 - D63C1621-C74A-4106-8587-817DEE5FB732
        //        - Great-grandchild 1 - 56E29EA9-E224-4210-A59F-7C2C5C0C5CC7
        //    - Child 3 - B606E3FF-E070-4D46-8CB9-D31352029FDF
        //      - Grandchild 4 - F381906C-223C-4466-80F7-B63B4EE073F8
        CreateTestData();
    }

    /// <summary>
    /// Tests that getting the parent key from a non-existing content key returns false and null.
    /// </summary>
    [Test]
    public void Cannot_Get_Parent_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.TryGetParentKey(nonExistingKey, out Guid? parentKey);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsNull(parentKey);
        });
    }

    /// <summary>
    /// Verifies that the parent key can be correctly retrieved for a given content item's key.
    /// </summary>
    /// <param name="childKey">The unique key (GUID) of the child content item whose parent is to be retrieved.</param>
    /// <param name="expectedParentKey">The expected unique key (GUID) of the parent content item, or <c>null</c> if the item is a root node and has no parent.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", null)] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", "D63C1621-C74A-4106-8587-817DEE5FB732")] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", "B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Grandchild 4
    public void Can_Get_Parent_From_Existing_Content_Key(Guid childKey, Guid? expectedParentKey)
    {
        // Act
        var result = _navigationService.TryGetParentKey(childKey, out Guid? parentKey);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);

            if (expectedParentKey is null)
            {
                Assert.IsNull(parentKey);
            }
            else
            {
                Assert.IsNotNull(parentKey);
                Assert.AreEqual(expectedParentKey, parentKey);
            }
        });
    }

    /// <summary>
    /// Tests that no root items are returned when the navigation tree is empty.
    /// </summary>
    [Test]
    public void Cannot_Get_Root_Items_When_Empty_Tree()
    {
        // Arrange
        var emptyNavigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IContentTypeService>());

        // Act
        emptyNavigationService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
        List<Guid> rootsList = rootKeys.ToList();

        // Assert
        Assert.IsEmpty(rootsList);
    }

    /// <summary>
    /// Tests that a single root item can be retrieved successfully.
    /// </summary>
    [Test]
    public void Can_Get_Single_Root_Item()
    {
        // Act
        var result = _navigationService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
        List<Guid> rootsList = rootKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.IsNotEmpty(rootsList);
            Assert.AreEqual(1, rootsList.Count);
            Assert.IsTrue(rootsList.Contains(Root));
        });
    }

    /// <summary>
    /// Tests that the root items can be retrieved in the correct order.
    /// </summary>
    [Test]
    public void Can_Get_Root_Item_In_Correct_Order()
    {
        // Arrange
        Guid anotherRoot = Guid.NewGuid();
        _navigationService.Add(anotherRoot, ContentType);

        // Act
        var result = _navigationService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);
        List<Guid> rootsList = rootKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(2, rootsList.Count);
            CollectionAssert.AreEqual(new[] { Root, anotherRoot }, rootsList); // Root and Another root in order
        });
    }

    /// <summary>
    /// Tests that attempting to get root items of a non-existing content type alias returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Root_Items_Of_Type_From_Non_Existing_Content_Type_Alias()
    {
        // Arrange
        var nonExistingContentTypeAlias = string.Empty;

        // Act
        var result = _navigationService.TryGetRootKeysOfType(nonExistingContentTypeAlias, out IEnumerable<Guid> rootKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(rootKeys);
        });
    }

    /// <summary>
    /// Tests that the navigation service can retrieve root items of a specified content type.
    /// </summary>
    [Test]
    public void Can_Get_Root_Items_Of_Type()
    {
        // Arrange
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        Guid anotherRoot = Guid.NewGuid();
        _navigationService.Add(anotherRoot, ContentType);

        // Act
        var result = _navigationService.TryGetRootKeysOfType(contentTypeAlias, out IEnumerable<Guid> rootKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(2, rootKeys.Count());
        });
    }

    /// <summary>
    /// Tests that retrieving root items filtered by a specific content type alias returns the correct subset of root items.
    /// </summary>
    [Test]
    public void Can_Get_Root_Items_Of_Type_Filters_Result()
    {
        // Arrange
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new root items with different content type
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey);
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey);

        // Act
        _navigationService.TryGetRootKeysOfType(anotherContentTypeAlias, out IEnumerable<Guid> rootKeysOfType);
        var rootsOfTypeCount = rootKeysOfType.Count();

        // Assert
        // Retrieve all root items without filtering to compare
        _navigationService.TryGetRootKeys(out IEnumerable<Guid> allRootKeys);
        var allRootsCount = allRootKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsTrue(allRootsCount > rootsOfTypeCount);
            Assert.AreEqual(3, allRootsCount);
            Assert.AreEqual(2, rootsOfTypeCount);
        });
    }

    /// <summary>
    /// Tests that the service can retrieve root items of a specific content type,
    /// filters the results accordingly, and maintains the order of creation of those items.
    /// </summary>
    [Test]
    public void Can_Get_Root_Items_Of_Type_Filters_Result_And_Maintains_Their_Order_Of_Creation()
    {
        // Arrange
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new root items with different content type
        Guid root2 = Guid.NewGuid();
        Guid root3 = Guid.NewGuid();
        _navigationService.Add(root2, anotherContentTypeKey);
        _navigationService.Add(root3, anotherContentTypeKey);

        var expectedRootsOrder = new List<Guid> { root2, root3 };

        // Act
        _navigationService.TryGetRootKeysOfType(anotherContentTypeAlias, out IEnumerable<Guid> rootKeysOfType);

        // Assert
        // Check that the order matches what is expected
        Assert.IsTrue(expectedRootsOrder.SequenceEqual(rootKeysOfType));
    }

    /// <summary>
    /// Tests that root items of a specific content type can be retrieved even when the content type was not initially loaded.
    /// </summary>
    [Test]
    public void Can_Get_Root_Items_Of_Type_Even_When_Content_Type_Was_Not_Initially_Loaded()
    {
        // Arrange
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x
            .Get(It.Is<string>(alias => alias == contentTypeAlias)))
            .Returns(contentTypeMock.Object);

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Act
        _navigationService.TryGetRootKeysOfType(contentTypeAlias, out IEnumerable<Guid> rootKeys);

        // Assert
        Assert.AreEqual(1, rootKeys.Count());
    }

    /// <summary>
    /// Tests that attempting to get children keys from a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Children_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.TryGetChildrenKeys(nonExistingKey, out IEnumerable<Guid> childrenKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(childrenKeys);
        });
    }

    /// <summary>
    /// Tests that children keys can be retrieved from an existing content key.
    /// </summary>
    /// <param name="parentKey">The GUID key of the parent content item.</param>
    /// <param name="childrenCount">The expected number of children for the given parent key.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 3)] // Root - Child 1, Child 2, Child 3
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 1 - Grandchild 1, Grandchild 2
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 0)] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 0)] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 1)] // Child 2 - Grandchild 3
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 1)] // Grandchild 3 - Great-grandchild 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 0)] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 3 - Grandchild 4
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 0)] // Grandchild 4
    public void Can_Get_Children_From_Existing_Content_Key(Guid parentKey, int childrenCount)
    {
        // Act
        var result = _navigationService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> childrenKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(childrenCount, childrenKeys.Count());
        });
    }

    /// <summary>
    /// Verifies that the navigation service retrieves the child content keys of a given parent content item
    /// in the exact order they were created.
    /// </summary>
    /// <param name="parentKey">The unique identifier (key) of the parent content item whose children are to be retrieved.</param>
    /// <param name="children">An array of string representations of the expected child content keys, in their creation order.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF", })] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "E856AC03-C23E-4F63-9AA9-681B42A58573", "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new string[0])] // Grandchild 1
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", new[] { "D63C1621-C74A-4106-8587-817DEE5FB732" })] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", new[] { "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", new string[0])] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", new[] { "F381906C-223C-4466-80F7-B63B4EE073F8" })] // Child 3
    public void Can_Get_Children_From_Existing_Content_Key_In_Their_Order_Of_Creation(Guid parentKey, string[] children)
    {
        // Arrange
        Guid[] expectedChildren = Array.ConvertAll(children, Guid.Parse);

        // Act
        _navigationService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> childrenKeys);
        List<Guid> childrenList = childrenKeys.ToList();

        // Assert
        for (var i = 0; i < expectedChildren.Length; i++)
        {
            Assert.AreEqual(expectedChildren[i], childrenList.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that trying to get children of a non-existing content type alias returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Children_Of_Type_From_Non_Existing_Content_Type_Alias()
    {
        // Arrange
        Guid parentKey = Root;
        var nonExistingContentTypeAlias = string.Empty;

        // Act
        var result = _navigationService.TryGetChildrenKeysOfType(parentKey, nonExistingContentTypeAlias, out IEnumerable<Guid> childrenKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(childrenKeys);
        });
    }

    /// <summary>
    /// Tests that attempting to get children of a specific content type from a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Children_Of_Type_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // Act
        var result = _navigationService.TryGetChildrenKeysOfType(nonExistingKey, contentTypeAlias, out IEnumerable<Guid> childrenKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(childrenKeys);
        });
    }

    /// <summary>
    /// Verifies that the navigation service can retrieve the keys of all child content items of a specified content type for a given parent content item's unique identifier.
    /// </summary>
    /// <param name="parentKey">The unique identifier (GUID) of the parent content item whose children are to be retrieved.</param>
    /// <param name="childrenCount">The expected number of child items of the specified content type under the given parent.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 3)] // Root - Child 1, Child 2, Child 3
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 1 - Grandchild 1, Grandchild 2
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 0)] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 0)] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 1)] // Child 2 - Grandchild 3
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 1)] // Grandchild 3 - Great-grandchild 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 0)] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 3 - Grandchild 4
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 0)] // Grandchild 4
    public void Can_Get_Children_Of_Type(Guid parentKey, int childrenCount)
    {
        // Arrange
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Act
        var result = _navigationService.TryGetChildrenKeysOfType(parentKey, contentTypeAlias, out IEnumerable<Guid> childrenKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(childrenCount, childrenKeys.Count());
        });
    }

    /// <summary>
    /// Verifies that retrieving children by a specific content type alias returns only those children,
    /// ensuring the filtering mechanism works as expected.
    /// </summary>
    [Test]
    public void Can_Get_Children_Of_Type_Filters_Result()
    {
        // Arrange
        Guid parentKey = Root;
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new children with different content type under Root
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey, Root);
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey, Root);

        // Act
        _navigationService.TryGetChildrenKeysOfType(parentKey, anotherContentTypeAlias, out IEnumerable<Guid> childrenKeysOfType);
        var childrenOfTypeCount = childrenKeysOfType.Count();

        // Assert
        // Retrieve all children without filtering to compare
        _navigationService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> allChildrenKeys);
        var allChildrenCount = allChildrenKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsTrue(allChildrenCount > childrenOfTypeCount);
            Assert.AreEqual(5, allChildrenCount);
            Assert.AreEqual(2, childrenOfTypeCount);
        });
    }

    /// <summary>
    /// Tests that retrieving children of a specific content type filters the results correctly
    /// and maintains the order in which the children were created.
    /// </summary>
    [Test]
    public void Can_Get_Children_Of_Type_Filters_Result_And_Maintains_Their_Order_Of_Creation()
    {
        // Arrange
        Guid parentKey = Root;
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new children with different content type under Root
        Guid child4 = Guid.NewGuid();
        Guid child5 = Guid.NewGuid();
        _navigationService.Add(child4, anotherContentTypeKey, Root);
        _navigationService.Add(child5, anotherContentTypeKey, Root);

        var expectedChildrenOrder = new List<Guid> { child4, child5 };

        // Act
        _navigationService.TryGetChildrenKeysOfType(parentKey, anotherContentTypeAlias, out IEnumerable<Guid> childrenKeysOfType);

        // Assert
        // Check that the order matches what is expected
        Assert.IsTrue(expectedChildrenOrder.SequenceEqual(childrenKeysOfType));
    }

    /// <summary>
    /// Tests that children of a specific content type can be retrieved even when the content type was not initially loaded.
    /// </summary>
    [Test]
    public void Can_Get_Children_Of_Type_Even_When_Content_Type_Was_Not_Initially_Loaded()
    {
        // Arrange
        Guid parentKey = Child1;
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x
            .Get(It.Is<string>(alias => alias == contentTypeAlias)))
            .Returns(contentTypeMock.Object);

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Act
        _navigationService.TryGetChildrenKeysOfType(parentKey, contentTypeAlias, out IEnumerable<Guid> childrenKeys);

        // Assert
        Assert.AreEqual(2, childrenKeys.Count());
    }

    /// <summary>
    /// Verifies that attempting to get descendants from a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Descendants_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.TryGetDescendantsKeys(nonExistingKey, out IEnumerable<Guid> descendantsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(descendantsKeys);
        });
    }

    /// <summary>
    /// Verifies that the navigation service can successfully retrieve all descendant content item keys for a given parent content key.
    /// </summary>
    /// <param name="parentKey">The unique key identifying the parent content item.</param>
    /// <param name="descendantsCount">The expected number of descendant content items to be retrieved.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 8)] // Root - Child 1, Grandchild 1, Grandchild 2, Child 2, Grandchild 3, Great-grandchild 1, Child 3, Grandchild 4
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 1 - Grandchild 1, Grandchild 2
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 0)] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 0)] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Child 2 - Grandchild 3, Great-grandchild 1
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 1)] // Grandchild 3 - Great-grandchild 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 0)] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 3 - Grandchild 4
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 0)] // Grandchild 4
    public void Can_Get_Descendants_From_Existing_Content_Key(Guid parentKey, int descendantsCount)
    {
        // Act
        var result = _navigationService.TryGetDescendantsKeys(parentKey, out IEnumerable<Guid> descendantsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(descendantsCount, descendantsKeys.Count());
        });
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "E856AC03-C23E-4F63-9AA9-681B42A58573", "A1B1B217-B02F-4307-862C-A5E22DB729EB", "60E0E5C4-084E-4144-A560-7393BEAD2E96", "D63C1621-C74A-4106-8587-817DEE5FB732", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", "B606E3FF-E070-4D46-8CB9-D31352029FDF", "F381906C-223C-4466-80F7-B63B4EE073F8", })] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "E856AC03-C23E-4F63-9AA9-681B42A58573", "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new string[0])] // Grandchild 1
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", new[] { "D63C1621-C74A-4106-8587-817DEE5FB732", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", new[] { "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", new string[0])] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", new[] { "F381906C-223C-4466-80F7-B63B4EE073F8" })] // Child 3
    public void Can_Get_Descendants_From_Existing_Content_Key_In_Their_Order_Of_Creation(Guid parentKey, string[] descendants)
    {
        // Arrange
        Guid[] expectedDescendants = Array.ConvertAll(descendants, Guid.Parse);

        // Act
        _navigationService.TryGetDescendantsKeys(parentKey, out IEnumerable<Guid> descendantsKeys);
        List<Guid> descendantsList = descendantsKeys.ToList();

        // Assert
        for (var i = 0; i < expectedDescendants.Length; i++)
        {
            Assert.AreEqual(expectedDescendants[i], descendantsList.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that attempting to get descendants of a non-existing content type alias returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Descendants_Of_Type_From_Non_Existing_Content_Type_Alias()
    {
        // Arrange
        Guid parentKey = Root;
        var nonExistingContentTypeAlias = string.Empty;

        // Act
        var result = _navigationService.TryGetDescendantsKeysOfType(parentKey, nonExistingContentTypeAlias, out IEnumerable<Guid> descendantsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(descendantsKeys);
        });
    }

    /// <summary>
    /// Tests that attempting to get descendants of a specific content type from a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Descendants_Of_Type_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // Act
        var result = _navigationService.TryGetDescendantsKeysOfType(nonExistingKey, contentTypeAlias, out IEnumerable<Guid> descendantsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(descendantsKeys);
        });
    }

    /// <summary>
    /// Tests that the content navigation service can retrieve the descendants of a specified content type alias for a given parent content item's key.
    /// </summary>
    /// <param name="parentKey">The key of the parent content item for which to retrieve descendants.</param>
    /// <param name="descendantsCount">The expected number of descendant items of the specified content type alias.</param>
    [Test]
    [TestCase(
        "E48DD82A-7059-418E-9B82-CDD5205796CF",
        8)] // Root - Child 1, Grandchild 1, Grandchild 2, Child 2, Grandchild 3, Great-grandchild 1, Child 3, Grandchild 4
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 1 - Grandchild 1, Grandchild 2
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 0)] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 0)] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Child 2 - Grandchild 3, Great-grandchild 1
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 1)] // Grandchild 3 - Great-grandchild 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 0)] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 3 - Grandchild 4
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 0)] // Grandchild 4
    public void Can_Get_Descendants_Of_Type(Guid parentKey, int descendantsCount)
    {
        // Arrange
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Act
        var result = _navigationService.TryGetDescendantsKeysOfType(parentKey, contentTypeAlias, out IEnumerable<Guid> descendantsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(descendantsCount, descendantsKeys.Count());
        });
    }

    /// <summary>
    /// Tests that the method Can_Get_Descendants_Of_Type correctly filters descendants by content type.
    /// It verifies that only descendants of the specified content type alias are returned.
    /// </summary>
    [Test]
    public void Can_Get_Descendants_Of_Type_Filters_Result()
    {
        // Arrange
        Guid parentKey = Child2;
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new descendants with different content type under Child2
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey, Grandchild3);
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey, GreatGrandchild1);

        // Act
        _navigationService.TryGetDescendantsKeysOfType(parentKey, anotherContentTypeAlias, out IEnumerable<Guid> descendantsKeysOfType);
        var descendantsOfTypeCount = descendantsKeysOfType.Count();

        // Assert
        // Retrieve descendants without filtering to compare
        _navigationService.TryGetDescendantsKeys(parentKey, out IEnumerable<Guid> allDescendantsKeys);
        var allDescendantsCount = allDescendantsKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsTrue(allDescendantsCount > descendantsOfTypeCount);
            Assert.AreEqual(4, allDescendantsCount);
            Assert.AreEqual(2, descendantsOfTypeCount);
        });
    }

    /// <summary>
    /// Verifies that the <c>TryGetDescendantsKeysOfType</c> method filters descendants by the specified content type alias
    /// and that the returned descendant keys maintain the order in which they were created.
    /// This test ensures both correct filtering and preservation of creation order for descendants of a given type.
    /// </summary>
    [Test]
    public void Can_Get_Descendants_Of_Type_Filters_Result_And_Maintains_Their_Order_Of_Creation()
    {
        // Arrange
        Guid parentKey = Child2;
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new descendants with different content type under Child2
        Guid greatGreatGrandchild2 = Guid.NewGuid();
        Guid greatGreatGrandchild3 = Guid.NewGuid();
        _navigationService.Add(greatGreatGrandchild2, anotherContentTypeKey, Grandchild3);
        _navigationService.Add(greatGreatGrandchild3, anotherContentTypeKey, Grandchild3);

        var expectedDescendantsOrder = new List<Guid> { greatGreatGrandchild2, greatGreatGrandchild3 };

        // Act
        _navigationService.TryGetDescendantsKeysOfType(parentKey, anotherContentTypeAlias, out IEnumerable<Guid> descendantsOfType);

        // Assert
        // Check that the order matches what is expected
        Assert.IsTrue(expectedDescendantsOrder.SequenceEqual(descendantsOfType));
    }

    /// <summary>
    /// Tests that attempting to get ancestors from a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Ancestors_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.TryGetAncestorsKeys(nonExistingKey, out IEnumerable<Guid> ancestorsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(ancestorsKeys);
        });
    }

    /// <summary>
    /// Unit test that verifies ancestors can be retrieved for a given content key.
    /// Asserts that the correct number of ancestor keys is returned for the specified child content item.
    /// </summary>
    /// <param name="childKey">The key of the child content item for which to retrieve ancestor keys.</param>
    /// <param name="ancestorsCount">The expected number of ancestor keys to be returned.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 0)] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 1)] // Child 1 - Root
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 2)] // Grandchild 1 - Child 1, Root
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 2)] // Grandchild 2 - Child 1, Root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 1)] // Child 2 - Root
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 2)] // Grandchild 3 - Child 2, Root
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 3)] // Great-grandchild 1 - Grandchild 3, Child 2, Root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 3 - Root
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 2)] // Grandchild 4 - Child 3, Root
    public void Can_Get_Ancestors_From_Existing_Content_Key(Guid childKey, int ancestorsCount)
    {
        // Act
        var result = _navigationService.TryGetAncestorsKeys(childKey, out IEnumerable<Guid> ancestorsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(ancestorsCount, ancestorsKeys.Count());
        });
    }

    /// <summary>
    /// Verifies that the ancestors of a given content item, identified by its key, can be retrieved in the correct order of creation.
    /// </summary>
    /// <param name="childKey">The GUID key of the content item whose ancestors are to be retrieved.</param>
    /// <param name="ancestors">An array of GUID keys (as strings) representing the expected ancestors, ordered from closest to furthest ancestor.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new string[0])] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Grandchild 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", new[] { "D63C1621-C74A-4106-8587-817DEE5FB732", "60E0E5C4-084E-4144-A560-7393BEAD2E96", "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Great-grandchild 1
    public void Can_Get_Ancestors_From_Existing_Content_Key_In_Correct_Order_Of_Creation(Guid childKey, string[] ancestors)
    {
        // Arrange
        Guid[] expectedAncestors = Array.ConvertAll(ancestors, Guid.Parse);

        // Act
        _navigationService.TryGetAncestorsKeys(childKey, out IEnumerable<Guid> ancestorsKeys);
        List<Guid> ancestorsList = ancestorsKeys.ToList();

        // Assert
        for (var i = 0; i < expectedAncestors.Length; i++)
        {
            Assert.AreEqual(expectedAncestors[i], ancestorsList.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that getting ancestors of a non-existing content type alias returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Ancestors_Of_Type_From_Non_Existing_Content_Type_Alias()
    {
        // Arrange
        Guid childKey = GreatGrandchild1;
        var nonExistingContentTypeAlias = string.Empty;

        // Act
        var result = _navigationService.TryGetAncestorsKeysOfType(childKey, nonExistingContentTypeAlias, out IEnumerable<Guid> ancestorsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(ancestorsKeys);
        });
    }

    /// <summary>
    /// Tests that attempting to get ancestors of a specific type from a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Ancestors_Of_Type_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // Act
        var result = _navigationService.TryGetAncestorsKeysOfType(nonExistingKey, contentTypeAlias, out IEnumerable<Guid> ancestorsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(ancestorsKeys);
        });
    }

    /// <summary>
    /// Verifies that the navigation service can retrieve the correct number of ancestor content items of a specified content type for a given child content item's key.
    /// </summary>
    /// <param name="childKey">The unique identifier (GUID) of the child content item whose ancestors are being retrieved.</param>
    /// <param name="ancestorsCount">The expected number of ancestor items of the specified content type.</param>
    /// <remarks>
    /// This test ensures that <see cref="TestContentNavigationService.TryGetAncestorsKeysOfType"/> returns the correct ancestor keys for the given content type alias.
    /// </remarks>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 0)] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 1)] // Child 1 - Root
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 2)] // Grandchild 1 - Child 1, Root
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 2)] // Grandchild 2 - Child 1, Root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 1)] // Child 2 - Root
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 2)] // Grandchild 3 - Child 2, Root
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 3)] // Great-grandchild 1 - Grandchild 3, Child 2, Root
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 3 - Root
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 2)] // Grandchild 4 - Child 3, Root
    public void Can_Get_Ancestors_Of_Type(Guid childKey, int ancestorsCount)
    {
        // Arrange
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Act
        var result = _navigationService.TryGetAncestorsKeysOfType(childKey, contentTypeAlias, out IEnumerable<Guid> ancestorsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(ancestorsCount, ancestorsKeys.Count());
        });
    }

    /// <summary>
    /// Tests that retrieving ancestors of a specific content type filters the results correctly.
    /// </summary>
    [Test]
    public void Can_Get_Ancestors_Of_Type_Filters_Result()
    {
        // Arrange
        Guid childKey = Guid.NewGuid();
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new items with different content type under Grandchild 1
        var greatGrandchild2 = Guid.NewGuid();
        _navigationService.Add(greatGrandchild2, anotherContentTypeKey, Grandchild1);
        _navigationService.Add(childKey, anotherContentTypeKey, greatGrandchild2);

        // Act
        _navigationService.TryGetAncestorsKeysOfType(childKey, anotherContentTypeAlias, out IEnumerable<Guid> ancestorsKeysOfType);
        var ancestorsOfTypeCount = ancestorsKeysOfType.Count();

        // Assert
        // Retrieve all ancestors without filtering to compare
        _navigationService.TryGetAncestorsKeys(childKey, out IEnumerable<Guid> allAncestorsKeys);
        var allAncestorsCount = allAncestorsKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsTrue(allAncestorsCount > ancestorsOfTypeCount);
            Assert.AreEqual(4, allAncestorsCount);
            Assert.AreEqual(1, ancestorsOfTypeCount);
        });
    }

    /// <summary>
    /// Tests that trying to get siblings of a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Siblings_Of_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.TryGetSiblingsKeys(nonExistingKey, out IEnumerable<Guid> siblingsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(siblingsKeys);
        });
    }

    /// <summary>
    /// Tests that the navigation service can retrieve sibling content keys of an existing content key,
    /// excluding the content key itself from the siblings list.
    /// </summary>
    [Test]
    public void Can_Get_Siblings_Of_Existing_Content_Key_Without_Self()
    {
        // Arrange
        Guid nodeKey = Child1;

        // Act
        var result = _navigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> siblingsKeys);
        List<Guid> siblingsList = siblingsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.IsNotEmpty(siblingsList);
            Assert.IsFalse(siblingsList.Contains(nodeKey));
        });
    }

    /// <summary>
    /// Tests that the service can retrieve sibling content keys of an existing content key at the content root.
    /// </summary>
    [Test]
    public void Can_Get_Siblings_Of_Existing_Content_Key_At_Content_Root()
    {
        // Arrange
        Guid anotherRoot = Guid.NewGuid();
        _navigationService.Add(anotherRoot, ContentType);

        // Act
        _navigationService.TryGetSiblingsKeys(anotherRoot, out IEnumerable<Guid> siblingsKeys);
        List<Guid> siblingsList = siblingsKeys.ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(siblingsList);
            Assert.AreEqual(1, siblingsList.Count);
            Assert.AreEqual(Root, siblingsList.First());
        });
    }

    /// <summary>
    /// Tests that siblings of the content item identified by the given key can be retrieved correctly.
    /// </summary>
    /// <param name="key">The unique identifier of the content item to find siblings for.</param>
    /// <param name="siblingsCount">The expected number of siblings for the given content key.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 0)] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 1 - Child 2, Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 1)] // Grandchild 1 - Grandchild 2
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 1)] // Grandchild 2 - Grandchild 1
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Child 2 - Child 1, Child 3
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 0)] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 0)] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 2)] // Child 3 - Child 1, Child 2
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 0)] // Grandchild 4
    public void Can_Get_Siblings_Of_Existing_Content_Key(Guid key, int siblingsCount)
    {
        // Act
        var result = _navigationService.TryGetSiblingsKeys(key, out IEnumerable<Guid> siblingsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(siblingsCount, siblingsKeys.Count());
        });
    }

    /// <summary>
    /// Verifies that the siblings of a specified content item can be retrieved in the order they were created.
    /// </summary>
    /// <param name="childKey">The unique key (GUID) of the content item whose siblings are to be retrieved.</param>
    /// <param name="siblings">An array of sibling keys (as strings) expected to be returned, in their creation order.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new string[0])] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF" })] // Child 1 - Child 2, Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new[] { "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Grandchild 1 - Grandchild 2
    public void Can_Get_Siblings_Of_Existing_Content_Key_In_Their_Order_Of_Creation(Guid childKey, string[] siblings)
    {
        // Arrange
        Guid[] expectedSiblings = Array.ConvertAll(siblings, Guid.Parse);

        // Act
        _navigationService.TryGetSiblingsKeys(childKey, out IEnumerable<Guid> siblingsKeys);
        List<Guid> siblingsList = siblingsKeys.ToList();

        // Assert
        for (var i = 0; i < expectedSiblings.Length; i++)
        {
            Assert.AreEqual(expectedSiblings[i], siblingsList.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that trying to get siblings of a non-existing content type alias returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Siblings_Of_Type_From_Non_Existing_Content_Type_Alias()
    {
        // Arrange
        Guid nodeKey = Child1;
        var nonExistingContentTypeAlias = string.Empty;

        // Act
        var result = _navigationService.TryGetSiblingsKeysOfType(nodeKey, nonExistingContentTypeAlias, out IEnumerable<Guid> siblingsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(siblingsKeys);
        });
    }

    /// <summary>
    /// Tests that attempting to get siblings of a specific content type from a non-existing content key returns false and an empty collection.
    /// </summary>
    [Test]
    public void Cannot_Get_Siblings_Of_Type_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // Act
        var result = _navigationService.TryGetSiblingsKeysOfType(nonExistingKey, contentTypeAlias, out IEnumerable<Guid> siblingsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsEmpty(siblingsKeys);
        });
    }

    /// <summary>
    /// Verifies that siblings of a given content item, filtered by a specific content type, can be retrieved correctly.
    /// </summary>
    /// <param name="key">The unique identifier of the content item whose siblings of the specified type are to be found.</param>
    /// <param name="siblingsCount">The expected number of siblings of the same content type as the specified content item.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 0)] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 1 - Child 2, Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 1)] // Grandchild 1 - Grandchild 2
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 1)] // Grandchild 2 - Grandchild 1
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Child 2 - Child 1, Child 3
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 0)] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 0)] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 2)] // Child 3 - Child 1, Child 2
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 0)] // Grandchild 4
    public void Can_Get_Siblings_Of_Type(Guid key, int siblingsCount)
    {
        // Arrange
        const string contentTypeAlias = "contentPage";

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Act
        var result = _navigationService.TryGetSiblingsKeysOfType(key, contentTypeAlias, out IEnumerable<Guid> siblingsKeys);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(siblingsCount, siblingsKeys.Count());
        });
    }

    /// <summary>
    /// Tests that the method Can_Get_Siblings_Of_Type correctly filters siblings by content type.
    /// It verifies that only siblings of the specified content type alias are returned.
    /// </summary>
    [Test]
    public void Can_Get_Siblings_Of_Type_Filters_Result()
    {
        // Arrange
        Guid nodeKey = Child1;
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new children with different content type under Root
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey, Root);
        _navigationService.Add(Guid.NewGuid(), anotherContentTypeKey, Root);

        // Act
        _navigationService.TryGetSiblingsKeysOfType(nodeKey, anotherContentTypeAlias, out IEnumerable<Guid> siblingsKeysOfType);
        var siblingsOfTypeCount = siblingsKeysOfType.Count();

        // Assert
        // Retrieve all siblings without filtering to compare
        _navigationService.TryGetSiblingsKeys(nodeKey, out IEnumerable<Guid> allSiblingsKeys);
        var allSiblingsCount = allSiblingsKeys.Count();

        Assert.Multiple(() =>
        {
            Assert.IsTrue(allSiblingsCount > siblingsOfTypeCount);
            Assert.AreEqual(4, allSiblingsCount);
            Assert.AreEqual(2, siblingsOfTypeCount);
        });
    }

    /// <summary>
    /// Tests that getting siblings of a specific content type filters the results correctly
    /// and maintains the order of creation of those siblings.
    /// </summary>
    [Test]
    public void Can_Get_Siblings_Of_Type_Filters_Result_And_Maintains_Their_Order_Of_Creation()
    {
        // Arrange
        Guid nodeKey = Child1;
        const string contentTypeAlias = "contentPage";
        const string anotherContentTypeAlias = "anotherContentPage";
        Guid anotherContentTypeKey = Guid.NewGuid();

        var contentTypeMock = new Mock<IContentType>();
        contentTypeMock.SetupGet(x => x.Alias).Returns(contentTypeAlias);
        contentTypeMock.SetupGet(x => x.Key).Returns(ContentType);

        var anotherContentTypeMock = new Mock<IContentType>();
        anotherContentTypeMock.SetupGet(x => x.Alias).Returns(anotherContentTypeAlias);
        anotherContentTypeMock.SetupGet(x => x.Key).Returns(anotherContentTypeKey);

        var contentTypeServiceMock = new Mock<IContentTypeService>();
        contentTypeServiceMock.Setup(x => x.GetAll()).Returns(new[] { contentTypeMock.Object, anotherContentTypeMock.Object });

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeServiceMock.Object);

        // We need to re-create the test data since we use new mock
        CreateTestData();

        // Adding 2 new children with different content type under Root
        Guid child4 = Guid.NewGuid();
        Guid child5 = Guid.NewGuid();
        _navigationService.Add(child4, anotherContentTypeKey, Root);
        _navigationService.Add(child5, anotherContentTypeKey, Root);

        var expectedSiblingsOrder = new List<Guid> { child4, child5 };

        // Act
        _navigationService.TryGetSiblingsKeysOfType(nodeKey, anotherContentTypeAlias, out IEnumerable<Guid> siblingsKeysOfType);

        // Assert
        // Check that the order matches what is expected
        Assert.IsTrue(expectedSiblingsOrder.SequenceEqual(siblingsKeysOfType));
    }

    /// <summary>
    /// Tests that getting the level from a non-existing content key returns false and null.
    /// </summary>
    [Test]
    public void Cannot_Get_Level_From_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.TryGetLevel(nonExistingKey, out var level);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsNull(level);
        });
    }

    /// <summary>
    /// Tests that the level of content can be retrieved correctly from an existing content key.
    /// </summary>
    /// <param name="key">The unique identifier of the content item.</param>
    /// <param name="expectedLevel">The expected level of the content item in the hierarchy.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", 1)] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 3)] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", 3)] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", 3)] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 4)] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 2)] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", 3)] // Grandchild 4
    public void Can_Get_Level_From_Existing_Content_Key(Guid key, int expectedLevel)
    {
        // Act
        var result = _navigationService.TryGetLevel(key, out var level);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.IsNotNull(level);
            Assert.AreEqual(expectedLevel, level);
        });
    }

    /// <summary>
    /// Tests that moving a node to the bin fails when the content key does not exist.
    /// </summary>
    [Test]
    public void Cannot_Move_Node_To_Bin_When_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.MoveToBin(nonExistingKey);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that a node identified by the given key can be moved to the bin.
    /// </summary>
    /// <param name="keyOfNodeToRemove">The unique identifier of the node to move to the bin.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573")] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Can_Move_Node_To_Bin(Guid keyOfNodeToRemove)
    {
        // Act
        var result = _navigationService.MoveToBin(keyOfNodeToRemove);

        // Assert
        Assert.IsTrue(result);

        var nodeExists = _navigationService.TryGetParentKey(keyOfNodeToRemove, out Guid? parentKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExists);
            Assert.IsNull(parentKey);
        });
    }

    /// <summary>
    /// Tests that moving a node to the bin removes the node and all its descendants.
    /// </summary>
    /// <param name="keyOfNodeToRemove">The key of the node to remove.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Moving_Node_To_Bin_Removes_Its_Descendants_As_Well(Guid keyOfNodeToRemove)
    {
        // Arrange
        _navigationService.TryGetDescendantsKeys(keyOfNodeToRemove, out IEnumerable<Guid> initialDescendantsKeys);

        // Act
        var result = _navigationService.MoveToBin(keyOfNodeToRemove);

        // Assert
        Assert.IsTrue(result);

        _navigationService.TryGetDescendantsKeys(keyOfNodeToRemove, out IEnumerable<Guid> descendantsKeys);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, descendantsKeys.Count());

            foreach (Guid descendant in initialDescendantsKeys)
            {
                var descendantExists = _navigationService.TryGetParentKey(descendant, out _);
                Assert.IsFalse(descendantExists);
            }
        });
    }

    /// <summary>
    /// Tests that moving a node to the bin adds it to the recycle bin root.
    /// </summary>
    /// <param name="keyOfNodeToRemove">The key of the node to remove and move to the bin.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573")] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Moving_Node_To_Bin_Adds_It_To_Recycle_Bin_Root(Guid keyOfNodeToRemove)
    {
        // Act
        _navigationService.MoveToBin(keyOfNodeToRemove);

        // Assert
        var nodeExistsInBin = _navigationService.TryGetParentKeyInBin(keyOfNodeToRemove, out Guid? parentKeyInBin);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(nodeExistsInBin);
            Assert.IsNull(parentKeyInBin);
        });
    }

    /// <summary>
    /// Tests that moving a node to the recycle bin adds it to the recycle bin root as the last item.
    /// </summary>
    /// <param name="keyOfNodeToRemove">The key of the node to move to the recycle bin.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public void Moving_Node_To_Bin_Adds_It_To_Recycle_Bin_Root_As_The_Last_Item(Guid keyOfNodeToRemove)
    {
        // Arrange
        Guid nodeInRecycleBin1 = Grandchild1;
        Guid nodeInRecycleBin2 = Child3;
        _navigationService.MoveToBin(nodeInRecycleBin1);
        _navigationService.MoveToBin(nodeInRecycleBin2);

        // Act
        _navigationService.MoveToBin(keyOfNodeToRemove);

        // Assert
        _navigationService.TryGetSiblingsKeysInBin(nodeInRecycleBin1, out IEnumerable<Guid> siblingsInBin);

        Assert.AreEqual(siblingsInBin.Last(), keyOfNodeToRemove);
    }

    /// <summary>
    /// Tests that moving a node to the recycle bin also moves all its descendants to the recycle bin.
    /// </summary>
    /// <param name="keyOfNodeToRemove">The key of the node to move to the recycle bin.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public void Moving_Node_To_Bin_Adds_Its_Descendants_To_Recycle_Bin_As_Well(Guid keyOfNodeToRemove)
    {
        // Arrange
        _navigationService.TryGetDescendantsKeys(keyOfNodeToRemove, out IEnumerable<Guid> initialDescendantsKeys);
        List<Guid> initialDescendantsList = initialDescendantsKeys.ToList();

        // Act
        _navigationService.MoveToBin(keyOfNodeToRemove);

        // Assert
        var nodeExistsInBin = _navigationService.TryGetDescendantsKeysInBin(keyOfNodeToRemove, out IEnumerable<Guid> descendantsKeysInBin);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(nodeExistsInBin);
            CollectionAssert.AreEqual(initialDescendantsList, descendantsKeysInBin);

            foreach (Guid descendant in initialDescendantsList)
            {
                _navigationService.TryGetParentKeyInBin(descendant, out Guid? parentKeyInBin);
                Assert.IsNotNull(parentKeyInBin); // The descendant kept its initial parent
            }
        });
    }

    /// <summary>
    /// Tests that adding a node fails when the specified parent node does not exist.
    /// </summary>
    [Test]
    public void Cannot_Add_Node_When_Parent_Does_Not_Exist()
    {
        // Arrange
        var newNodeKey = Guid.NewGuid();
        var nonExistentParentKey = Guid.NewGuid();

        // Act
        var result = _navigationService.Add(newNodeKey, ContentType, nonExistentParentKey);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that adding a node with a key that already exists is not allowed.
    /// </summary>
    [Test]
    public void Cannot_Add_When_Node_With_The_Same_Key_Already_Exists()
    {
        // Act
        var result = _navigationService.Add(Child1, ContentType);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that a new node can be added to the content root.
    /// </summary>
    [Test]
    public void Can_Add_Node_To_Content_Root()
    {
        // Arrange
        var newNodeKey = Guid.NewGuid();

        // Act
        var result = _navigationService.Add(newNodeKey, ContentType); // parentKey is null

        // Assert
        Assert.IsTrue(result);

        var nodeExists = _navigationService.TryGetParentKey(newNodeKey, out Guid? parentKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(nodeExists);
            Assert.IsNull(parentKey);
        });
    }

    /// <summary>
    /// Tests that a new node can be added to the specified parent node.
    /// </summary>
    /// <param name="parentKey">The key of the parent node to which the new node will be added.</param>
    [Test]
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public void Can_Add_Node_To_Parent(Guid parentKey)
    {
        // Arrange
        var newNodeKey = Guid.NewGuid();
        _navigationService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> currentChildrenKeys);
        var currentChildrenCount = currentChildrenKeys.Count();

        // Act
        var result = _navigationService.Add(newNodeKey, ContentType, parentKey);

        // Assert
        Assert.IsTrue(result);

        _navigationService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> newChildrenKeys);
        var newChildrenList = newChildrenKeys.ToList();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(currentChildrenCount + 1, newChildrenList.Count);
            Assert.IsTrue(newChildrenList.Any(childKey => childKey == newNodeKey));
        });
    }

    /// <summary>
    /// Verifies that when a node is added to a specified parent, it is inserted as the last child of that parent in the navigation structure.
    /// </summary>
    /// <param name="parentKey">The <see cref="Guid"/> identifying the parent node to which the new node will be added.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573")] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Adding_Node_To_Parent_Adds_It_As_The_Last_Child(Guid parentKey)
    {
        // Arrange
        var newNodeKey = Guid.NewGuid();

        // Act
        _navigationService.Add(newNodeKey, ContentType, parentKey);

        // Assert
        _navigationService.TryGetChildrenKeys(parentKey, out IEnumerable<Guid> childrenKeys);

        Assert.AreEqual(newNodeKey, childrenKeys.Last());
    }

    /// <summary>
    /// Tests that moving a node to a target parent that does not exist fails.
    /// </summary>
    [Test]
    public void Cannot_Move_Node_When_Target_Parent_Does_Not_Exist()
    {
        // Arrange
        Guid nodeToMove = Child1;
        var nonExistentTargetParentKey = Guid.NewGuid();

        // Act
        var result = _navigationService.Move(nodeToMove, nonExistentTargetParentKey);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that attempting to move a node that does not exist returns false.
    /// </summary>
    [Test]
    public void Cannot_Move_Node_That_Does_Not_Exist()
    {
        // Arrange
        var nonExistentNodeKey = Guid.NewGuid();
        Guid targetParentKey = Child1;

        // Act
        var result = _navigationService.Move(nonExistentNodeKey, targetParentKey);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that a node cannot be moved to itself.
    /// </summary>
    [Test]
    public void Cannot_Move_Node_To_Itself()
    {
        // Arrange
        Guid nodeToMove = Child1;

        // Act
        var result = _navigationService.Move(nodeToMove, nodeToMove);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a content node can be moved to the root of the content tree.
    /// Ensures that after moving, the node's parent is set to null, indicating it is at the root level.
    /// </summary>
    [Test]
    public void Can_Move_Node_To_Content_Root()
    {
        // Arrange
        Guid nodeToMove = Child1;

        // Act
        var result = _navigationService.Move(nodeToMove); // parentKey is null

        // Assert
        Assert.IsTrue(result);

        // Verify the node's new parent is null (moved to content root)
        _navigationService.TryGetParentKey(nodeToMove, out Guid? newParentKey);

        Assert.IsNull(newParentKey);
    }

    /// <summary>
    /// Tests that a node can be moved to an existing target parent successfully.
    /// Verifies that the move operation returns true and the node's parent key is updated accordingly.
    /// </summary>
    [Test]
    public void Can_Move_Node_To_Existing_Target_Parent()
    {
        // Arrange
        Guid nodeToMove = Grandchild4;
        Guid targetParentKey = Child1;

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        // Verify the node's new parent is updated
        _navigationService.TryGetParentKey(nodeToMove, out Guid? newParentKey);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(newParentKey);
            Assert.AreEqual(targetParentKey, newParentKey);
        });
    }

    /// <summary>
    /// Tests that when a node is moved, its parent is correctly updated.
    /// </summary>
    [Test]
    public void Moved_Node_Has_Updated_Parent()
    {
        // Arrange
        Guid nodeToMove = Grandchild1;
        Guid targetParentKey = Child2;
        _navigationService.TryGetParentKey(nodeToMove, out Guid? oldParentKey);

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        // Verify the node's new parent is updated
        _navigationService.TryGetParentKey(nodeToMove, out Guid? newParentKey);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(newParentKey);
            Assert.AreEqual(targetParentKey, newParentKey);

            // Verify that the new parent is different from the old one
            Assert.AreNotEqual(oldParentKey, targetParentKey);
        });
    }

    /// <summary>
    /// Tests that when a node is moved, it is removed from its current parent's children list.
    /// </summary>
    [Test]
    public void Moved_Node_Is_Removed_From_Its_Current_Parent()
    {
        // Arrange
        Guid nodeToMove = Grandchild3;
        Guid targetParentKey = Child3;
        _navigationService.TryGetParentKey(nodeToMove, out Guid? oldParentKey);
        _navigationService.TryGetChildrenKeys(oldParentKey!.Value, out IEnumerable<Guid> oldParentChildrenKeys);
        var oldParentChildrenCount = oldParentChildrenKeys.Count();

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        // Verify the node is removed from its old parent's children list
        _navigationService.TryGetChildrenKeys(oldParentKey.Value, out IEnumerable<Guid> childrenKeys);
        List<Guid> childrenList = childrenKeys.ToList();

        Assert.Multiple(() =>
        {
            CollectionAssert.DoesNotContain(childrenList, nodeToMove);
            Assert.AreEqual(oldParentChildrenCount - 1, childrenList.Count);
        });
    }

    /// <summary>
    /// Tests that when a node is moved, it is correctly added to its new parent's children list.
    /// </summary>
    [Test]
    public void Moved_Node_Is_Added_To_Its_New_Parent()
    {
        // Arrange
        Guid nodeToMove = Grandchild2;
        Guid targetParentKey = Child2;
        _navigationService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> targetParentChildrenKeys);
        var targetParentChildrenCount = targetParentChildrenKeys.Count();

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        // Verify the node is added to its new parent's children list
        _navigationService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> childrenKeys);
        List<Guid> childrenList = childrenKeys.ToList();

        Assert.Multiple(() =>
        {
            CollectionAssert.Contains(childrenList, nodeToMove);
            Assert.AreEqual(targetParentChildrenCount + 1, childrenList.Count);
        });
    }

    /// <summary>
    /// Tests that moving a node to a specified parent adds it as the last child of that parent.
    /// </summary>
    /// <param name="targetParentKey">The key of the target parent node to move the node to.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573")] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public void Moving_Node_To_Parent_Adds_It_As_The_Last_Child(Guid targetParentKey)
    {
        // Arrange
        Guid nodeToMove = Grandchild4;

        // Act
        _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        _navigationService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> childrenKeys);

        Assert.AreEqual(nodeToMove, childrenKeys.Last());
    }

    /// <summary>
    /// Verifies that moving a content node to a new parent or to the root does not change the number of its descendants.
    /// </summary>
    /// <param name="nodeToMove">The unique identifier of the node to be moved.</param>
    /// <param name="sortOrder">The sort order position to move the node to within the new parent.</param>
    /// <param name="targetParentKey">The unique identifier of the new parent node, or <c>null</c> to move the node to the root.</param>
    /// <param name="initialDescendantsCount">The expected number of descendants the node should have after the move (should match the count before the move).</param>
    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 1, "60E0E5C4-084E-4144-A560-7393BEAD2E96", 0)] // Grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 1, null, 1)] // Child 3 to content root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 2, "C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 2 to Child 1
    public void Moved_Node_Has_The_Same_Amount_Of_Descendants(Guid nodeToMove, int sortOrder, Guid? targetParentKey, int initialDescendantsCount)
    {
        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        // Verify that the number of descendants remain the same after moving the node
        _navigationService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> descendantsKeys);
        var descendantsCountAfterMove = descendantsKeys.Count();

        Assert.AreEqual(initialDescendantsCount, descendantsCountAfterMove);
    }

    /// <summary>
    /// Verifies that when a node with descendants is moved to a new parent, the target parent's descendant count is updated correctly.
    /// The test ensures that the target parent's descendant count increases by the number of descendants of the moved node plus the node itself.
    /// </summary>
    /// <param name="nodeToMove">The key of the node to move.</param>
    /// <param name="sortOrder">The sort order for the move operation (not directly asserted in this test).</param>
    /// <param name="targetParentKey">The key of the target parent node to which the node is moved.</param>
    /// <param name="initialDescendantsCountOfTargetParent">The initial count of descendants of the target parent before the move.</param>
    [Test]
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", 0, "A1B1B217-B02F-4307-862C-A5E22DB729EB", 0)] // Child 3 to Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", 1, "B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 2 to Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", 1, "60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Grandchild 1 to Child 2
    public void Number_Of_Target_Parent_Descendants_Updates_When_Moving_Node_With_Descendants(Guid nodeToMove, int sortOrder, Guid targetParentKey, int initialDescendantsCountOfTargetParent)
    {
        // Arrange
        // Get the number of descendants of the node to move
        _navigationService.TryGetDescendantsKeys(nodeToMove, out IEnumerable<Guid> descendantsKeys);
        var descendantsCountOfNodeToMove = descendantsKeys.Count();

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        _navigationService.TryGetDescendantsKeys(targetParentKey, out IEnumerable<Guid> updatedTargetParentDescendantsKeys);
        var updatedDescendantsCountOfTargetParent = updatedTargetParentDescendantsKeys.Count();

        // Verify the number of descendants of the target parent has increased by the number of descendants of the moved node plus the node itself
        Assert.AreEqual(initialDescendantsCountOfTargetParent + descendantsCountOfNodeToMove + 1, updatedDescendantsCountOfTargetParent);
    }

    /// <summary>
    /// Tests that a node cannot be restored when the target parent does not exist.
    /// </summary>
    [Test]
    public void Cannot_Restore_Node_When_Target_Parent_Does_Not_Exist()
    {
        // Arrange
        Guid nodeToRestore = Grandchild1;
        var nonExistentTargetParentKey = Guid.NewGuid();
        _navigationService.MoveToBin(nodeToRestore);

        // Act
        var result = _navigationService.RestoreFromBin(nodeToRestore, nonExistentTargetParentKey);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that attempting to restore a node that does not exist returns false.
    /// </summary>
    [Test]
    public void Cannot_Restore_Node_That_Does_Not_Exist()
    {
        // Arrange
        Guid notDeletedNodeKey = Grandchild4;
        Guid targetParentKey = Child3;

        // Act
        var result = _navigationService.RestoreFromBin(notDeletedNodeKey, targetParentKey);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests that a node can be restored to an existing target parent.
    /// </summary>
    /// <param name="nodeToRestore">The key of the node to restore.</param>
    /// <param name="targetParentKey">The key of the target parent node, or null if restoring to root.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", null)] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Grandchild 1
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", "D63C1621-C74A-4106-8587-817DEE5FB732")] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", "B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Grandchild 4
    public void Can_Restore_Node_To_Existing_Target_Parent(Guid nodeToRestore, Guid? targetParentKey)
    {
        // Arrange
        _navigationService.MoveToBin(nodeToRestore);

        // Act
        var result = _navigationService.RestoreFromBin(nodeToRestore, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        // Verify the node's new parent is updated
        _navigationService.TryGetParentKey(nodeToRestore, out Guid? parentKeyAfterRestore);

        Assert.Multiple(() =>
        {
            if (targetParentKey is null)
            {
                Assert.IsNull(parentKeyAfterRestore);
            }
            else
            {
                Assert.IsNotNull(parentKeyAfterRestore);
            }

            Assert.AreEqual(targetParentKey, parentKeyAfterRestore);
        });
    }

    /// <summary>
    /// Verifies that when a node is restored from the recycle bin, it is correctly added to the children of the specified target parent node.
    /// </summary>
    /// <param name="nodeToRestore">The unique identifier (GUID) of the node to be restored from the recycle bin.</param>
    /// <param name="targetParentKey">The unique identifier (GUID) of the target parent node to which the restored node should be added as a child.</param>
    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Grandchild 1 to Child 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Great-grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 3 to Root
    public void Restored_Node_Is_Added_To_Its_Target_Parent(Guid nodeToRestore, Guid targetParentKey)
    {
        // Arrange
        _navigationService.MoveToBin(nodeToRestore);
        _navigationService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> targetParentChildrenKeys);
        var targetParentChildrenCount = targetParentChildrenKeys.Count();

        // Act
        var result = _navigationService.RestoreFromBin(nodeToRestore, targetParentKey);

        // Assert
        Assert.IsTrue(result);

        // Verify the node is added to its target parent's children list
        _navigationService.TryGetChildrenKeys(targetParentKey, out IEnumerable<Guid> childrenKeys);
        List<Guid> childrenList = childrenKeys.ToList();

        Assert.Multiple(() =>
        {
            CollectionAssert.Contains(childrenList, nodeToRestore);
            Assert.AreEqual(targetParentChildrenCount + 1, childrenList.Count);
        });
    }

    /// <summary>
    /// Tests that when a node is restored from the bin, the node and all its descendants are removed from the bin.
    /// </summary>
    /// <param name="nodeToRestore">The unique identifier of the node to restore from the bin.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Restored_Node_And_Its_Descendants_Are_Removed_From_Bin(Guid nodeToRestore)
    {
        // Arrange
        _navigationService.MoveToBin(nodeToRestore);
        _navigationService.TryGetDescendantsKeysInBin(nodeToRestore, out IEnumerable<Guid> descendantsKeysInBin);

        // Act
        _navigationService.RestoreFromBin(nodeToRestore);

        // Assert
        var nodeExistsInBin = _navigationService.TryGetParentKeyInBin(nodeToRestore, out Guid? parentKeyInBinAfterRestore);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExistsInBin);
            Assert.IsNull(parentKeyInBinAfterRestore);

            foreach (Guid descendant in descendantsKeysInBin)
            {
                var descendantExistsInBin = _navigationService.TryGetParentKeyInBin(descendant, out _);
                Assert.IsFalse(descendantExistsInBin);
            }
        });
    }

    /// <summary>
    /// Tests that a restored node has the same amount of descendants as before it was moved to the bin.
    /// </summary>
    /// <param name="nodeToRestore">The key of the node to restore.</param>
    /// <param name="targetParentKey">The key of the target parent node where the node will be restored. Can be null.</param>
    /// <param name="initialDescendantsCount">The initial count of descendants before the node was moved to the bin.</param>
    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", null, 8)] // Root to content root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 2)] // Child 1 to Great-grandchild 1
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", "60E0E5C4-084E-4144-A560-7393BEAD2E96", 0)] // Grandchild 4 to Child 2
    public void Restored_Node_Has_The_Same_Amount_Of_Descendants(Guid nodeToRestore, Guid? targetParentKey, int initialDescendantsCount)
    {
        // Arrange
        _navigationService.MoveToBin(nodeToRestore);

        // Act
        _navigationService.RestoreFromBin(nodeToRestore, targetParentKey);

        // Assert
        // Verify that the number of descendants remain the same after restoring the node
        _navigationService.TryGetDescendantsKeys(nodeToRestore, out IEnumerable<Guid> restoredDescendantsKeys);
        var descendantsCountAfterRestore = restoredDescendantsKeys.Count();

        Assert.AreEqual(initialDescendantsCount, descendantsCountAfterRestore);
    }

    private void CreateTestData()
    {
        ContentType = new Guid("217C492D-0067-478C-BEA8-D0CE2DECBEB9");

        Root = new Guid("E48DD82A-7059-418E-9B82-CDD5205796CF");
        _navigationService.Add(Root, ContentType);

        Child1 = new Guid("C6173927-0C59-4778-825D-D7B9F45D8DDE");
        _navigationService.Add(Child1, ContentType, Root);

        Grandchild1 = new Guid("E856AC03-C23E-4F63-9AA9-681B42A58573");
        _navigationService.Add(Grandchild1, ContentType, Child1);

        Grandchild2 = new Guid("A1B1B217-B02F-4307-862C-A5E22DB729EB");
        _navigationService.Add(Grandchild2, ContentType, Child1);

        Child2 = new Guid("60E0E5C4-084E-4144-A560-7393BEAD2E96");
        _navigationService.Add(Child2, ContentType, Root);

        Grandchild3 = new Guid("D63C1621-C74A-4106-8587-817DEE5FB732");
        _navigationService.Add(Grandchild3, ContentType, Child2);

        GreatGrandchild1 = new Guid("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7");
        _navigationService.Add(GreatGrandchild1, ContentType, Grandchild3);

        Child3 = new Guid("B606E3FF-E070-4D46-8CB9-D31352029FDF");
        _navigationService.Add(Child3, ContentType, Root);

        Grandchild4 = new Guid("F381906C-223C-4466-80F7-B63B4EE073F8");
        _navigationService.Add(Grandchild4, ContentType, Child3);
    }
}

internal class TestContentNavigationService : ContentNavigationServiceBase<IContentType, IContentTypeService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestContentNavigationService"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    /// <param name="navigationRepository">The navigation repository.</param>
    /// <param name="contentTypeService">The content type service.</param>
    public TestContentNavigationService(
        ICoreScopeProvider coreScopeProvider,
        INavigationRepository navigationRepository,
        IContentTypeService contentTypeService)
        : base(coreScopeProvider, navigationRepository, contentTypeService)
    {
    }

    // Not needed for testing here
    /// <summary>
    /// Asynchronously rebuilds the content navigation service.
    /// In this test implementation, the method completes immediately and performs no operation.
    /// </summary>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    public override Task RebuildAsync() => Task.CompletedTask;

    // Not needed for testing here
    /// <summary>
    /// Rebuilds the binary data asynchronously. Not needed for testing here.
    /// </summary>
    /// <returns>A completed task.</returns>
    public override Task RebuildBinAsync() => Task.CompletedTask;
}
