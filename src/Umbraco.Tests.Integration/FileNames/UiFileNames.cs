using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.Integration.FileNames
{
    [TestFixture]
    public class UiFileNames : UmbracoIntegrationTest
    {
        [Test]
        public void MacroTemplates()
        {
            var basePath = IOHelper.MapPath("~/");
            var files = Directory.GetFiles(@"..\\..\\..\\..\\Umbraco.Web.UI.NetCore\\umbraco\\PartialViewMacros\\Templates");
            foreach(var file in files)
            {
                var fileName = file.Split("\\").Last();
                Assert.AreEqual(char.ToUpper(fileName[0]), fileName[0], $"{fileName} does not start with an uppercase letter.");
                var titleCase = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fileName.ToLower());
            }
        }


    }
}
