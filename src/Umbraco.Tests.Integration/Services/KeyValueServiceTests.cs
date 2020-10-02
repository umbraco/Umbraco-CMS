using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering methods in the KeyValueService class.
    /// This is more of an integration test as it involves multiple layers
    /// as well as configuration.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class KeyValueServiceTests : UmbracoIntegrationTest
    {
        private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

        [Test]
        public void GetValue_ForMissingKey_ReturnsNull()
        {
            // Arrange
            var keyValueService = KeyValueService;

            // Act
            var value = keyValueService.GetValue("foo");

            // Assert
            Assert.IsNull(value);
        }

        [Test]
        public void GetValue_ForExistingKey_ReturnsValue()
        {
            // Arrange
            var keyValueService = KeyValueService;
            keyValueService.SetValue("foo", "bar");

            // Act
            var value = keyValueService.GetValue("foo");

            // Assert
            Assert.AreEqual("bar", value);
        }

        [Test]
        public void SetValue_ForExistingKey_SavesValue()
        {
            // Arrange
            var keyValueService = KeyValueService;
            keyValueService.SetValue("foo", "bar");

            // Act
            keyValueService.SetValue("foo", "buzz");
            var value = keyValueService.GetValue("foo");

            // Assert
            Assert.AreEqual("buzz", value);
        }

        [Test]
        public void TrySetValue_ForExistingKeyWithProvidedValue_ReturnsTrueAndSetsValue()
        {
            // Arrange
            var keyValueService = KeyValueService;
            keyValueService.SetValue("foo", "bar");

            // Act
            var result = keyValueService.TrySetValue("foo", "bar", "buzz");
            var value = keyValueService.GetValue("foo");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("buzz", value);
        }

        [Test]
        public void TrySetValue_ForExistingKeyWithoutProvidedValue_ReturnsFalseAndDoesNotSetValue()
        {
            // Arrange
            var keyValueService = KeyValueService;
            keyValueService.SetValue("foo", "bar");

            // Act
            var result = keyValueService.TrySetValue("foo", "bang", "buzz");
            var value = keyValueService.GetValue("foo");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("bar", value);
        }
    }
}
