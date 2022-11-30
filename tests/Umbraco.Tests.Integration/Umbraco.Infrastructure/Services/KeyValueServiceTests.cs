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
public class KeyValueServiceTests : UmbracoIntegrationTest
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

        Assert.AreEqual(4, value.Count);
        Assert.AreEqual("hello1", value["test1"]);
        Assert.AreEqual("hello2", value["test2"]);
        Assert.AreEqual("hello3", value["test3"]);
        Assert.AreEqual("hello4", value["test4"]);
    }

    [Test]
    public void GetValue_ForMissingKey_ReturnsNull()
    {
        // Act
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.IsNull(value);
    }

    [Test]
    public void GetValue_ForExistingKey_ReturnsValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.AreEqual("bar", value);
    }

    [Test]
    public void SetValue_ForExistingKey_SavesValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        KeyValueService.SetValue("foo", "buzz");
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.AreEqual("buzz", value);
    }

    [Test]
    public void TrySetValue_ForExistingKeyWithProvidedValue_ReturnsTrueAndSetsValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        var result = KeyValueService.TrySetValue("foo", "bar", "buzz");
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual("buzz", value);
    }

    [Test]
    public void TrySetValue_ForExistingKeyWithoutProvidedValue_ReturnsFalseAndDoesNotSetValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        var result = KeyValueService.TrySetValue("foo", "bang", "buzz");
        var value = KeyValueService.GetValue("foo");

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual("bar", value);
    }
}
