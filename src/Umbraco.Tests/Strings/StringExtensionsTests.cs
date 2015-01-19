using System;
using System.Globalization;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class StringExtensionsTests
    {
	    [SetUp]
	    public void Setup()
	    {
            ShortStringHelperResolver.Reset();
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new MockShortStringHelper());
	        Resolution.Freeze();
	    }

        [TearDown]
        public void TearDown()
        {
            ShortStringHelperResolver.Reset();
        }

        [TestCase("hello.txt", "hello")]
        [TestCase("this.is.a.Txt", "this.is.a")]
        [TestCase("this.is.not.a. Txt", "this.is.not.a. Txt")]
        [TestCase("not a file","not a file")]
        public void Strip_File_Extension(string input, string result)
        {
            var stripped = input.StripFileExtension();
            Assert.AreEqual(stripped, result);
        }

	    [TestCase("This is a string to encrypt")]
		[TestCase("This is a string to encrypt\nThis is a second line")]
		[TestCase("    White space is preserved    ")]
		[TestCase("\nWhite space is preserved\n")]
		public void Encrypt_And_Decrypt(string input)
		{
			var encrypted = input.EncryptWithMachineKey();
			var decrypted = encrypted.DecryptWithMachineKey();
			Assert.AreNotEqual(input, encrypted);
			Assert.AreEqual(input, decrypted);
		}

		[Test()]
		public void Encrypt_And_Decrypt_Long_Value()
		{
			// Generate a really long string
			char[] chars = { 'a', 'b', 'c', '1', '2', '3', '\n' };

			string valueToTest = string.Empty;

			// Create a string 7035 chars long
			for (int i = 0; i < 1005; i++)
				for (int j = 0; j < chars.Length; j++)
					valueToTest += chars[j].ToString();

			var encrypted = valueToTest.EncryptWithMachineKey();
			var decrypted = encrypted.DecryptWithMachineKey();
			Assert.AreNotEqual(valueToTest, encrypted);
			Assert.AreEqual(valueToTest, decrypted);
		}

        [TestCase("Hello this is my string", " string", "Hello this is my")]
        [TestCase("Hello this is my string strung", " string", "Hello this is my string strung")]
        [TestCase("Hello this is my string string", " string", "Hello this is my")]
        [TestCase("Hello this is my string string", "g", "Hello this is my string strin")]
        [TestCase("Hello this is my string string", "ello this is my string string", "H")]
        [TestCase("Hello this is my string string", "Hello this is my string string", "")]
        public void TrimEnd(string input, string forTrimming, string shouldBe)
        {
            var trimmed = input.TrimEnd(forTrimming);
            Assert.AreEqual(shouldBe, trimmed);
        }

        [TestCase("Hello this is my string", "hello", " this is my string")]
        [TestCase("Hello this is my string", "Hello this", " is my string")]
        [TestCase("Hello this is my string", "Hello this is my ", "string")]
        [TestCase("Hello this is my string", "Hello this is my string", "")]
        public void TrimStart(string input, string forTrimming, string shouldBe)
        {
            var trimmed = input.TrimStart(forTrimming);
            Assert.AreEqual(shouldBe, trimmed);
        }

        [TestCase("Hello this is my string", "hello", "replaced", "replaced this is my string", StringComparison.CurrentCultureIgnoreCase)]
        [TestCase("Hello this is hello my string", "hello", "replaced", "replaced this is replaced my string", StringComparison.CurrentCultureIgnoreCase)]
        [TestCase("Hello this is my string", "nonexistent", "replaced", "Hello this is my string", StringComparison.CurrentCultureIgnoreCase)]
        [TestCase("Hellohello this is my string", "hello", "replaced", "replacedreplaced this is my string", StringComparison.CurrentCultureIgnoreCase)]
        // Ensure replacing with the same string doesn't cause infinite loop.
        [TestCase("Hello this is my string", "hello", "hello", "hello this is my string", StringComparison.CurrentCultureIgnoreCase)]
        public void ReplaceWithStringComparison(string input, string oldString, string newString, string shouldBe, StringComparison stringComparison)
        {
            var replaced = input.Replace(oldString, newString, stringComparison);
            Assert.AreEqual(shouldBe, replaced);
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("x", "X")]
        [TestCase("xyzT", "XyzT")]
        [TestCase("XyzT", "XyzT")]
        public void ToFirstUpper(string input, string expected)
        {
            var output = input.ToFirstUpper();
            Assert.AreEqual(expected, output);
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("X", "x")]
        [TestCase("XyZ", "xyZ")]
        [TestCase("xyZ", "xyZ")]
        public void ToFirstLower(string input, string expected)
        {
            var output = input.ToFirstLower();
            Assert.AreEqual(expected, output);
        }

        // FORMAT STRINGS

        // note: here we just ensure that the proper helper gets called properly
        // but the "legacy" tests have moved to the legacy helper tests

        [Test]
        public void ToUrlAlias()
        {
            var output = "JUST-ANYTHING".ToUrlSegment();
            Assert.AreEqual("URL-SEGMENT::JUST-ANYTHING", output);
        }

        [Test]
        public void FormatUrl()
        {
            var output = "JUST-ANYTHING".ToUrlSegment();
            Assert.AreEqual("URL-SEGMENT::JUST-ANYTHING", output);
        }

        [Test]
        public void ToUmbracoAlias()
        {
            var output = "JUST-ANYTHING".ToSafeAlias();
            Assert.AreEqual("SAFE-ALIAS::JUST-ANYTHING", output);
        }

        [Test]
        public void ToSafeAlias()
        {
            var output = "JUST-ANYTHING".ToSafeAlias();
            Assert.AreEqual("SAFE-ALIAS::JUST-ANYTHING", output);
        }

        [Test]
        public void ToSafeAliasWithCulture()
        {
            var output = "JUST-ANYTHING".ToSafeAlias(CultureInfo.InvariantCulture);
            Assert.AreEqual("SAFE-ALIAS-CULTURE::JUST-ANYTHING", output);
        }

        [Test]
        public void ToUrlSegment()
        {
            var output = "JUST-ANYTHING".ToUrlSegment();
            Assert.AreEqual("URL-SEGMENT::JUST-ANYTHING", output);
        }

        [Test]
        public void ToUrlSegmentWithCulture()
        {
            var output = "JUST-ANYTHING".ToUrlSegment(CultureInfo.InvariantCulture);
            Assert.AreEqual("URL-SEGMENT-CULTURE::JUST-ANYTHING", output);
        }

        [Test]
        public void ToSafeFileName()
        {
            var output = "JUST-ANYTHING".ToSafeFileName();
            Assert.AreEqual("SAFE-FILE-NAME::JUST-ANYTHING", output);
        }

        [Test]
        public void ToSafeFileNameWithCulture()
        {
            var output = "JUST-ANYTHING".ToSafeFileName(CultureInfo.InvariantCulture);
            Assert.AreEqual("SAFE-FILE-NAME-CULTURE::JUST-ANYTHING", output);
        }

        [Test]
        public void ConvertCase()
        {
            var output = "JUST-ANYTHING".ToCleanString(CleanStringType.Unchanged);
            Assert.AreEqual("CLEAN-STRING-A::JUST-ANYTHING", output);
        }

        [Test]
        public void SplitPascalCasing()
        {
            var output = "JUST-ANYTHING".SplitPascalCasing();
            Assert.AreEqual("SPLIT-PASCAL-CASING::JUST-ANYTHING", output);
        }

        [Test]
        public void ReplaceManyWithCharMap()
        {
            var output = "JUST-ANYTHING".ReplaceMany(null);
            Assert.AreEqual("REPLACE-MANY-A::JUST-ANYTHING", output);
        }

        [Test]
        public void ReplaceManyByOneChar()
        {
            var output = "JUST-ANYTHING".ReplaceMany(new char[] {}, '*');
            Assert.AreEqual("REPLACE-MANY-B::JUST-ANYTHING", output);
        }
    }
}
