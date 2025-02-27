using NUnit.Framework;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class TreeEntitySortingServiceTests
{
    private readonly List<ITreeEntity> _treeEntities = new()
    {
        new ContentEntitySlim { Id = 01, SortOrder = 00, Key = new Guid("8c927e65-f175-406a-8328-9e5d5bd77689") },
        new ContentEntitySlim { Id = 02, SortOrder = 01, Key = new Guid("92b27594-882a-4bdc-b238-087851970176") },
        new ContentEntitySlim { Id = 03, SortOrder = 02, Key = new Guid("30b763bc-2a8b-4e61-859b-d48b3134eaaf") },
        new ContentEntitySlim { Id = 04, SortOrder = 03, Key = new Guid("aac00e78-a132-4903-9f9e-c7089f17f151") },
        new ContentEntitySlim { Id = 05, SortOrder = 04, Key = new Guid("263c26b8-9dd6-4bbc-a359-461baf2df191") },
        new ContentEntitySlim { Id = 06, SortOrder = 05, Key = new Guid("84fa6fef-4b7a-40ea-ad03-c198abaaaea2") },
        new ContentEntitySlim { Id = 07, SortOrder = 06, Key = new Guid("2db04e7f-2190-4f4b-9a76-55d074a58fa1") },
        new ContentEntitySlim { Id = 08, SortOrder = 07, Key = new Guid("220a6f46-f5eb-4ab7-80c3-b29f5a927b71") },
        new ContentEntitySlim { Id = 09, SortOrder = 08, Key = new Guid("f2595836-7133-402c-9f05-3dfeabeb851c") },
        new ContentEntitySlim { Id = 10, SortOrder = 09, Key = new Guid("143a30ca-8e65-4c94-83da-96cc5088ec2c") },
        new ContentEntitySlim { Id = 11, SortOrder = 10, Key = new Guid("0c089f9f-f901-4a73-896b-6bd9e8b927a4") },
        new ContentEntitySlim { Id = 12, SortOrder = 11, Key = new Guid("3f522f7e-f5cc-4f88-90f9-ea7fd52ad868") },
        new ContentEntitySlim { Id = 13, SortOrder = 12, Key = new Guid("b1cd6692-a073-4992-9905-7e08e91234e5") },
        new ContentEntitySlim { Id = 14, SortOrder = 13, Key = new Guid("22a7eb2f-1981-4d00-8735-cd87b2a30bda") },
        new ContentEntitySlim { Id = 15, SortOrder = 14, Key = new Guid("fdb6785c-1f16-4c82-924b-cda37c42839f") },
        new ContentEntitySlim { Id = 16, SortOrder = 15, Key = new Guid("8ef27112-7224-4066-a628-2ae883b92843") },
        new ContentEntitySlim { Id = 17, SortOrder = 16, Key = new Guid("df06a384-006e-4d11-bc21-dbdbd864f66f") },
        new ContentEntitySlim { Id = 18, SortOrder = 17, Key = new Guid("4547c977-97a2-4d08-9ad5-6f65c6cb7aed") },
        new ContentEntitySlim { Id = 19, SortOrder = 18, Key = new Guid("58f1c5cd-507a-4426-97df-b9e917c10334") },
        new ContentEntitySlim { Id = 20, SortOrder = 19, Key = new Guid("c3447f0d-f5d2-4301-86fe-0dcfc5f63c6c") },
    };

    private Guid EntityKey(int id) => _treeEntities.First(i => i.Id == id).Key;

    private SortingModel SortingModel(Guid key, int sortOrder) => new() { Key = key, SortOrder = sortOrder };

    private void AssertContainsAllEntities(ITreeEntity[] result)
    {
        Assert.AreEqual(_treeEntities.Count, result.Length);
        Assert.IsTrue(_treeEntities.All(e => result.SingleOrDefault(r => r.Key == e.Key) is not null));
    }

    [Test]
    public void Can_Move_Last_Entity_First()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[] { SortingModel(EntityKey(20), 0) })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.AreEqual(EntityKey(20), result.First().Key);
        Assert.AreEqual(EntityKey(1), result[1].Key);
        Assert.AreEqual(EntityKey(19), result.Last().Key);
    }

    [Test]
    public void Can_Move_First_Entity_Last()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[] { SortingModel(EntityKey(1), 19) })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.AreEqual(EntityKey(2), result.First().Key);
        Assert.AreEqual(EntityKey(3), result[1].Key);
        Assert.AreEqual(EntityKey(1), result.Last().Key);
    }

    [Test]
    public void Can_Move_Three_Entities_Last()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[]
                {
                    SortingModel(EntityKey(7), 19),
                    SortingModel(EntityKey(2), 18),
                    SortingModel(EntityKey(12), 17)
                })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.AreEqual(EntityKey(1), result.First().Key);
        Assert.AreEqual(EntityKey(7), result[19].Key);
        Assert.AreEqual(EntityKey(2), result[18].Key);
        Assert.AreEqual(EntityKey(12), result[17].Key);
    }

    [Test]
    public void Can_Move_Three_Entities_First()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[]
                {
                    SortingModel(EntityKey(7), 0),
                    SortingModel(EntityKey(17), 1),
                    SortingModel(EntityKey(12), 2)
                })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.AreEqual(EntityKey(7), result.First().Key);
        Assert.AreEqual(EntityKey(17), result[1].Key);
        Assert.AreEqual(EntityKey(12), result[2].Key);
        Assert.AreEqual(EntityKey(20), result.Last().Key);
    }

    [Test]
    public void Can_Apply_Same_Position_To_Entities()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[]
                {
                    SortingModel(EntityKey(1), 0),
                    SortingModel(EntityKey(7), 6),
                    SortingModel(EntityKey(17), 16),
                    SortingModel(EntityKey(12), 11)
                })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.IsTrue(result.SequenceEqual(_treeEntities));
    }

    [Test]
    public void Can_Move_Multiple_Entities_Forwards()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[]
                {
                    SortingModel(EntityKey(10), 4),
                    SortingModel(EntityKey(20), 9)
                })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.AreEqual(EntityKey(10), result[4].Key);
        Assert.AreEqual(EntityKey(20), result[9].Key);
    }

    [Test]
    public void Can_Move_Multiple_Entities_Backwards()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[]
                {
                    SortingModel(EntityKey(10), 19),
                    SortingModel(EntityKey(5), 14)
                })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.AreEqual(EntityKey(5), result[14].Key);
        Assert.AreEqual(EntityKey(10), result[19].Key);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Sorting_Instruction_Order_Does_Not_Matter_When_Overlapping(bool lastFirst)
    {
        var treeEntitySortingService = new TreeEntitySortingService();

        var sortingInstructions = lastFirst
            ? new[] { SortingModel(EntityKey(10), 19), SortingModel(EntityKey(5), 14) }
            : new[] { SortingModel(EntityKey(5), 14), SortingModel(EntityKey(10), 19) };

        var result = treeEntitySortingService.SortEntities(_treeEntities, sortingInstructions).ToArray();

        AssertContainsAllEntities(result);

        // the result must not depend on the order in which the instructions are carried out
        Assert.AreEqual(EntityKey(5), result[14].Key);
        Assert.AreEqual(EntityKey(10), result[19].Key);
    }

    [TestCase(false)]
    [TestCase(false)]
    public void Sorting_Instruction_Order_Does_Not_Matter_When_Not_Overlapping(bool lastFirst)
    {
        var treeEntitySortingService = new TreeEntitySortingService();

        var sortingInstructions = lastFirst
            ? new[] { SortingModel(EntityKey(5), 8), SortingModel(EntityKey(12), 17) }
            : new[] { SortingModel(EntityKey(12), 17), SortingModel(EntityKey(5), 8) };

        var result = treeEntitySortingService.SortEntities(_treeEntities, sortingInstructions).ToArray();

        AssertContainsAllEntities(result);

        // the result must not depend on the order in which the instructions are carried out
        Assert.AreEqual(EntityKey(5), result[8].Key);
        Assert.AreEqual(EntityKey(12), result[17].Key);
    }

    [Test]
    public void Can_Reverse_All_Entities()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        var result = treeEntitySortingService.SortEntities(
                _treeEntities,
                new[]
                {
                    SortingModel(EntityKey(1), 19),
                    SortingModel(EntityKey(2), 18),
                    SortingModel(EntityKey(3), 17),
                    SortingModel(EntityKey(4), 16),
                    SortingModel(EntityKey(5), 15),
                    SortingModel(EntityKey(6), 14),
                    SortingModel(EntityKey(7), 13),
                    SortingModel(EntityKey(8), 12),
                    SortingModel(EntityKey(9), 11),
                    SortingModel(EntityKey(10), 10),
                    SortingModel(EntityKey(11), 9),
                    SortingModel(EntityKey(12), 8),
                    SortingModel(EntityKey(13), 7),
                    SortingModel(EntityKey(14), 6),
                    SortingModel(EntityKey(15), 5),
                    SortingModel(EntityKey(16), 4),
                    SortingModel(EntityKey(17), 3),
                    SortingModel(EntityKey(18), 2),
                    SortingModel(EntityKey(19), 1),
                    SortingModel(EntityKey(20), 0),
                })
            .ToArray();

        AssertContainsAllEntities(result);

        Assert.AreEqual(EntityKey(20), result[0].Key);
        Assert.AreEqual(EntityKey(19), result[1].Key);
        Assert.AreEqual(EntityKey(18), result[2].Key);
        Assert.AreEqual(EntityKey(17), result[3].Key);
        Assert.AreEqual(EntityKey(16), result[4].Key);
        Assert.AreEqual(EntityKey(15), result[5].Key);
        Assert.AreEqual(EntityKey(14), result[6].Key);
        Assert.AreEqual(EntityKey(13), result[7].Key);
        Assert.AreEqual(EntityKey(12), result[8].Key);
        Assert.AreEqual(EntityKey(11), result[9].Key);
        Assert.AreEqual(EntityKey(10), result[10].Key);
        Assert.AreEqual(EntityKey(9), result[11].Key);
        Assert.AreEqual(EntityKey(8), result[12].Key);
        Assert.AreEqual(EntityKey(7), result[13].Key);
        Assert.AreEqual(EntityKey(6), result[14].Key);
        Assert.AreEqual(EntityKey(5), result[15].Key);
        Assert.AreEqual(EntityKey(4), result[16].Key);
        Assert.AreEqual(EntityKey(3), result[17].Key);
        Assert.AreEqual(EntityKey(2), result[18].Key);
        Assert.AreEqual(EntityKey(1), result[19].Key);
    }

    [TestCase(-1)]
    [TestCase(-1000)]
    [TestCase(20)]
    [TestCase(2000)]
    public void Cannot_Sort_Outside_Entities_Bounds(int sortOrder)
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        Assert.Throws<ArgumentException>(() => treeEntitySortingService.SortEntities(
            _treeEntities,
            new[]
            {
                SortingModel(EntityKey(10), sortOrder)
            }));
    }

    [Test]
    public void Cannot_Sort_Entities_Under_Different_Parents()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        Assert.Throws<ArgumentException>(() => treeEntitySortingService.SortEntities(
            new []
            {
                new ContentEntitySlim { Id = 01, ParentId = 123, SortOrder = 0, Key = new Guid("8c927e65-f175-406a-8328-9e5d5bd77689") },
                new ContentEntitySlim { Id = 02, ParentId = 456, SortOrder = 1, Key = new Guid("92b27594-882a-4bdc-b238-087851970176") },
            },
            new[]
            {
                SortingModel(new Guid("8c927e65-f175-406a-8328-9e5d5bd77689"), 1)
            }));
    }

    [Test]
    public void Cannot_Sort_NonExisting_Entity()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        Assert.Throws<ArgumentException>(() => treeEntitySortingService.SortEntities(
            _treeEntities,
            new[]
            {
                SortingModel(Guid.NewGuid(), 1)
            }));
    }

    [Test]
    public void Cannot_Supply_Duplicate_Key_Instructions()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        Assert.Throws<ArgumentException>(() => treeEntitySortingService.SortEntities(
            _treeEntities,
            new[]
            {
                SortingModel(EntityKey(10), 1),
                SortingModel(EntityKey(10), 2),
            }));
    }

    [Test]
    public void Cannot_Supply_Duplicate_SortOrder_Instructions()
    {
        var treeEntitySortingService = new TreeEntitySortingService();
        Assert.Throws<ArgumentException>(() => treeEntitySortingService.SortEntities(
            _treeEntities,
            new[]
            {
                SortingModel(EntityKey(10), 1),
                SortingModel(EntityKey(11), 1),
            }));
    }
}
