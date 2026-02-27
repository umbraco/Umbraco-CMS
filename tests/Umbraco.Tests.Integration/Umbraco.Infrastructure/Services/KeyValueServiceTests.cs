// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
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
    public async Task Can_Query_For_Key_Prefix()
    {
        // Arrange
        await KeyValueService.SetValue("test1", "hello1");
        await KeyValueService.SetValue("test2", "hello2");
        await KeyValueService.SetValue("test3", "hello3");
        await KeyValueService.SetValue("test4", "hello4");
        await KeyValueService.SetValue("someotherprefix1", "helloagain1");
        // Act
        var attempt = await KeyValueService.FindByKeyPrefix("test");

        Assert.AreEqual(attempt.Status, KeyValueOperationStatus.Success);
        var value = attempt.Result;

        // Assert
        Assert.AreEqual(4, value.Count);
        Assert.AreEqual("hello1", value["test1"]);
        Assert.AreEqual("hello2", value["test2"]);
        Assert.AreEqual("hello3", value["test3"]);
        Assert.AreEqual("hello4", value["test4"]);
    }

    [Test]
    public async Task GetValue_ForMissingKey_ReturnsNull()
    {
        // Act
        var value = await KeyValueService.GetValue("foo");

        // Assert
        Assert.IsNull(value);
    }

    [Test]
    public async Task GetValue_ForExistingKey_ReturnsValue()
    {
        await KeyValueService.SetValue("foo", "bar");

        // Act
        var value = await KeyValueService.GetValue("foo");

        // Assert
        Assert.AreEqual("bar", value);
    }

    [Test]
    public async Task SetValue_ForExistingKey_SavesValue()
    {
        await KeyValueService.SetValue("foo", "bar");

        // Act
        await KeyValueService.SetValue("foo", "buzz");
        var value = await KeyValueService.GetValue("foo");

        // Assert
        Assert.AreEqual("buzz", value);
    }

    [Test]
    public async Task TrySetValue_ForExistingKeyWithProvidedValue_ReturnsTrueAndSetsValue()
    {
        KeyValueService.SetValue("foo", "bar");

        // Act
        var attempt = await KeyValueService.TrySetValue("foo", "bar", "buzz");
        var value = await KeyValueService.GetValue("foo");

        // Assert
        Assert.AreEqual(attempt.Status, KeyValueOperationStatus.Success);
        Assert.AreEqual("buzz", value);
    }

    [Test]
    public async Task TrySetValue_ForExistingKeyWithoutProvidedValue_ReturnsFalseAndDoesNotSetValue()
    {
        await KeyValueService.SetValue("foo", "bar");

        // Act
        var attempt = await KeyValueService.TrySetValue("foo", "bang", "buzz");
        var value = await KeyValueService.GetValue("foo");

        // Assert
        Assert.AreEqual(attempt.Status, KeyValueOperationStatus.NoValueSet);
        Assert.AreEqual("bar", value);
    }
}
