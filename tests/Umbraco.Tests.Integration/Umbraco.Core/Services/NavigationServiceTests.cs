using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class NavigationServiceTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private INavigationService NavigationService => GetRequiredService<INavigationService>();

    private Content Root { get; set; }

    private Content Child1 { get; set; }

    private Content Grandchild1 { get; set; }

    private Content Grandchild2 { get; set; }

    private Content Child2 { get; set; }

    private Content Grandchild3 { get; set; }

    private Content GreatGrandchild1 { get; set; }

    private Content Child3 { get; set; }

    private Content Grandchild4 { get; set; }

    [SetUp]
    public async Task Setup()
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

        // Doc Type
        var contentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page");
        contentType.Key = new Guid("DD72B8A6-2CE3-47F0-887E-B695A1A5D086");
        contentType.AllowedAsRoot = true;
        contentType.AllowedTemplates = null;
        contentType.AllowedContentTypes = new[] { new ContentTypeSort(contentType.Key, 0, contentType.Alias) };
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Content
        Root = ContentBuilder.CreateSimpleContent(contentType, "Root");
        Root.Key = new Guid("E48DD82A-7059-418E-9B82-CDD5205796CF");
        ContentService.Save(Root, Constants.Security.SuperUserId);

        Child1 = ContentBuilder.CreateSimpleContent(contentType, "Child 1", Root.Id);
        Child1.Key = new Guid("C6173927-0C59-4778-825D-D7B9F45D8DDE");
        ContentService.Save(Child1, Constants.Security.SuperUserId);

        Grandchild1 = ContentBuilder.CreateSimpleContent(contentType, "Grandchild 1", Child1.Id);
        Grandchild1.Key = new Guid("E856AC03-C23E-4F63-9AA9-681B42A58573");
        ContentService.Save(Grandchild1, Constants.Security.SuperUserId);

        Grandchild2 = ContentBuilder.CreateSimpleContent(contentType, "Grandchild 2", Child1.Id);
        Grandchild2.Key = new Guid("A1B1B217-B02F-4307-862C-A5E22DB729EB");
        ContentService.Save(Grandchild2, Constants.Security.SuperUserId);

        Child2 = ContentBuilder.CreateSimpleContent(contentType, "Child 2", Root.Id);
        Child2.Key = new Guid("60E0E5C4-084E-4144-A560-7393BEAD2E96");
        ContentService.Save(Child2, Constants.Security.SuperUserId);

        Grandchild3 = ContentBuilder.CreateSimpleContent(contentType, "Grandchild 3", Child2.Id);
        Grandchild3.Key = new Guid("D63C1621-C74A-4106-8587-817DEE5FB732");
        ContentService.Save(Grandchild3, Constants.Security.SuperUserId);

        GreatGrandchild1 = ContentBuilder.CreateSimpleContent(contentType, "Great-grandchild 1", Grandchild3.Id);
        GreatGrandchild1.Key = new Guid("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7");
        ContentService.Save(GreatGrandchild1, Constants.Security.SuperUserId);

        Child3 = ContentBuilder.CreateSimpleContent(contentType, "Child 3", Root.Id);
        Child3.Key = new Guid("B606E3FF-E070-4D46-8CB9-D31352029FDF");
        ContentService.Save(Child3, Constants.Security.SuperUserId);

        Grandchild4 = ContentBuilder.CreateSimpleContent(contentType, "Grandchild 3", Child3.Id);
        Grandchild4.Key = new Guid("F381906C-223C-4466-80F7-B63B4EE073F8");
        ContentService.Save(Grandchild4, Constants.Security.SuperUserId);
    }

    [Test]
    public async Task Cannot_Get_Parent_From_Non_Existing_Content_Key()
    {
        // Act
        var result = await NavigationService.GetParentKeyAsync(Guid.NewGuid());

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
        Guid? result = await NavigationService.GetParentKeyAsync(childKey);

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
        IEnumerable<Guid> result = await NavigationService.GetChildrenKeysAsync(Guid.NewGuid());

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
        IEnumerable<Guid> result = await NavigationService.GetChildrenKeysAsync(parentKey);

        // Assert
        Assert.AreEqual(childrenCount, result.Count());
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "60E0E5C4-084E-4144-A560-7393BEAD2E96", "B606E3FF-E070-4D46-8CB9-D31352029FDF" })] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "E856AC03-C23E-4F63-9AA9-681B42A58573", "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Child 1
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
        IEnumerable<Guid> result = await NavigationService.GetChildrenKeysAsync(parentKey);

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
        IEnumerable<Guid> result = await NavigationService.GetDescendantsKeysAsync(Guid.NewGuid());

        // Assert
        Assert.IsEmpty(result);
    }

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
    public async Task Can_Get_Descendants_From_Existing_Content_Key(Guid parentKey, int descendantsCount)
    {
        // Act
        IEnumerable<Guid> result = await NavigationService.GetDescendantsKeysAsync(parentKey);

        // Assert
        Assert.AreEqual(descendantsCount, result.Count());
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "E856AC03-C23E-4F63-9AA9-681B42A58573",
        "A1B1B217-B02F-4307-862C-A5E22DB729EB", "60E0E5C4-084E-4144-A560-7393BEAD2E96", "D63C1621-C74A-4106-8587-817DEE5FB732", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7",
        "B606E3FF-E070-4D46-8CB9-D31352029FDF", "F381906C-223C-4466-80F7-B63B4EE073F8" })] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "E856AC03-C23E-4F63-9AA9-681B42A58573", "A1B1B217-B02F-4307-862C-A5E22DB729EB" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new string[0])] // Grandchild 1
    [TestCase("60E0E5C4-084E-4144-A560-7393BEAD2E96", new[] { "D63C1621-C74A-4106-8587-817DEE5FB732", "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Child 2
    [TestCase("D63C1621-C74A-4106-8587-817DEE5FB732", new[] { "56E29EA9-E224-4210-A59F-7C2C5C0C5CC7" })] // Grandchild 3
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", new string[0])] // Great-grandchild 1
    [TestCase("B606E3FF-E070-4D46-8CB9-D31352029FDF", new[] { "F381906C-223C-4466-80F7-B63B4EE073F8" })] // Child 3
    public async Task Can_Get_Descendants_From_Existing_Content_Key_In_Correct_Order(Guid parentKey, string[] descendants)
    {
        // Arrange
        Guid[] expectedDescendants = Array.ConvertAll(descendants, Guid.Parse);

        // Act
        IEnumerable<Guid> result = await NavigationService.GetDescendantsKeysAsync(parentKey);

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
        IEnumerable<Guid> result = await NavigationService.GetAncestorsKeysAsync(Guid.NewGuid());

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
        IEnumerable<Guid> result = await NavigationService.GetAncestorsKeysAsync(childKey);

        // Assert
        Assert.AreEqual(ancestorsCount, result.Count());
    }

    [Test]
    [TestCase("E48DD82A-7059-418E-9B82-CDD5205796CF", new string[0])] // Root
    [TestCase("C6173927-0C59-4778-825D-D7B9F45D8DDE", new[] { "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Child 1
    [TestCase("E856AC03-C23E-4F63-9AA9-681B42A58573", new[] { "C6173927-0C59-4778-825D-D7B9F45D8DDE", "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Grandchild 1
    [TestCase("56E29EA9-E224-4210-A59F-7C2C5C0C5CC7", new[] { "D63C1621-C74A-4106-8587-817DEE5FB732", "60E0E5C4-084E-4144-A560-7393BEAD2E96", "E48DD82A-7059-418E-9B82-CDD5205796CF" })] // Great-grandchild 1
    public async Task Can_Get_Ancestors_From_Existing_Content_Key_In_Correct_Order(Guid childKey, string[] ancestors)
    {
        // Arrange
        Guid[] expectedAncestors = Array.ConvertAll(ancestors, Guid.Parse);

        // Act
        IEnumerable<Guid> result = await NavigationService.GetAncestorsKeysAsync(childKey);

        // Assert
        for (var i = 0; i < expectedAncestors.Length; i++)
        {
            Assert.AreEqual(expectedAncestors[i], result.ElementAt(i));
        }
    }
}
