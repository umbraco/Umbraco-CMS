using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Services
{
    /// <summary>
    /// Tests covering methods in the KeyValueService class.
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
}
