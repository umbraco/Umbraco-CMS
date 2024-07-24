using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class NavigationServiceTests
{
    private INavigationService _navigationService;

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

        _navigationService = new ContentNavigationService(
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
    public async Task Cannot_Get_Parent_From_Non_Existing_Content_Key()
    {
        // Act
        Guid? result = await _navigationService.GetParentKeyAsync(Guid.NewGuid());

        // Assert
        Assert.IsNull(result);
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
    public async Task Can_Get_Parent_From_Existing_Content_Key(Guid childKey, Guid? parentKey)
    {
        // Act
        Guid? result = await _navigationService.GetParentKeyAsync(childKey);

        // Assert
        Assert.Multiple(() =>
        {
            if (parentKey is null)
            {
                Assert.IsNull(result);
            }
            else
            {
                Assert.IsNotNull(result);
                Assert.AreEqual(parentKey, result);
            }
        });
    }

    [Test]
    public async Task Cannot_Get_Children_From_Non_Existing_Content_Key()
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetChildrenKeysAsync(Guid.NewGuid());

        // Assert
        Assert.IsEmpty(result);
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
    public async Task Can_Get_Children_From_Existing_Content_Key(Guid parentKey, int childrenCount)
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetChildrenKeysAsync(parentKey);

        // Assert
        Assert.AreEqual(childrenCount, result.Count());
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
    public async Task Can_Get_Children_From_Existing_Content_Key_In_Correct_Order(Guid parentKey, string[] children)
    {
        // Arrange
        Guid[] expectedChildren = Array.ConvertAll(children, Guid.Parse);

        // Act
        IEnumerable<Guid> result = await _navigationService.GetChildrenKeysAsync(parentKey);

        // Assert
        for (var i = 0; i < expectedChildren.Length; i++)
        {
            Assert.AreEqual(expectedChildren[i], result.ElementAt(i));
        }
    }

    [Test]
    public async Task Cannot_Get_Descendants_From_Non_Existing_Content_Key()
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetDescendantsKeysAsync(Guid.NewGuid());

        // Assert
        Assert.IsEmpty(result);
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
    public async Task Can_Get_Descendants_From_Existing_Content_Key(Guid parentKey, int descendantsCount)
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetDescendantsKeysAsync(parentKey);

        // Assert
        Assert.AreEqual(descendantsCount, result.Count());
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
    public async Task Can_Get_Descendants_From_Existing_Content_Key_In_Correct_Order(Guid parentKey, string[] descendants)
    {
        // Arrange
        Guid[] expectedDescendants = Array.ConvertAll(descendants, Guid.Parse);

        // Act
        IEnumerable<Guid> result = await _navigationService.GetDescendantsKeysAsync(parentKey);

        // Assert
        for (var i = 0; i < expectedDescendants.Length; i++)
        {
            Assert.AreEqual(expectedDescendants[i], result.ElementAt(i));
        }
    }

    [Test]
    public async Task Cannot_Get_Ancestors_From_Non_Existing_Content_Key()
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetAncestorsKeysAsync(Guid.NewGuid());

        // Assert
        Assert.IsEmpty(result);
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
    public async Task Can_Get_Ancestors_From_Existing_Content_Key(Guid childKey, int ancestorsCount)
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetAncestorsKeysAsync(childKey);

        // Assert
        Assert.AreEqual(ancestorsCount, result.Count());
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
    public async Task Can_Get_Ancestors_From_Existing_Content_Key_In_Correct_Order(Guid childKey, string[] ancestors)
    {
        // Arrange
        Guid[] expectedAncestors = Array.ConvertAll(ancestors, Guid.Parse);

        // Act
        IEnumerable<Guid> result = await _navigationService.GetAncestorsKeysAsync(childKey);

        // Assert
        for (var i = 0; i < expectedAncestors.Length; i++)
        {
            Assert.AreEqual(expectedAncestors[i], result.ElementAt(i));
        }
    }

    [Test]
    public async Task Cannot_Get_Siblings_Of_Non_Existing_Content_Key()
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetSiblingsKeysAsync(Guid.NewGuid());

        // Assert
        Assert.IsEmpty(result);
    }

    [Test]
    public async Task Can_Get_Siblings_Of_Existing_Content_Key_Without_Self()
    {
        // Arrange
        Guid nodeKey = Child1;

        // Act
        IEnumerable<Guid> result = await _navigationService.GetSiblingsKeysAsync(nodeKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsNotEmpty(result);
            Assert.IsFalse(result.Contains(nodeKey));
        });
    }

    [Test]
    public async Task Can_Get_Siblings_Of_Existing_Content_Key_At_Content_Root()
    {
        // Arrange
        Guid anotherRoot = new Guid("716380B9-DAA9-4930-A461-95EF39EBAB41");
        _navigationService.Add(anotherRoot);

        // Act
        IEnumerable<Guid> result = await _navigationService.GetSiblingsKeysAsync(anotherRoot);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(Root, result.First());
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
    public async Task Can_Get_Siblings_Of_Existing_Content_Key(Guid key, int siblingsCount)
    {
        // Act
        IEnumerable<Guid> result = await _navigationService.GetSiblingsKeysAsync(key);

        // Assert
        Assert.AreEqual(siblingsCount, result.Count());
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new string[0])] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF" })] // Child 1 - Child 2, Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new[] { "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Grandchild 1 - Grandchild 2
    public async Task Can_Get_Siblings_Of_Existing_Content_Key_In_Correct_Order(Guid childKey, string[] siblings)
    {
        // Arrange
        Guid[] expectedSiblings = Array.ConvertAll(siblings, Guid.Parse);

        // Act
        IEnumerable<Guid> result = await _navigationService.GetSiblingsKeysAsync(childKey);

        // Assert
        for (var i = 0; i < expectedSiblings.Length; i++)
        {
            Assert.AreEqual(expectedSiblings[i], result.ElementAt(i));
        }
    }

    [Test]
    public void Cannot_Remove_Node_With_Non_Existing_Content_Key()
    {
        // Act
        var result = _navigationService.Remove(Guid.NewGuid());

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF")] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    public void Removing_Node_Removes_Its_Descendants_As_Well(Guid keyOfNodeToRemove)
    {
        // Act
        var result = _navigationService.Remove(keyOfNodeToRemove);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(0, (await _navigationService.GetDescendantsKeysAsync(keyOfNodeToRemove)).Count());
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
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual(null, await _navigationService.GetParentKeyAsync(newNodeKey));
        });
    }

    [Test]
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE")] // Child 1
    [TestCase("F381906C-223C-4466-80F7-B63B4EE073F8")] // Grandchild 4
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7")] // Great-grandchild 1
    public async Task Can_Add_Node_To_Parent(Guid parentKey)
    {
        // Arrange
        var newNodeKey = Guid.NewGuid();
        var currentChildrenCount = (await _navigationService.GetChildrenKeysAsync(parentKey)).Count();

        // Act
        var result = _navigationService.Add(newNodeKey, parentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);
            var newChildren = await _navigationService.GetChildrenKeysAsync(parentKey);
            Assert.AreEqual(currentChildrenCount + 1, newChildren.Count());
            Assert.IsTrue(newChildren.Any(childKey => childKey == newNodeKey));
        });
    }

    [Test]
    public void Cannot_Copy_When_Target_Parent_Does_Not_Exist()
    {
        // Arrange
        Guid nodeToCopy = Child1;
        var nonExistentTargetParentKey = Guid.NewGuid();

        // Act
        var result = _navigationService.Copy(nodeToCopy, out Guid copiedNodeKey, nonExistentTargetParentKey);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.AreEqual(Guid.Empty, copiedNodeKey);
        });
    }

    [Test]
    public void Cannot_Copy_When_Source_Node_Does_Not_Exist()
    {
        // Arrange
        var nonExistentSourceNodeKey = Guid.NewGuid();
        Guid targetParentKey = Grandchild1;

        // Act
        var result = _navigationService.Copy(nonExistentSourceNodeKey, out Guid copiedNodeKey, targetParentKey);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.AreEqual(Guid.Empty, copiedNodeKey);
        });
    }

    [Test]
    public void Can_Copy_Node_To_Itself()
    {
        // Arrange
        Guid nodeToCopy = GreatGrandchild1;

        // Act
        var result = _navigationService.Copy(nodeToCopy, out Guid copiedNodeKey);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreNotEqual(Guid.Empty, copiedNodeKey);
            Assert.AreNotEqual(nodeToCopy, copiedNodeKey);
        });
    }

    [Test]
    public void Can_Copy_Node_To_Content_Root()
    {
        // Arrange
        Guid nodeToCopy = Child1;

        // Act
        var result = _navigationService.Copy(nodeToCopy, out Guid copiedNodeKey); // parentKey is null

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);
            Assert.AreNotEqual(Guid.Empty, copiedNodeKey);
            Assert.AreNotEqual(nodeToCopy, copiedNodeKey);

            // Verify the copied node's parent is null (it's been copied to content root)
            Guid? copiedNodeParentKey = await _navigationService.GetParentKeyAsync(copiedNodeKey);
            Assert.IsNull(copiedNodeParentKey);
        });
    }

    [Test]
    public void Can_Copy_Node_To_Existing_Target_Parent()
    {
        // Arrange
        Guid nodeToCopy = Grandchild4;
        Guid targetParentKey = Child1;

        // Act
        var result = _navigationService.Copy(nodeToCopy, out Guid copiedNodeKey, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);
            Assert.AreNotEqual(Guid.Empty, copiedNodeKey);
            Assert.AreNotEqual(nodeToCopy, copiedNodeKey);

            // Verify the node is copied to the correct parent
            Guid? copiedNodeParentKey = await _navigationService.GetParentKeyAsync(copiedNodeKey);
            Assert.IsNotNull(copiedNodeParentKey);
            Assert.AreEqual(targetParentKey, copiedNodeParentKey);
        });
    }

    [Test]
    public async Task Copying_Node_Does_Not_Update_Source_Node_Parent()
    {
        // Arrange
        Guid nodeToCopy = Grandchild1;
        Guid targetParentKey = Child3;
        Guid? originalParentKey = await _navigationService.GetParentKeyAsync(nodeToCopy);

        // Act
        var result = _navigationService.Copy(nodeToCopy, out _, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify that the original parent is still the same
            Guid? currentParentKey = await _navigationService.GetParentKeyAsync(nodeToCopy);
            Assert.AreEqual(originalParentKey, currentParentKey);
        });
    }

    [Test]
    public async Task Copied_Node_Is_Added_To_Its_New_Parent()
    {
        // Arrange
        Guid nodeToCopy = Grandchild2;
        Guid targetParentKey = Child2;
        var targetParentChildrenCount = (await _navigationService.GetChildrenKeysAsync(targetParentKey)).Count();

        // Act
        var result = _navigationService.Copy(nodeToCopy, out Guid copiedNodeKey, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify the node is added to its new parent's children list
            IEnumerable<Guid> children = await _navigationService.GetChildrenKeysAsync(targetParentKey);
            CollectionAssert.Contains(children, copiedNodeKey);
            Assert.AreEqual(targetParentChildrenCount + 1, children.Count());
        });
    }

    [Test]
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "60E0E5C4-084E-4144-A560-7393BEAD2E96", 0)] // Grandchild 2 to Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", null, 1)] // Grandchild 3 to content root
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF", 2)] // Child 2 to Child 3
    public void Copied_Node_Has_The_Same_Amount_Of_Descendants(Guid nodeToCopy, Guid? targetParentKey, int initialDescendantsCount)
    {
        // Act
        var result = _navigationService.Copy(nodeToCopy, out Guid copiedNodeKey, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Get the number of descendants of the copied node
            var descendantsCountAfterCopy = (await _navigationService.GetDescendantsKeysAsync(copiedNodeKey)).Count();
            Assert.AreEqual(initialDescendantsCount, descendantsCountAfterCopy);
        });
    }

    [Test]
    [TestCase("A1B1B217-B02F-4307-862C-A5E22DB729EB", "E48DD82A-7059-418E-9B82-CDD5205796CF", 8)] // Grandchild 2 to Root
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", "B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Grandchild 3 to Child 3
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "F381906C-223C-4466-80F7-B63B4EE073F8", 0)] // Child 2 to Grandchild 4
    public async Task Number_Of_Target_Parent_Descendants_Updates_When_Copying_Node_With_Descendants(Guid nodeToCopy, Guid targetParentKey, int initialDescendantsCountOfTargetParent)
    {
        // Arrange
        // Get the number of descendants of the node to copy
        var descendantsCountOfNodeToCopy = (await _navigationService.GetDescendantsKeysAsync(nodeToCopy)).Count();

        // Act
        var result = _navigationService.Copy(nodeToCopy, out _, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            var updatedDescendantsCountOfTargetParent = (await _navigationService.GetDescendantsKeysAsync(targetParentKey)).Count();

            // Verify the number of descendants of the target parent has increased by the number of descendants of the copied node plus the node itself
            Assert.AreEqual(initialDescendantsCountOfTargetParent + descendantsCountOfNodeToCopy + 1, updatedDescendantsCountOfTargetParent);
        });
    }

    [Test]
    public async Task Copied_Node_Descendants_Have_Different_Keys_Than_Source_Node_Descendants()
    {
        // Arrange
        Guid nodeToCopy = Child2;
        Guid targetParentKey = Grandchild4;
        IEnumerable<Guid> sourceDescendants = await _navigationService.GetDescendantsKeysAsync(nodeToCopy);

        // Act
        var result = _navigationService.Copy(nodeToCopy, out Guid copiedNodeKey, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Get the descendants of the copied node
            IEnumerable<Guid> copiedDescendants = await _navigationService.GetDescendantsKeysAsync(copiedNodeKey);

            // Ensure all keys of the copied descendants are different from the source descendants
            Assert.IsTrue(copiedDescendants.All(copiedDescendantKey => sourceDescendants.Contains(copiedDescendantKey) is false));
        });
    }

    [Test]
    public void Cannot_Move_When_Target_Parent_Does_Not_Exist()
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
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify the node's new parent is null (moved to content root)
            Guid? newParentKey = await _navigationService.GetParentKeyAsync(nodeToMove);
            Assert.IsNull(newParentKey);
        });
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
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify the node's new parent is updated
            Guid? parentKey = await _navigationService.GetParentKeyAsync(nodeToMove);
            Assert.IsNotNull(parentKey);
            Assert.AreEqual(targetParentKey, parentKey);
        });
    }

    [Test]
    public async Task Moved_Node_Has_Updated_Parent()
    {
        // Arrange
        Guid nodeToMove = Grandchild1;
        Guid targetParentKey = Child2;
        Guid? oldParentKey = await _navigationService.GetParentKeyAsync(nodeToMove);

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify the node's new parent is updated
            Guid? parentKey = await _navigationService.GetParentKeyAsync(nodeToMove);
            Assert.IsNotNull(parentKey);
            Assert.AreEqual(targetParentKey, parentKey);

            // Verify that the new parent is different from the old one
            Assert.AreNotEqual(oldParentKey, targetParentKey);
        });
    }

    [Test]
    public async Task Moved_Node_Is_Removed_From_Its_Current_Parent()
    {
        // Arrange
        Guid nodeToMove = Grandchild3;
        Guid targetParentKey = Child3;
        Guid? oldParentKey = await _navigationService.GetParentKeyAsync(nodeToMove);
        var oldParentChildrenCount = (await _navigationService.GetChildrenKeysAsync(oldParentKey.Value)).Count();

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify the node is removed from its old parent's children list
            IEnumerable<Guid> children = await _navigationService.GetChildrenKeysAsync(oldParentKey.Value);
            CollectionAssert.DoesNotContain(children, nodeToMove);
            Assert.AreEqual(oldParentChildrenCount - 1, children.Count());
        });
    }

    [Test]
    public async Task Moved_Node_Is_Added_To_Its_New_Parent()
    {
        // Arrange
        Guid nodeToMove = Grandchild2;
        Guid targetParentKey = Child2;
        var targetParentChildrenCount = (await _navigationService.GetChildrenKeysAsync(targetParentKey)).Count();

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify the node is added to its new parent's children list
            IEnumerable<Guid> children = await _navigationService.GetChildrenKeysAsync(targetParentKey);
            CollectionAssert.Contains(children, nodeToMove);
            Assert.AreEqual(targetParentChildrenCount + 1, children.Count());
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
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            // Verify that the number of descendants remain the same after moving the node
            var descendantsCountAfterMove = (await _navigationService.GetDescendantsKeysAsync(nodeToMove)).Count();
            Assert.AreEqual(initialDescendantsCount, descendantsCountAfterMove);
        });
    }

    [Test]
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", "A1B1B217-B02F-4307-862C-A5E22DB729EB", 0)] // Child 3 to Grandchild 2
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF", 1)] // Child 2 to Child 3
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", "60E0E5C4-084E-4144-A560-7393BEAD2E96", 2)] // Grandchild 1 to Child 2
    public async Task Number_Of_Target_Parent_Descendants_Updates_When_Moving_Node_With_Descendants(Guid nodeToMove, Guid targetParentKey, int initialDescendantsCountOfTargetParent)
    {
        // Arrange
        // Get the number of descendants of the node to move
        var descendantsCountOfNodeToMove = (await _navigationService.GetDescendantsKeysAsync(nodeToMove)).Count();

        // Act
        var result = _navigationService.Move(nodeToMove, targetParentKey);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.IsTrue(result);

            var updatedDescendantsCountOfTargetParent = (await _navigationService.GetDescendantsKeysAsync(targetParentKey)).Count();

            // Verify the number of descendants of the target parent has increased by the number of descendants of the moved node plus the node itself
            Assert.AreEqual(initialDescendantsCountOfTargetParent + descendantsCountOfNodeToMove + 1, updatedDescendantsCountOfTargetParent);
        });
    }
}
