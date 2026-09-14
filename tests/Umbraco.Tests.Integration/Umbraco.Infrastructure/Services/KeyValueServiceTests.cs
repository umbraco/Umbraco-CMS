// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

/// <summary>
///     Tests covering methods in the KeyValueService class.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class KeyValueServiceTests : UmbracoIntegrationTest
{
    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

    [Test]
    public void Can_Query_For_Key_Prefix()
    {
        // Arrange
        KeyValueService.SetValue("test1", "hello1");
        KeyValueService.SetValue("test2", "hello2");
        KeyValueService.SetValue("test3", "hello3");
        KeyValueService.SetValue("test4", "hello4");
        KeyValueService.SetValue("someotherprefix1", "helloagain1");
        // Act
        var value = KeyValueService.FindByKeyPrefix("test");

        // Assert

        Assert.That(value, Has.Count.EqualTo(4));
        Assert.That(value["test1"], Is.EqualTo("hello1"));
        Assert.That(value["test2"], Is.EqualTo("hello2"));
        Assert.That(value["test3"], Is.EqualTo("hello3"));
        Assert.That(value["test4"], Is.EqualTo("hello4"));
    }

    [Test]
    public void GetValue_ForMissingKey_ReturnsNull()
    {
        // Act
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.That(value, Is.Null);
    }

    [Test]
    public void GetValue_ForExistingKey_ReturnsValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.That(value, Is.EqualTo("bar"));
    }

    [Test]
    public void SetValue_ForExistingKey_SavesValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        KeyValueService.SetValue("foo", "buzz");
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.That(value, Is.EqualTo("buzz"));
    }

    [Test]
    public void TrySetValue_ForExistingKeyWithProvidedValue_ReturnsTrueAndSetsValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        var result = KeyValueService.TrySetValue("foo", "bar", "buzz");
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.That(result, Is.True);
        Assert.That(value, Is.EqualTo("buzz"));
    }

    [Test]
    public void TrySetValue_ForExistingKeyWithoutProvidedValue_ReturnsFalseAndDoesNotSetValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        var result = KeyValueService.TrySetValue("foo", "bang", "buzz");
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.That(result, Is.False);
        Assert.That(value, Is.EqualTo("bar"));
    }
}
