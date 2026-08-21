using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.SchemaLockdown;

[TestFixture]
public class SchemaLockdownMatrixTests
{
    [Test]
    public void Everything_Is_Allowed_By_Default()
    {
        var matrix = new SchemaLockdownMatrix();

        Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.True);
    }

    [Test]
    public void Block_Then_Allow_Leaves_Cell_Allowed()
    {
        var matrix = new SchemaLockdownMatrix();

        matrix.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete);
        matrix.Allow(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete);

        Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.True);
    }

    [Test]
    public void Cells_Are_Independent()
    {
        var matrix = new SchemaLockdownMatrix();

        matrix.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Create);

        Assert.Multiple(() =>
        {
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.True);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Create), Is.True);
        });
    }

    [Test]
    public void BlockMutations_Blocks_Every_Operation_Except_Read()
    {
        var matrix = new SchemaLockdownMatrix();

        matrix.BlockMutations(Constants.UdiEntityType.DocumentType);

        Assert.Multiple(() =>
        {
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Update), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Unknown), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Read), Is.True);
        });
    }

    [Test]
    public void Cell_Lookup_Is_Case_Insensitive_On_Entity_Type()
    {
        var matrix = new SchemaLockdownMatrix();

        matrix.Block("Dictionary-Item", SchemaOperation.Update);

        Assert.Multiple(() =>
        {
            Assert.That(matrix.IsAllowed("dictionary-item", SchemaOperation.Update), Is.False);
            Assert.That(matrix.IsAllowed("Dictionary-Item", SchemaOperation.Update), Is.False);
        });

        matrix.Allow("dictionary-item", SchemaOperation.Update);

        Assert.That(matrix.IsAllowed("DICTIONARY-ITEM", SchemaOperation.Update), Is.True);
    }

    [Test]
    public void Mutating_After_Freeze_Throws()
    {
        var matrix = new SchemaLockdownMatrix();
        matrix.Freeze();

        Assert.Throws<InvalidOperationException>(() => matrix.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Create));
    }

    [Test]
    public void Snapshot_Reports_Every_Entity_Type_And_Operation()
    {
        var matrix = new SchemaLockdownMatrix();
        matrix.Block(Constants.UdiEntityType.Script, SchemaOperation.Update);
        matrix.Freeze();

        IReadOnlyDictionary<string, IReadOnlyDictionary<SchemaOperation, bool>> snapshot = matrix.Snapshot();

        Assert.That(snapshot[Constants.UdiEntityType.Script][SchemaOperation.Update], Is.False);
    }
}
