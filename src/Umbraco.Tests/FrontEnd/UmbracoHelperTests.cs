using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests.FrontEnd
{
    [TestFixture]
    public class UmbracoHelperTests
        : BaseUmbracoApplicationTest
    {
        private const string SampleWithAnchorElement = "Hello world, this is some text <a href='blah'>with a link</a>";
        private const string SampleWithBoldAndAnchorElements = "Hello world, <b>this</b> is some text <a href='blah'>with a link</a>";

        [Test]
        public static void Truncate_Simple()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SampleWithAnchorElement, 25).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public static void When_Truncating_A_String_Ends_With_A_Space_We_Should_Trim_The_Space_Before_Appending_The_Ellipsis()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SampleWithAnchorElement, 26).ToString();

            Assert.AreEqual("Hello world, this is some&hellip;", result);
        }

        [Test]
        public static void Truncate_Inside_Word()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SampleWithAnchorElement, 24).ToString();

            Assert.AreEqual("Hello world, this is som&hellip;", result);
        }

        [Test]
        public static void Truncate_With_Tag()
        {
            var helper = new UmbracoHelper();

            var result = helper.Truncate(SampleWithAnchorElement, 35).ToString();

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
            const string expectedResult = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

            Assert.AreEqual(expectedResult, result);
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
            const string expectedResult = "c=FormController&a=FormAction&ar=&key1=value1&key2=value2&Key3=Value3&keY4=valuE4";

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public static void Truncate_By_Words()
        {
            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(SampleWithAnchorElement, 4).ToString();

            Assert.AreEqual("Hello world, this is&hellip;", result);
        }

        [Test]
        public static void Truncate_By_Words_With_Tag()
        {
            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(SampleWithBoldAndAnchorElements, 4).ToString();

            Assert.AreEqual("Hello world, <b>this</b> is&hellip;", result);
        }

        [Test]
        public static void Truncate_By_Words_Mid_Tag()
        {
            var helper = new UmbracoHelper();

            var result = helper.TruncateByWords(SampleWithAnchorElement, 7).ToString();

            Assert.AreEqual("Hello world, this is some text <a href='blah'>with&hellip;</a>", result);
        }

        [Test]
        public static void Strip_All_Html()
        {
            var helper = new UmbracoHelper();

            var result = helper.StripHtml(SampleWithBoldAndAnchorElements, null).ToString();

            Assert.AreEqual("Hello world, this is some text with a link", result);
        }

        [Test]
        public static void Strip_Specific_Html()
        {
            string[] tags = { "b" };

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(SampleWithBoldAndAnchorElements, tags).ToString();

            Assert.AreEqual(SampleWithAnchorElement, result);
        }

        [Test]
        public static void Strip_Invalid_Html()
        {
            var text = "Hello world, <bthis</b> is some text <a href='blah'>with a link</a>";

            var helper = new UmbracoHelper();

            var result = helper.StripHtml(text).ToString();

            Assert.AreEqual("Hello world, is some text with a link", result);
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
        public static void Converting_String_Udi_To_A_Udi_Returns_Original_Udi_Value()
        {
            // Arrange
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
        public static void Converting_Hello_To_A_Udi_Returns_False()
        {
            // Arrange
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
    }
}
