using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Configurations
{
    [TestFixture]
    public class LanguageXmlTests
    {
        [Test]
        public void Can_Load_Language_Xml_Files()
        {
            var languageDirectory = GetLanguageDirectory();
            var readFilesCount = 0;
            var xmlDocument = new XmlDocument();
            foreach (var languageFile in languageDirectory.EnumerateFiles("*.xml"))
            {
                // Load will throw an exception if the XML isn't valid.
                xmlDocument.Load(languageFile.FullName);
                readFilesCount++;
            }

            // Ensure that at least one file was read.
            Assert.AreNotEqual(0, readFilesCount);
        }

        private static DirectoryInfo GetLanguageDirectory()
        {
            var testDirectoryPathParts = Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var solutionDirectoryPathParts = testDirectoryPathParts
                .Take(Array.IndexOf(testDirectoryPathParts, "src") + 1);
            var languageFolderPathParts = new List<string>(solutionDirectoryPathParts);
            var additionalPathParts = new[] { "Umbraco.Web.UI", "Umbraco", "config", "lang" };
            languageFolderPathParts.AddRange(additionalPathParts);

            // Hack for build-server - when this path is generated in that envrionment it's missing the "src" folder.
            // Not sure why, but if it's missing we'll add it in the right place.
            if (!languageFolderPathParts.Contains("src"))
            {
                languageFolderPathParts.Insert(languageFolderPathParts.Count - additionalPathParts.Length, "src");
            }

            return new DirectoryInfo(string.Join(Path.DirectorySeparatorChar.ToString(), languageFolderPathParts));
        }
    }
}
