using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Strings
{
	[TestFixture]
    public class LegacyStringExtensionsTests
    {
        [SetUp]
        public void Setup()
        {
            ShortStringHelperResolver.Reset();
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(new LegacyShortStringHelper());
            Resolution.Freeze();
        }

        [TearDown]
        public void TearDown()
        {
            ShortStringHelperResolver.Reset();
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

			var encrypted = valueToTest.ToString().EncryptWithMachineKey();
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


        [TestCase]
        public void StringExtensions_To_Url_Alias()
        {
            var replacements = new Dictionary<string, string>
            {
                {" ", "-"},
                {"\"", ""},
                {"&quot;", ""},
                {"@", ""},
                {"%", ""},
                {".", ""},
                {";", ""},
                {"/", ""},
                {":", ""},
                {"#", ""},
                {"+", ""},
                {"*", ""},
                {"&amp;", ""},
                {"?", ""}
            };

            var name1 = "Home Page";
            var name2 = "Shannon's Home Page!";
            var name3 = "#Someones's Twitter $h1z%n";
            var name4 = "Räksmörgås";
            var name5 = "'em guys-over there, are#goin' a \"little\"bit crazy eh!! :)";
            var name6 = "汉#字*/漢?字";

            var url1 = name1.ToUrlAlias(replacements, true, true, false);
            var url2 = name2.ToUrlAlias(replacements, true, true, false);
            var url3 = name3.ToUrlAlias(replacements, true, true, false);
            var url4 = name4.ToUrlAlias(replacements, true, true, false);
            var url5 = name5.ToUrlAlias(replacements, true, true, false);
            var url6 = name6.ToUrlAlias(replacements, true, true, false);
            var url7 = name6.ToUrlAlias(replacements, true, false, false);
            var url8 = name6.ToUrlAlias(replacements, true, false, true);

            Assert.AreEqual("home-page", url1);
            Assert.AreEqual("shannons-home-page", url2);
            Assert.AreEqual("someoness-twitter-h1zn", url3);
            Assert.AreEqual("rksmrgs", url4);
            Assert.AreEqual("em-guys-over-there-aregoin-a-littlebit-crazy-eh", url5);
            Assert.AreEqual("", url6);
            Assert.AreEqual("汉字漢字", url7);
            Assert.AreEqual("%e6%b1%89%e5%ad%97%e6%bc%a2%e5%ad%97", url8);

        }

        [TestCase]
        public void StringExtensions_To_Camel_Case()
        {
            //Arrange

            var name1 = "Tab 1";
            var name2 = "Home - Page";
            var name3 = "Shannon's document type";

            //Act

            var camelCase1 = name1.ConvertCase(StringAliasCaseType.CamelCase);
            var camelCase2 = name2.ConvertCase(StringAliasCaseType.CamelCase);
            var camelCase3 = name3.ConvertCase(StringAliasCaseType.CamelCase);

            //Assert

            Assert.AreEqual("tab1", camelCase1);
            Assert.AreEqual("homePage", camelCase2);
            Assert.AreEqual("shannon'sDocumentType", camelCase3);
        }

        [TestCase]
        public void StringExtensions_To_Entity_Alias()
        {
            //Arrange

            var name1 = "Tab 1";
            var name2 = "Home - Page";
            var name3 = "Shannon's Document Type";
            var name4 = "!BADDLY nam-ed Document Type";
            var name5 = "i %Want!thisTo end up In Proper@case";

            //Act

            var alias1 = name1.ToUmbracoAlias();
            var alias2 = name2.ToUmbracoAlias();
            var alias3 = name3.ToUmbracoAlias();
            var alias4 = name4.ToUmbracoAlias();
            var alias5 = name5.ToUmbracoAlias(/*StringAliasCaseType.PascalCase*/);

            //Assert

            Assert.AreEqual("tab1", alias1);
            Assert.AreEqual("homePage", alias2);
            Assert.AreEqual("shannonsDocumentType", alias3);
            Assert.AreEqual("baddlyNamEdDocumentType", alias4);
            
            // disable: does not support PascalCase anymore
            //Assert.AreEqual("IWantThisToEndUpInProperCase", alias5);
        }

    }
}
