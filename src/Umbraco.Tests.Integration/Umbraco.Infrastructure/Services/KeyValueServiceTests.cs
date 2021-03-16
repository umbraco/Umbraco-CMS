// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
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
            string value = KeyValueService.GetValue("foo");

            // Assert
            Assert.IsNull(value);
        }

        [Test]
        public void GetValue_ForExistingKey_ReturnsValue()
        {
            KeyValueService.SetValue("foo", "bar");

            // Act
            string value = KeyValueService.GetValue("foo");

            // Assert
            Assert.AreEqual("bar", value);
        }

        [Test]
        public void SetValue_ForExistingKey_SavesValue()
        {
            KeyValueService.SetValue("foo", "bar");

            // Act
            KeyValueService.SetValue("foo", "buzz");
            string value = KeyValueService.GetValue("foo");

            // Assert
            Assert.AreEqual("buzz", value);
        }

        [Test]
        public void TrySetValue_ForExistingKeyWithProvidedValue_ReturnsTrueAndSetsValue()
        {
            KeyValueService.SetValue("foo", "bar");

            // Act
            bool result = KeyValueService.TrySetValue("foo", "bar", "buzz");
            string value = KeyValueService.GetValue("foo");

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("buzz", value);
        }

        [Test]
        public void TrySetValue_ForExistingKeyWithoutProvidedValue_ReturnsFalseAndDoesNotSetValue()
        {
            KeyValueService.SetValue("foo", "bar");

            // Act
            bool result = KeyValueService.TrySetValue("foo", "bang", "buzz");
            string value = KeyValueService.GetValue("foo");

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual("bar", value);
        }
    }
}
