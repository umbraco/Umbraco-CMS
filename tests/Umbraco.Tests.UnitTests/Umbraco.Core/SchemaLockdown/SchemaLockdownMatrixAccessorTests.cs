using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.SchemaLockdown;

[TestFixture]
public class SchemaLockdownMatrixAccessorTests
{
    private sealed class AllowDeletes : ISchemaLockdownConfigurator
    {
        public void Configure(SchemaLockdownMatrix matrix)
            => matrix.Allow(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete);
    }

    private sealed class BlockDeletes : ISchemaLockdownConfigurator
    {
        public void Configure(SchemaLockdownMatrix matrix)
            => matrix.Block(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete);
    }

    private sealed class LockDocumentTypes : ISchemaLockdownConfigurator
    {
        public void Configure(SchemaLockdownMatrix matrix)
            => matrix.BlockMutations(Constants.UdiEntityType.DocumentType);
    }

    private static SchemaLockdownMatrixAccessor CreateAccessor(params ISchemaLockdownConfigurator[] configurators)
        => new(new SchemaLockdownConfiguratorCollection(() => configurators));

    [Test]
    public void Nothing_Is_Locked_When_No_Configurator_Is_Registered()
    {
        SchemaLockdownMatrix matrix = CreateAccessor().Matrix;

        Assert.Multiple(() =>
        {
            foreach (var entityType in SchemaEntityTypes.All)
            {
                Assert.That(matrix.IsAllowed(entityType, SchemaOperation.Create), Is.True);
                Assert.That(matrix.IsAllowed(entityType, SchemaOperation.Update), Is.True);
                Assert.That(matrix.IsAllowed(entityType, SchemaOperation.Delete), Is.True);
            }
        });
    }

    [Test]
    public void Only_The_Entity_Types_A_Configurator_Names_Are_Locked()
    {
        SchemaLockdownMatrix matrix = CreateAccessor(new LockDocumentTypes()).Matrix;

        Assert.Multiple(() =>
        {
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Read), Is.True);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DataType, SchemaOperation.Create), Is.True);
        });
    }

    [Test]
    public void Configurators_Run_In_Order_And_Later_Writes_Win()
    {
        SchemaLockdownMatrix matrix = CreateAccessor(new AllowDeletes(), new BlockDeletes()).Matrix;

        Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
    }

    [Test]
    public void Matrix_Is_Frozen_And_Cached()
    {
        SchemaLockdownMatrixAccessor accessor = CreateAccessor(new LockDocumentTypes());

        Assert.Multiple(() =>
        {
            Assert.That(accessor.Matrix, Is.SameAs(accessor.Matrix));
            Assert.Throws<InvalidOperationException>(() => accessor.Matrix.Allow(Constants.UdiEntityType.DataType, SchemaOperation.Create));
        });
    }
}
