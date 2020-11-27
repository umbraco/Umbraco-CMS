using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Strings;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class StringExtensionsTests : UmbracoTestBase
    {
        protected override void Compose()
        {
            base.Compose();
            Composition.RegisterUnique<IShortStringHelper>(_ => new MockShortStringHelper());
        }

        [Test]
        public void CurrentHelper()
        {
            var helper = Current.ShortStringHelper;
            Assert.IsInstanceOf<MockShortStringHelper>(helper);
        }

        [TestCase("hello-world.png", "Hello World")]
        [TestCase("hello-world .png", "Hello World")]
        [TestCase("_hello-world __1.png", "Hello World 1")]
        public void To_Friendly_Name(string first, string second)
        {
            Assert.AreEqual(first.ToFriendlyName(), second);
        }

        [TestCase("hello", "world", false)]
        [TestCase("hello", "hello", true)]
        [TestCase("hellohellohellohellohellohellohello", "hellohellohellohellohellohellohelloo", false)]
        [TestCase("hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello", "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohelloo", false)]
        [TestCase("hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello", "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello", true)]
        public void String_To_Guid(string first, string second, bool result)
        {
            Debug.Print("First: " + first.ToGuid());
            Debug.Print("Second: " + second.ToGuid());
            Assert.AreEqual(result, first.ToGuid() == second.ToGuid());
        }

        [TestCase("alert('hello');", false)]
        [TestCase("~/Test.js", true)]
        [TestCase("../Test.js", true)]
        [TestCase("/Test.js", true)]
        [TestCase("Test.js", true)]
        [TestCase("Test.js==", false)]
        [TestCase("/Test.js function(){return true;}", false)]
        public void Detect_Is_JavaScript_Path(string input, bool result)
        {
            var output = input.DetectIsJavaScriptPath();
            Assert.AreEqual(result, output.Success);
        }

        [TestCase("hello.txt", "hello")]
        [TestCase("this.is.a.Txt", "this.is.a")]
        [TestCase("this.is.not.a. Txt", "this.is.not.a. Txt")]
        [TestCase("not a file", "not a file")]
        public void Strip_File_Extension(string input, string result)
        {
            var stripped = input.StripFileExtension();
            Assert.AreEqual(stripped, result);
        }

        [TestCase("../wtf.js?x=wtf", ".js")]
        [TestCase(".htaccess", ".htaccess")]
        [TestCase("path/to/file/image.png", ".png")]
        [TestCase("c:\\abc\\def\\ghi.jkl", ".jkl")]
        [TestCase("/root/folder.name/file.ext", ".ext")]
        [TestCase("http://www.domain.com/folder/name/file.txt", ".txt")]
        [TestCase("i/don't\\have\\an/extension", "")]
        [TestCase("https://some.where/path/to/file.ext?query=this&more=that", ".ext")]
        [TestCase("double_query.string/file.ext?query=abc?something.else", ".ext")]
        [TestCase("test.tar.gz", ".gz")]
        [TestCase("wierd.file,but._legal", "._legal")]
        [TestCase("one_char.x", ".x")]
        public void Get_File_Extension(string input, string result)
        {
            var extension = input.GetFileExtension();
            Assert.AreEqual(result, extension);
        }

        [TestCase("'+alert(1234)+'", "+alert1234+")]
        [TestCase("'+alert(56+78)+'", "+alert56+78+")]
        [TestCase("{{file}}", "file")]
        [TestCase("'+alert('hello')+'", "+alerthello+")]
        [TestCase("Test", "Test")]
        public void Clean_From_XSS(string input, string result)
        {
            var cleaned = input.CleanForXss();
            Assert.AreEqual(cleaned, result);
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

        [TestCase("pineapple", new string[] { "banana", "apple", "blueberry", "strawberry" }, StringComparison.CurrentCulture, true)]
        [TestCase("PINEAPPLE", new string[] { "banana", "apple", "blueberry", "strawberry" }, StringComparison.CurrentCulture, false)]
        [TestCase("pineapple", new string[] { "banana", "Apple", "blueberry", "strawberry" }, StringComparison.CurrentCulture, false)]
        [TestCase("pineapple", new string[] { "banana", "Apple", "blueberry", "strawberry" }, StringComparison.OrdinalIgnoreCase, true)]
        [TestCase("pineapple", new string[] { "banana", "blueberry", "strawberry" }, StringComparison.OrdinalIgnoreCase, false)]
        [TestCase("Strawberry unicorn pie", new string[] { "Berry" }, StringComparison.OrdinalIgnoreCase, true)]
        [TestCase("empty pie", new string[0], StringComparison.OrdinalIgnoreCase, false)]
        public void ContainsAny(string haystack, IEnumerable<string> needles, StringComparison comparison, bool expected)
        {
            bool output = haystack.ContainsAny(needles, comparison);
            Assert.AreEqual(expected, output);
        }

        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase("\r\n\r\n", true)]
        [TestCase("\r\n", true)]
        [TestCase(@"
        Hello
        ", false)]
        [TestCase(null, true)]
        [TestCase("a", false)]
        [TestCase("abc", false)]
        [TestCase("abc   ", false)]
        [TestCase("   abc", false)]
        [TestCase("   abc   ", false)]
        public void IsNullOrWhiteSpace(string value, bool expected)
        {
            // Act
            bool result = value.IsNullOrWhiteSpace();

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestCase("hello", "aGVsbG8")]
        [TestCase("tad", "dGFk")]
        [TestCase("AmqGr+Fd!~ééé", "QW1xR3IrRmQhfsOpw6nDqQ")]
        public void UrlTokenEncoding(string value, string expected)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Console.WriteLine("base64: " + Convert.ToBase64String(bytes));
            var encoded = StringExtensions.UrlTokenEncode(bytes);
            Assert.AreEqual(expected, encoded);

            var backBytes = StringExtensions.UrlTokenDecode(encoded);
            var backString = Encoding.UTF8.GetString(backBytes);
            Assert.AreEqual(value, backString);
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
            var output = "JUST-ANYTHING".ToSafeAlias(null);
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
            var output = "JUST-ANYTHING".ToUrlSegment(null);
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
            var output = "JUST-ANYTHING".ToSafeFileName(null);
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

        [Test] // can't do cases with an IDictionary
        public void ReplaceManyWithCharMap()
        {
            const string input = "télévisiön tzvâr ßup &nbsp; pof";
            const string expected = "television tzvar ssup   pof";
            IDictionary<string, string> replacements = new Dictionary<string, string>
                {
                    { "é", "e" },
                    { "ö", "o" },
                    { "â", "a" },
                    { "ß", "ss" },
                    { "&nbsp;", " " },
                };
            var output = input.ReplaceMany(replacements);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("val$id!ate|this|str'ing", "$!'", '-', "val-id-ate|this|str-ing")]
        [TestCase("val$id!ate|this|str'ing", "$!'", '*', "val*id*ate|this|str*ing")]
        #endregion
        public void ReplaceManyByOneChar(string input, string toReplace, char replacement, string expected)
        {
            var output = input.ReplaceMany(toReplace.ToArray(), replacement);
            Assert.AreEqual(expected, output);
        }
    }
}
