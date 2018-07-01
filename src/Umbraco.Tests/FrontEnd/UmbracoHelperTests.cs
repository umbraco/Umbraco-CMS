using System;
using System.Collections.Generic;
using System.Text;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.FrontEnd
{
    [TestFixture]
    public class UmbracoHelperTests
    {
        private const string SAMPLE = "Hello world, this is some text <a href='blah'>with a link</a>";

        [Test]
        public static void Truncate_Simple()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SAMPLE, 25).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public static void When_truncating_a_string_ending_with_a_space_we_should_trim_the_space_before_appending_the_ellipsis()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SAMPLE, 26).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public static void Truncate_Inside_Word()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SAMPLE, 24).ToString();

            Assert.AreEqual("Hello world, this is som&hellip;", result);
        }

        [Test]
        public static void Truncate_With_Tag()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SAMPLE, 35).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

        [Test]
        public static void Create_Encrypted_RouteString_From_Anonymous_Object()
        {
            var additionalRouteValues = new
            {
                key1 = "value1",
                key2 = "value2",
                Key3 = "Value3",
                keY4 = "valuE4"
            };

            var encryptedRouteString = UmbracoHelper.CreateEncryptedRouteString(
                "FormController",
                "FormAction",
                "",
                additionalRouteValues
                );

            var result = encryptedRouteString.DecryptWithMachineKey();

            const string EXPECTED_RESULT = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

            Assert.AreEqual(EXPECTED_RESULT, result);
        }

        [Test]
        public static void Create_Encrypted_RouteString_From_Dictionary()
        {
            var additionalRouteValues = new Dictionary<string, object>()
            {
                {"key1", "value1"},
                {"key2", "value2"},
                {"Key3", "Value3"},
                {"keY4", "valuE4"}
            };

            var encryptedRouteString = UmbracoHelper.CreateEncryptedRouteString(
                "FormController",
                "FormAction",
                "",
                additionalRouteValues
                );

            var result = encryptedRouteString.DecryptWithMachineKey();

            const string EXPECTED_RESULT = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

            Assert.AreEqual(EXPECTED_RESULT, result);
        }

        [Test]
        public static void Truncate_By_Words()
        {
            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(SAMPLE, 4).ToString();

            Assert.AreEqual("Hello world, this is&hellip;", result);
        }

        [Test]
        public static void Truncate_By_Words_With_Tag()
        {
            const string text = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(text, 4).ToString();

            Assert.AreEqual("Hello world, <b>this</b> is&hellip;", result);
        }

        [Test]
        public static void Truncate_By_Words_Mid_Tag()
        {
            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(SAMPLE, 7).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

        [Test]
        public static void Strip_All_Html()
        {
            const string text = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(text, null).ToString();

            Assert.AreEqual("Hello world, this is some text with a link", result);
        }

        [Test]
        public static void Strip_Specific_Html()
        {
            const string text = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

            string[] tags = { "b" };

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(text, tags).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with a link</a>", result);
        }

        [Test]
        public static void Strip_Invalid_Html()
        {
            const string text = "Hello world, <bthis</b> is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(text).ToString();

            Assert.AreEqual("Hello world, is some text with a link", result);
        }

        // ------- Int32 conversion tests
        [Test]
        public static void Converting_boxed_34_to_an_int_returns_34()
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
        public static void Converting_string_54_to_an_int_returns_54()
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
        public static void Converting_hello_to_an_int_returns_false()
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
        public static void Converting_unsupported_object_to_an_int_returns_false()
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
        public static void Converting_boxed_guid_to_a_guid_returns_original_guid_value()
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
        public static void Converting_string_guid_to_a_guid_returns_original_guid_value()
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
        public static void Converting_hello_to_a_guid_returns_false()
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
        public static void Converting_unsupported_object_to_a_guid_returns_false()
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
        [Test]
        public void Converting_boxed_udi_to_a_udi_returns_original_udi_value()
        {
            // Arrange
            SetUpUdiTests();
            Udi sample = new GuidUdi(Constants.UdiEntityType.AnyGuid, Guid.NewGuid());

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                sample,
                out Udi result
                );

            // Assert
            TearDownUdiTests();

            Assert.IsTrue(success);
            Assert.That(result, Is.EqualTo(sample));
        }

        [Test]
        public void Converting_string_udi_to_a_udi_returns_original_udi_value()
        {
            // Arrange
            SetUpUdiTests();
            Udi sample = new GuidUdi(Constants.UdiEntityType.AnyGuid, Guid.NewGuid());

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                sample.ToString(),
                out Udi result
                );

            // Assert
            TearDownUdiTests();

            Assert.IsTrue(success, "Conversion of UDI failed.");
            Assert.That(result, Is.EqualTo(sample));
        }

        [Test]
        public void Converting_hello_to_a_udi_returns_false()
        {
            // Arrange
            SetUpUdiTests();
            const string SAMPLE = "Hello";

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                SAMPLE,
                out Udi result
                );

            // Assert
            TearDownUdiTests();

            Assert.IsFalse(success);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Converting_unsupported_object_to_a_udi_returns_false()
        {
            // Arrange
            SetUpUdiTests();
            var clearlyWillNotConvertToGuid = new StringBuilder(0);

            // Act
            bool success = UmbracoHelper.ConvertIdObjectToUdi(
                clearlyWillNotConvertToGuid,
                out Udi result
                );

            // Assert
            TearDownUdiTests();

            Assert.IsFalse(success);
            Assert.That(result, Is.Null);
        }

        public void SetUpUdiTests()
        {
            // fixme - bad in a unit test - but Udi has a static ctor that wants it?!
            var container = new Mock<IServiceContainer>();
            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();

            container
                .Setup(x => x.GetInstance(typeof(TypeLoader)))
                .Returns(new TypeLoader(
                    NullCacheProvider.Instance,
                    globalSettings,
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())
                    )
                );

            Current.Container = container.Object;

            Udi.ResetUdiTypes();
        }

        public void TearDownUdiTests() => Current.Reset();
    }
}
