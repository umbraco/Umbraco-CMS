using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Persistence.EFCore.Sqlite;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.EFCore;

[TestFixture]
public class SqliteCollationModelCustomizerTests
{
    private static SqliteCollationModelCustomizer CreateCustomizer() => new();

    [Test]
    public void ProviderName_ReturnsSQLiteProviderName()
    {
        var customizer = CreateCustomizer();
        Assert.That(customizer.ProviderName, Is.EqualTo(Constants.ProviderNames.EFCore.SQLite));
    }

    [Test]
    public void Apply_SetsNocaseCollationOnAllStringProperties()
    {
        var customizer = CreateCustomizer();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<TestEntity>(b =>
        {
            b.Property(e => e.Id);
            b.Property(e => e.Name);
            b.Property(e => e.Description);
            b.Property(e => e.IsActive);
        });

        customizer.Apply(modelBuilder);

        var entityType = modelBuilder.Model.FindEntityType(typeof(TestEntity))!;

        var nameProperty = entityType.FindProperty(nameof(TestEntity.Name))!;
        Assert.That(GetCollationAnnotation(nameProperty), Is.EqualTo("NOCASE"));

        var descriptionProperty = entityType.FindProperty(nameof(TestEntity.Description))!;
        Assert.That(GetCollationAnnotation(descriptionProperty), Is.EqualTo("NOCASE"));
    }

    [Test]
    public void Apply_DoesNotSetCollationOnNonStringProperties()
    {
        var customizer = CreateCustomizer();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<TestEntity>(b =>
        {
            b.Property(e => e.Id);
            b.Property(e => e.Name);
            b.Property(e => e.Description);
            b.Property(e => e.IsActive);
        });

        customizer.Apply(modelBuilder);

        var entityType = modelBuilder.Model.FindEntityType(typeof(TestEntity))!;

        var idProperty = entityType.FindProperty(nameof(TestEntity.Id))!;
        Assert.That(GetCollationAnnotation(idProperty), Is.Null);

        var isActiveProperty = entityType.FindProperty(nameof(TestEntity.IsActive))!;
        Assert.That(GetCollationAnnotation(isActiveProperty), Is.Null);
    }

    [Test]
    public void Apply_HandlesMultipleEntityTypes()
    {
        var customizer = CreateCustomizer();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<TestEntity>(b =>
        {
            b.Property(e => e.Id);
            b.Property(e => e.Name);
            b.Property(e => e.Description);
            b.Property(e => e.IsActive);
        });
        modelBuilder.Entity<AnotherEntity>(b =>
        {
            b.Property(e => e.Id);
            b.Property(e => e.Title);
        });

        customizer.Apply(modelBuilder);

        var testEntityType = modelBuilder.Model.FindEntityType(typeof(TestEntity))!;
        var nameProperty = testEntityType.FindProperty(nameof(TestEntity.Name))!;
        Assert.That(GetCollationAnnotation(nameProperty), Is.EqualTo("NOCASE"));

        var anotherEntityType = modelBuilder.Model.FindEntityType(typeof(AnotherEntity))!;
        var titleProperty = anotherEntityType.FindProperty(nameof(AnotherEntity.Title))!;
        Assert.That(GetCollationAnnotation(titleProperty), Is.EqualTo("NOCASE"));
    }

    private static object? GetCollationAnnotation(IReadOnlyProperty property)
        => property.FindAnnotation(RelationalAnnotationNames.Collation)?.Value;

    private class TestEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }

    private class AnotherEntity
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
    }
}
