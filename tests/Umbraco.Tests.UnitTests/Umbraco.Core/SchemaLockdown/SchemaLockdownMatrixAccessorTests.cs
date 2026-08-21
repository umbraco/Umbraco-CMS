using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
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

    private static SchemaLockdownMatrixAccessor CreateAccessor(bool enabled, params ISchemaLockdownConfigurator[] configurators)
        => CreateAccessor(new SchemaLockdownSettings { Enabled = enabled }, configurators);

    private static SchemaLockdownMatrixAccessor CreateAccessor(
        SchemaLockdownSettings settings,
        params ISchemaLockdownConfigurator[] configurators)
    {
        var collection = new SchemaLockdownConfiguratorCollection(() => configurators);
        return new SchemaLockdownMatrixAccessor(Options.Create(settings), collection);
    }

    [Test]
    public void Everything_Allowed_When_Disabled()
    {
        SchemaLockdownMatrix matrix = CreateAccessor(enabled: false).Matrix;

        Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.True);
    }

    [Test]
    public void Governed_Types_Block_Everything_But_Read_When_Enabled()
    {
        SchemaLockdownMatrix matrix = CreateAccessor(enabled: true).Matrix;

        Assert.Multiple(() =>
        {
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Read), Is.True);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Create), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Unknown), Is.False);
            Assert.That(matrix.IsAllowed(Constants.UdiEntityType.Webhook, SchemaOperation.Create), Is.True);
        });
    }

    [Test]
    public void Configured_Entity_Type_Governs_Whatever_Case_It_Was_Written_In()
    {
        var settings = new SchemaLockdownSettings { Enabled = true };
        settings.LockedEntityTypes.Add("WebHook");

        SchemaLockdownMatrix matrix = CreateAccessor(settings).Matrix;

        Assert.That(matrix.IsAllowed(Constants.UdiEntityType.Webhook, SchemaOperation.Create), Is.False);
    }

    [Test]
    public void Configurators_Run_In_Order_And_Later_Writes_Win()
    {
        SchemaLockdownMatrix matrix = CreateAccessor(enabled: true, new AllowDeletes(), new BlockDeletes()).Matrix;

        Assert.That(matrix.IsAllowed(Constants.UdiEntityType.DocumentType, SchemaOperation.Delete), Is.False);
    }

    [Test]
    public void Matrix_Is_Frozen_And_Cached()
    {
        SchemaLockdownMatrixAccessor accessor = CreateAccessor(enabled: true);

        Assert.Multiple(() =>
        {
            Assert.That(accessor.Matrix, Is.SameAs(accessor.Matrix));
            Assert.Throws<InvalidOperationException>(() => accessor.Matrix.Allow(Constants.UdiEntityType.DataType, SchemaOperation.Create));
        });
    }
}
