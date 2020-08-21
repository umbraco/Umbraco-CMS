using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Umbraco.Tests.Integration.FileNames
{
    [TestFixture]
    public class UiFileNames
    {
        [Test]
        public void MacroTemplates()
        {
            var files = Directory.GetFiles(@"..\\..\\..\\..\\Umbraco.Web.UI.NetCore\\umbraco\\PartialViewMacros\\Templates");
            foreach(var file in files)
            {
                var fileName = file.Split("\\").Last();
                Assert.AreEqual(char.ToUpper(fileName[0]), fileName[0], $"{fileName} does not start with an uppercase letter.");
                var titleCase = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fileName.ToLower());
            }
        }

        [Test]
        public void LanguageFilesAreLowercase()
        {
            var files = Directory.GetFiles(@"..\\..\\..\\..\\Umbraco.Web.UI.NetCore\\umbraco\\config\\lang");
            foreach(var file in files)
            {
                var fileName = file.Split("\\").Last();
                Assert.AreEqual(fileName.ToLower(), fileName);
            }

        }
    }
}
