// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class EntityTests
{
    private IContentType _contentType = null!;

    [SetUp]
    public void SetUp() =>
        _contentType = new ContentTypeBuilder()
            .WithAlias("testType")
            .Build();

    [Test]
    public void Changing_Key_On_Existing_Entity_Throws_InvalidOperationException()
    {
        // Arrange
        var content = new ContentBuilder()
            .WithId(123)
            .WithContentType(_contentType)
            .Build();

        var originalKey = content.Key;
        var newKey = Guid.NewGuid();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => content.Key = newKey);
        Assert.That(ex!.Message, Does.Contain("Cannot change the Key"));
        Assert.That(ex.Message, Does.Contain("Content"));
    }

    [Test]
    public void Setting_Key_On_New_Entity_Succeeds()
    {
        // Arrange
        var content = new ContentBuilder()
            .WithContentType(_contentType)
            .Build();

        // Entity has no identity yet (Id = 0)
        Assert.IsFalse(content.HasIdentity);

        var newKey = Guid.NewGuid();

        // Act
        content.Key = newKey;

        // Assert
        Assert.AreEqual(newKey, content.Key);
    }

    [Test]
    public void Setting_Key_From_Empty_On_Existing_Entity_Succeeds()
    {
        // Arrange - simulate factory loading scenario
        // Create entity with default state (Key is Empty until accessed, HasIdentity is false)
        var content = new Content("Test", -1, _contentType)
        {
            // Simulate factory loading: Set Id first (which sets HasIdentity = true)
            Id = 123,
        };
        Assert.IsTrue(content.HasIdentity);

        // At this point, _key is still Guid.Empty (not accessed yet)
        var keyFromDb = Guid.NewGuid();

        // Act - simulate factory setting Key from database
        content.Key = keyFromDb;

        // Assert
        Assert.AreEqual(keyFromDb, content.Key);
    }

    [Test]
    public void Setting_Key_To_Empty_On_Existing_Entity_Does_Not_Throw()
    {
        // Arrange
        var content = new ContentBuilder()
            .WithId(123)
            .WithContentType(_contentType)
            .Build();

        // Ensure Key has a value
        var originalKey = content.Key;
        Assert.AreNotEqual(Guid.Empty, originalKey);
        Assert.IsTrue(content.HasIdentity);

        // Act - setting to Empty is allowed (for identity reset scenarios)
        // This should not throw
        Assert.DoesNotThrow(() => content.Key = Guid.Empty);

        // Note: Reading Key will auto-generate a new GUID since the getter
        // creates one if the internal field is Empty
    }

    [Test]
    public void ResetIdentity_Clears_Key_And_HasIdentity()
    {
        // Arrange
        var content = new ContentBuilder()
            .WithId(123)
            .WithContentType(_contentType)
            .Build();

        var originalKey = content.Key;
        Assert.IsTrue(content.HasIdentity);

        // Act - reset identity (simulates cloning scenario)
        content.ResetIdentity();

        // Assert
        Assert.IsFalse(content.HasIdentity);
        Assert.AreEqual(0, content.Id);

        // After ResetIdentity, we can set a new key
        var newKey = Guid.NewGuid();
        content.Key = newKey;
        Assert.AreEqual(newKey, content.Key);
    }

    [Test]
    public void DeepCloneWithResetIdentities_Creates_Valid_Clone_Without_Identity()
    {
        // Arrange
        var content = new ContentBuilder()
            .WithId(123)
            .WithContentType(_contentType)
            .Build();

        var originalKey = content.Key;
        Assert.IsTrue(content.HasIdentity);

        // Act
        var clone = content.DeepCloneWithResetIdentities();

        // Assert
        Assert.IsFalse(clone.HasIdentity);
        Assert.AreEqual(0, clone.Id);
        // After ResetIdentity, the Key should be auto-generated when accessed
        // (not the same as the original)
        Assert.AreNotEqual(originalKey, clone.Key);
    }

    [Test]
    public void Setting_Same_Key_On_Existing_Entity_Succeeds()
    {
        // Arrange
        var content = new ContentBuilder()
            .WithId(123)
            .WithContentType(_contentType)
            .Build();

        var originalKey = content.Key;
        Assert.IsTrue(content.HasIdentity);

        // Act - setting the same key should not throw
        content.Key = originalKey;

        // Assert
        Assert.AreEqual(originalKey, content.Key);
    }

    [Test]
    public void Fake_Entity_With_Id_Minus_One_And_Empty_Key_Succeeds()
    {
        // Arrange - this pattern is used in ContentService and MediaService for notifications
        var fakeContentType = new ContentTypeBuilder()
            .WithId(-1)
            .WithAlias("fakeType")
            .Build();

        // Act - simulate the fake entity creation pattern: new Content(...) { Id = -1, Key = Guid.Empty }
        // This should not throw
        Content? fakeContent = null;
        Assert.DoesNotThrow(() =>
        {
            fakeContent = new Content("root", -1, fakeContentType) { Id = -1, Key = Guid.Empty };
        });

        // Assert
        Assert.IsNotNull(fakeContent);
        Assert.AreEqual(-1, fakeContent!.Id);
        Assert.IsTrue(fakeContent.HasIdentity); // Id = -1 != 0, so HasIdentity is true
    }

    [Test]
    public void After_DeepClone_Can_Set_New_Key()
    {
        // Arrange
        var content = new ContentBuilder()
            .WithId(123)
            .WithContentType(_contentType)
            .Build();

        Assert.IsTrue(content.HasIdentity);

        // Act - deep clone and reset identity
        var clone = (Content)content.DeepClone();
        clone.ResetIdentity();

        // After ResetIdentity, should be able to set a new Key
        var newKey = Guid.NewGuid();
        clone.Key = newKey;

        // Assert
        Assert.AreEqual(newKey, clone.Key);
        Assert.IsFalse(clone.HasIdentity);
    }

    [Test]
    public void Setting_Key_After_Id_When_Key_Was_AutoGenerated_Succeeds()
    {
        // Arrange - this is a common test scenario:
        // Create content, access Key (auto-generates), set Id, then set Key
        var content = new Content("Test", -1, _contentType);

        // Access Key - this triggers auto-generation
        var autoGeneratedKey = content.Key;
        Assert.AreNotEqual(Guid.Empty, autoGeneratedKey);

        // Set Id (makes HasIdentity = true)
        content.Id = 10;
        Assert.IsTrue(content.HasIdentity);

        // Act - set Key to a specific value (first explicit assignment)
        var specificKey = new Guid("29181B97-CB8F-403F-86DE-5FEB497F4800");
        content.Key = specificKey;

        // Assert
        Assert.AreEqual(specificKey, content.Key);
    }

    [Test]
    public void Changing_Key_After_Explicit_Assignment_Throws()
    {
        // Arrange
        var content = new Content("Test", -1, _contentType)
        {
            Id = 10,
        };

        // First explicit assignment
        var firstKey = new Guid("29181B97-CB8F-403F-86DE-5FEB497F4800");
        content.Key = firstKey;

        // Act & Assert - second assignment should throw
        var secondKey = Guid.NewGuid();
        var ex = Assert.Throws<InvalidOperationException>(() => content.Key = secondKey);
        Assert.That(ex!.Message, Does.Contain("Cannot change the Key"));
    }

    [Test]
    public void Setting_Key_To_Empty_Then_New_Value_Still_Throws()
    {
        // Arrange - verify that setting Key to Empty doesn't create a bypass
        var content = new Content("Test", -1, _contentType)
        {
            Id = 10,
        };

        // First explicit assignment
        var firstKey = new Guid("29181B97-CB8F-403F-86DE-5FEB497F4800");
        content.Key = firstKey;

        // Setting to Empty is allowed
        content.Key = Guid.Empty;

        // Act & Assert - but then setting to a new value should still throw
        var newKey = Guid.NewGuid();
        var ex = Assert.Throws<InvalidOperationException>(() => content.Key = newKey);
        Assert.That(ex!.Message, Does.Contain("Cannot change the Key"));
    }

    [Test]
    public void Cannot_Change_Key_After_Serialization_Roundtrip()
    {
        // Arrange - create a persisted entity with a Key
        var entity = new TestEntity { Id = 123 };
        var originalKey = new Guid("29181B97-CB8F-403F-86DE-5FEB497F4800");
        entity.Key = originalKey;

        Assert.IsTrue(entity.HasIdentity);

        // Act - serialize and deserialize using DataContractSerializer
        var serializer = new DataContractSerializer(typeof(TestEntity));
        using var stream = new MemoryStream();
        serializer.WriteObject(stream, entity);
        stream.Position = 0;
        var deserialized = (TestEntity)serializer.ReadObject(stream)!;

        // Assert - the deserialized entity should still protect its Key
        Assert.That(deserialized.Id, Is.EqualTo(123));
        Assert.That(deserialized.Key, Is.EqualTo(originalKey));
        Assert.IsTrue(deserialized.HasIdentity);

        // Attempting to change the Key should throw
        var newKey = Guid.NewGuid();
        var ex = Assert.Throws<InvalidOperationException>(() => deserialized.Key = newKey);
        Assert.That(ex!.Message, Does.Contain("Cannot change the Key"));
    }

    /// <summary>
    /// Minimal entity class for testing EntityBase serialization behavior.
    /// </summary>
    [DataContract(IsReference = true)]
    private class TestEntity : EntityBase
    {
    }
}
