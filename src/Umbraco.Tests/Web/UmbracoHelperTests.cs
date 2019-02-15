using System;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class UmbracoHelperTests
    {   

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }
        

        
        // ------- Int32 conversion tests
        [Test]
        public static void Converting_Boxed_34_To_An_Int_Returns_34()
        {
            // Arrange
            const int sample = 34;

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToInt(
                sample,
                out int result
                );

            // Assert
            Assert.IsTrue(success);
            Assert.That(result, Is.EqualTo(34));
        }

        [Test]
        public static void Converting_String_54_To_An_Int_Returns_54()
        {
            // Arrange
            const string sample = "54";

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToInt(
                sample,
                out int result
                );

            // Assert
            Assert.IsTrue(success);
            Assert.That(result, Is.EqualTo(54));
        }

        [Test]
        public static void Converting_Hello_To_An_Int_Returns_False()
        {
            // Arrange
            const string sample = "Hello";

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToInt(
                sample,
                out int result
                );

            // Assert
            Assert.IsFalse(success);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public static void Converting_Unsupported_Object_To_An_Int_Returns_False()
        {
            // Arrange
            var clearlyWillNotConvertToInt = new StringBuilder(0);

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToInt(
                clearlyWillNotConvertToInt,
                out int result
                );

            // Assert
            Assert.IsFalse(success);
            Assert.That(result, Is.EqualTo(0));
        }

        // ------- GUID conversion tests
        [Test]
        public static void Converting_Boxed_Guid_To_A_Guid_Returns_Original_Guid_Value()
        {
            // Arrange
            Guid sample = Guid.NewGuid();

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToGuid(
                sample,
                out Guid result
                );

            // Assert
            Assert.IsTrue(success);
            Assert.That(result, Is.EqualTo(sample));
        }

        [Test]
        public static void Converting_String_Guid_To_A_Guid_Returns_Original_Guid_Value()
        {
            // Arrange
            Guid sample = Guid.NewGuid();

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToGuid(
                sample.ToString(),
                out Guid result
                );

            // Assert
            Assert.IsTrue(success);
            Assert.That(result, Is.EqualTo(sample));
        }

        [Test]
        public static void Converting_Hello_To_A_Guid_Returns_False()
        {
            // Arrange
            const string sample = "Hello";

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToGuid(
                sample,
                out Guid result
                );

            // Assert
            Assert.IsFalse(success);
            Assert.That(result, Is.EqualTo(new Guid("00000000-0000-0000-0000-000000000000")));
        }

        [Test]
        public static void Converting_Unsupported_Object_To_A_Guid_Returns_False()
        {
            // Arrange
            var clearlyWillNotConvertToGuid = new StringBuilder(0);

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToGuid(
                clearlyWillNotConvertToGuid,
                out Guid result
                );

            // Assert
            Assert.IsFalse(success);
            Assert.That(result, Is.EqualTo(new Guid("00000000-0000-0000-0000-000000000000")));
        }

        // ------- UDI Conversion Tests
        /// <remarks>
        /// This requires PluginManager.Current to be initialised before running.
        /// </remarks>
        [Test]
        public static void Converting_Boxed_Udi_To_A_Udi_Returns_Original_Udi_Value()
        {
            // Arrange
            Udi.ResetUdiTypes();
            Udi sample = new GuidUdi(Constants.UdiEntityType.AnyGuid, Guid.NewGuid());

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                sample,
                out Udi result
                );

            // Assert
            Assert.IsTrue(success);
            Assert.That(result, Is.EqualTo(sample));
        }

        /// <remarks>
        /// This requires PluginManager.Current to be initialised before running.
        /// </remarks>
        [Test]
        public void Converting_String_Udi_To_A_Udi_Returns_Original_Udi_Value()
        {
            // Arrange
            SetUpDependencyContainer();
            Udi.ResetUdiTypes();
            Udi sample = new GuidUdi(Constants.UdiEntityType.AnyGuid, Guid.NewGuid());

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                sample.ToString(),
                out Udi result
                );

            // Assert
            Assert.IsTrue(success, "Conversion of UDI failed.");
            Assert.That(result, Is.EqualTo(sample));
        }

        /// <remarks>
        /// This requires PluginManager.Current to be initialised before running.
        /// </remarks>
        [Test]
        public void Converting_Hello_To_A_Udi_Returns_False()
        {
            // Arrange
            SetUpDependencyContainer();
            Udi.ResetUdiTypes();
            const string sample = "Hello";

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                sample,
                out Udi result
                );

            // Assert
            Assert.IsFalse(success);
            Assert.That(result, Is.Null);
        }

        /// <remarks>
        /// This requires PluginManager.Current to be initialised before running.
        /// </remarks>
        [Test]
        public static void Converting_Unsupported_Object_To_A_Udi_Returns_False()
        {
            // Arrange
            Udi.ResetUdiTypes();

            var clearlyWillNotConvertToGuid = new StringBuilder(0);

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                clearlyWillNotConvertToGuid,
                out Udi result
                );

            // Assert
            Assert.IsFalse(success);
            Assert.That(result, Is.Null);
        }

        private void SetUpDependencyContainer()
        {
            // FIXME: bad in a unit test - but Udi has a static ctor that wants it?!
            var container = new Mock<IFactory>();
            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();

            container
                .Setup(x => x.GetInstance(typeof(TypeLoader)))
                .Returns(new TypeLoader(
                    NoAppCache.Instance,
                    LocalTempStorage.Default,
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())
                    )
                );

            Current.Factory = container.Object;
        }
    }
}
