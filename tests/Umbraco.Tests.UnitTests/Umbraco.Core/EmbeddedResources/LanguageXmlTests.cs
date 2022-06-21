using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.FileProviders;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.EmbeddedResources;

[TestFixture]
public class LanguageXmlTests
{
    [Test]
    public void Can_Load_Language_Xml_Files()
    {
        var readFilesCount = 0;
        var xmlDocument = new XmlDocument();

        var languageProvider = new EmbeddedFileProvider(typeof(IAssemblyProvider).Assembly, "Umbraco.Cms.Core.EmbeddedResources.Lang");
        var files = languageProvider.GetDirectoryContents(string.Empty)
            .Where(x => !x.IsDirectory && x.Name.EndsWith(".xml"));

        foreach (var languageFile in files)
        {
            using var stream = new StreamReader(languageFile.CreateReadStream());

            // Load will throw an exception if the XML isn't valid.
            xmlDocument.Load(stream);
            readFilesCount++;
        }

        // Ensure that at least one file was read.
        Assert.AreNotEqual(0, readFilesCount);
    }
}
