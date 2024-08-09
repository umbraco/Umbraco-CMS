using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ContentNavigationServiceBaseTests
{
    private TestContentNavigationService _navigationService;

    private Guid Root { get; set; }

    private Guid Child1 { get; set; }

    private Guid Grandchild1 { get; set; }

    private Guid Grandchild2 { get; set; }

    private Guid Child2 { get; set; }

    private Guid Grandchild3 { get; set; }

    private Guid GreatGrandchild1 { get; set; }

    private Guid Child3 { get; set; }

    private Guid Grandchild4 { get; set; }

    [SetUp]
    public void Setup()
    {
        // Root
        //    - Child 1
        //      - Grandchild 1
        //      - Grandchild 2
        //    - Child 2
        //      - Grandchild 3
        //        - Great-grandchild 1
        //    - Child 3
        //      - Grandchild 4

        _navigationService = new TestContentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>());

        Root = new Guid("E48DD82A-7059-418E-9B82-CDD5205796CF");
        _navigationService.Add(Root);

        Child1 = new Guid("C6173927-0C59-4778-825D-D7B9F45D8DDE");
        _navigationService.Add(Child1, Root);

        Grandchild1 = new Guid("E856AC03-C23E-4F63-9AA9-681B42A58573");
        _navigationService.Add(Grandchild1, Child1);

        Grandchild2 = new Guid("A1B1B217-B02F-4307-862C-A5E22DB729EB");
        _navigationService.Add(Grandchild2, Child1);

        Child2 = new Guid("60E0E5C4-084E-4144-A560-7393BEAD2E96");
        _navigationService.Add(Child2, Root);

        Grandchild3 = new Guid("D63C1621-C74A-4106-8587-817DEE5FB732");
        _navigationService.Add(Grandchild3, Child2);

        GreatGrandchild1 = new Guid("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7");
        _navigationService.Add(GreatGrandchild1, Grandchild3);

        Child3 = new Guid("B606E3FF-E070-4D46-8CB9-D31352029FDF");
        _navigationService.Add(Child3, Root);

        Grandchild4 = new Guid("F381906C-223C-4466-80F7-B63B4EE073F8");
        _navigationService.Add(Grandchild4, Child3);
    }

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

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF",
        new[]
        {
            "C6173927-0C59-4778-825D-D7B9F45D8DDE", "60E0E5C4-084E-4144-A560-7393BEAD2E96",
            "B606E3FF-E070-4D46-8CB9-D31352029FDF"
        })] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE",
        new[] { "E856AC03-C23E-4F63-9AA9-681B42A58573", "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new string[0])] // Grandchild 1
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", new[] { "D63C1621-C74A-4106-8587-817DEE5FB732" })] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", new[] { "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", new string[0])] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", new[] { "F381906C-223C-4466-80F7-B63B4EE073F8" })] // Child 3
    public void Can_Get_Children_From_Existing_Content_Key_In_Correct_Order(Guid parentKey, string[] children)
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

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF",
        8)] // Root - Child 1, Grandchild 1, Grandchild 2, Child 2, Grandchild 3, Great-grandchild 1, Child 3, Grandchild 4
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
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF",
        new[]
        {
            "C6173927-0C59-4778-825D-D7B9F45D8DDE", "E856AC03-C23E-4F63-9AA9-681B42A58573",
            "A1B1B217-B02F-4307-862C-A5E22DB729EB", "60E0E5C4-084E-4144-A560-7393BEAD2E96",
            "D63C1621-C74A-4106-8587-817DEE5FB732", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7",
            "B606E3FF-E070-4D46-8CB9-D31352029FDF", "F381906C-223C-4466-80F7-B63B4EE073F8"
        })] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE",
        new[] { "E856AC03-C23E-4F63-9AA9-681B42A58573", "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new string[0])] // Grandchild 1
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96",
        new[] { "D63C1621-C74A-4106-8587-817DEE5FB732", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", new[] { "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", new string[0])] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", new[] { "F381906C-223C-4466-80F7-B63B4EE073F8" })] // Child 3
    public void Can_Get_Descendants_From_Existing_Content_Key_In_Correct_Order(Guid parentKey, string[] descendants)
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

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new string[0])] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573",
        new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Grandchild 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7",
        new[]
        {
            "D63C1621-C74A-4106-8587-817DEE5FB732", "60E0E5C4-084E-4144-A560-7393BEAD2E96",
            "E48DD82A-7059-418E-9B82-CDD5205796CF"
        })] // Great-grandchild 1
    public void Can_Get_Ancestors_From_Existing_Content_Key_In_Correct_Order(Guid childKey, string[] ancestors)
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

    [Test]
    public void Can_Get_Siblings_Of_Existing_Content_Key_At_Content_Root()
    {
        // Arrange
        Guid anotherRoot = new Guid("716380B9-DAA9-4930-A461-95EF39EBAB41");
        _navigationService.Add(anotherRoot);

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

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new string[0])] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF" })] // Child 1 - Child 2, Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new[] { "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Grandchild 1 - Grandchild 2
    public void Can_Get_Siblings_Of_Existing_Content_Key_In_Correct_Order(Guid childKey, string[] siblings)
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

    [Test]
    public void Cannot_Remove_Node_With_Non_Existing_Content_Key()
    {
        // Arrange
        var nonExistingKey = Guid.NewGuid();

        // Act
        var result = _navigationService.Remove(nonExistingKey);

        // Assert
        Assert.IsFalse(result);
    }

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
    public void Can_Remove_Node(Guid keyOfNodeToRemove)
    {
        // Act
        var result = _navigationService.Remove(keyOfNodeToRemove);

        // Assert
        Assert.IsTrue(result);

        var nodeExists = _navigationService.TryGetParentKey(keyOfNodeToRemove, out Guid? parentKey);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(nodeExists);
            Assert.IsNull(parentKey);
        });
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Removing_Node_Removes_Its_Descendants_As_Well(Guid keyOfNodeToRemove)
    {
        // Arrange
        _navigationService.TryGetDescendantsKeys(keyOfNodeToRemove, out IEnumerable<Guid> initialDescendantsKeys);

        // Act
        var result = _navigationService.Remove(keyOfNodeToRemove);

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
    public void Removing_Node_Adds_It_To_Recycle_Bin_Root(Guid keyOfNodeToRemove)
    {
        // Act
        _navigationService.Remove(keyOfNodeToRemove);

        // Assert
        var nodeExistsInBin = _navigationService.TryGetParentKeyInBin(keyOfNodeToRemove, out Guid? parentKeyInBin);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(nodeExistsInBin);
            Assert.IsNull(parentKeyInBin);
        });
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF")] // Child 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public void Removing_Node_Adds_Its_Descendants_To_Recycle_Bin_As_Well(Guid keyOfNodeToRemove)
    {
        // Arrange
        _navigationService.TryGetDescendantsKeys(keyOfNodeToRemove, out IEnumerable<Guid> initialDescendantsKeys);
        List<Guid> initialDescendantsList = initialDescendantsKeys.ToList();

        // Act
        _navigationService.Remove(keyOfNodeToRemove);

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

    [Test]
    public void Cannot_Add_Node_When_Parent_Does_Not_Exist()
    {
        // Arrange
        var newNodeKey = Guid.NewGuid();
        var nonExistentParentKey = Guid.NewGuid();

        // Act
        var result = _navigationService.Add(newNodeKey, nonExistentParentKey);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void Cannot_Add_When_Node_With_The_Same_Key_Already_Exists()
    {
        // Act
        var result = _navigationService.Add(Child1);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void Can_Add_Node_To_Content_Root()
    {
        // Arrange
        var newNodeKey = Guid.NewGuid();

        // Act
        var result = _navigationService.Add(newNodeKey); // parentKey is null

        // Assert
        Assert.IsTrue(result);

        var nodeExists = _navigationService.TryGetParentKey(newNodeKey, out Guid? parentKey);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(nodeExists);
            Assert.IsNull(parentKey);
        });
    }

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
        var result = _navigationService.Add(newNodeKey, parentKey);

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

    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "60E0E5C4-084E-4144-A560-7393BEAD2E96", 0)] // Grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", null, 1)] // Child 3 to content root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "C6173927-0C59-4778-825D-D7B9F45D8DDE", 2)] // Child 2 to Child 1
    public void Moved_Node_Has_The_Same_Amount_Of_Descendants(Guid nodeToMove, Guid? targetParentKey, int initialDescendantsCount)
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

    [Test]
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "A1B1B217-B02F-4307-862C-A5E22DB729EB", 0)] // Child 3 to Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 2 to Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Grandchild 1 to Child 2
    public void Number_Of_Target_Parent_Descendants_Updates_When_Moving_Node_With_Descendants(Guid nodeToMove, Guid targetParentKey, int initialDescendantsCountOfTargetParent)
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

    [Test]
    public void Cannot_Restore_Node_When_Target_Parent_Does_Not_Exist()
    {
        // Arrange
        Guid nodeToRestore = Grandchild1;
        var nonExistentTargetParentKey = Guid.NewGuid();
        _navigationService.Remove(nodeToRestore);

        // Act
        var result = _navigationService.RestoreFromBin(nodeToRestore, nonExistentTargetParentKey);

        // Assert
        Assert.IsFalse(result);
    }

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
        _navigationService.Remove(nodeToRestore);

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

    [Test]
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Grandchild 1 to Child 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", "60E0E5C4-084E-4144-A560-7393BEAD2E96")] // Great-grandchild 1 to Child 2
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "E48DD82A-7059-418E-9B82-CDD5205796CF")] // Child 3 to Root
    public void Restored_Node_Is_Added_To_Its_Target_Parent(Guid nodeToRestore, Guid targetParentKey)
    {
        // Arrange
        _navigationService.Remove(nodeToRestore);
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

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732")] // Grandchild 3
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Restored_Node_And_Its_Descendants_Are_Removed_From_Bin(Guid nodeToRestore)
    {
        // Arrange
        _navigationService.Remove(nodeToRestore);
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

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", null, 8)] // Root to content root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", 2)] // Child 1 to Great-grandchild 1
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8", "60E0E5C4-084E-4144-A560-7393BEAD2E96", 0)] // Grandchild 4 to Child 2
    public void Restored_Node_Has_The_Same_Amount_Of_Descendants(Guid nodeToRestore, Guid? targetParentKey, int initialDescendantsCount)
    {
        // Arrange
        _navigationService.Remove(nodeToRestore);

        // Act
        _navigationService.RestoreFromBin(nodeToRestore, targetParentKey);

        // Assert
        // Verify that the number of descendants remain the same after restoring the node
        _navigationService.TryGetDescendantsKeys(nodeToRestore, out IEnumerable<Guid> restoredDescendantsKeys);
        var descendantsCountAfterRestore = restoredDescendantsKeys.Count();

        Assert.AreEqual(initialDescendantsCount, descendantsCountAfterRestore);
    }
}

internal class TestContentNavigationService : ContentNavigationServiceBase
{
    public TestContentNavigationService(ICoreScopeProvider coreScopeProvider, INavigationRepository navigationRepository)
        : base(coreScopeProvider, navigationRepository)
    {
    }

    // Not needed for testing here
    public override Task RebuildAsync() => Task.CompletedTask;
}
