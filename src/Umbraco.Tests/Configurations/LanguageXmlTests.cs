using System.IO;
using System.Xml;
using NUnit.Framework;
using Umbraco.Core.IO;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations
{

    [TestFixture]
    public class LanguageXmlTests
    {
        [Test]
        public void Can_Load_Language_Xml_Files()
        {
            var languageDirectory = new DirectoryInfo(TestHelper.MapPathForTest(SystemDirectories.Umbraco + "/../../../../Umbraco.Web.UI/Umbraco/config/lang/"));
            var readFilesCount = 0;
            foreach (var languageFile in languageDirectory.EnumerateFiles("*.xml"))
            {
                var xmlDocument = new XmlDocument();
                // load will throw an exception if the xml isn't valid.
                xmlDocument.Load(languageFile.FullName);
                readFilesCount++;
            }
            // ensure that at least one file was read.
            Assert.AreNotEqual(0, readFilesCount);
        }
    }
}
