﻿using System;
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
            var testDirectoryPathParts = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)).Split(Path.DirectorySeparatorChar);
            var solutionDirectoryPathParts = testDirectoryPathParts
                .Take(Array.IndexOf(testDirectoryPathParts, "src") + 1);
            var languageFolderPathParts = new List<string>(solutionDirectoryPathParts);
            languageFolderPathParts.AddRange(new[] { "Umbraco.Web.UI", "Umbraco", "config", "lang" });
            return new DirectoryInfo(string.Join(Path.DirectorySeparatorChar.ToString(), languageFolderPathParts));
        }
    }
}
