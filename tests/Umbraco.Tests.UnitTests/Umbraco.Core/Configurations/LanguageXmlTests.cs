using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configurations
{
    [TestFixture]
    public class LanguageXmlTests
    {
        [Test]
        [Platform("Win")] //TODO figure out why Path.GetFullPath("/mnt/c/...") is not considered an absolute path on linux + mac
        public void Can_Load_Language_Xml_Files()
        {
            var languageDirectoryPath = GetLanguageDirectory();
            var readFilesCount = 0;
            var xmlDocument = new XmlDocument();

            var directoryInfo = new DirectoryInfo(languageDirectoryPath);

            foreach (var languageFile in directoryInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly))
            {
                // Load will throw an exception if the XML isn't valid.
                xmlDocument.Load(languageFile.FullName);
                readFilesCount++;
            }

            // Ensure that at least one file was read.
            Assert.AreNotEqual(0, readFilesCount);
        }

        private static string GetLanguageDirectory()
        {
            var testDirectoryPathParts = Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)
                .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            var solutionDirectoryPathParts = testDirectoryPathParts
                .Take(Array.IndexOf(testDirectoryPathParts, "tests"));
            var languageFolderPathParts = new List<string>(solutionDirectoryPathParts);
            var additionalPathParts = new[] { "Umbraco.Web.UI", "umbraco", "config", "lang" };
            languageFolderPathParts.AddRange(additionalPathParts);

            // Hack for build-server - when this path is generated in that envrionment it's missing the "src" folder.
            // Not sure why, but if it's missing we'll add it in the right place.
            if (!languageFolderPathParts.Contains("src"))
            {
                languageFolderPathParts.Insert(languageFolderPathParts.Count - additionalPathParts.Length, "src");
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(), languageFolderPathParts);
        }
    }
}
