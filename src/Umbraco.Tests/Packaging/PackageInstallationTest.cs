using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Packaging;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    public class PackageInstallationTest
    {
        private const string Xml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<umbPackage>
  <files>
   <file><guid>095e064b-ba4d-442d-9006-3050983c13d8.dll</guid><orgPath>/bin</orgPath><orgName>Auros.DocumentTypePicker.dll</orgName></file></files>
  <info>
    <package>
      <name>Document Type Picker</name>
      <version>1.1</version>
      <license url=""http://www.opensource.org/licenses/mit-license.php"">MIT</license>
      <url>http://www.auros.co.uk</url>
      <requirements>
        <major>3</major>
        <minor>0</minor>
        <patch>0</patch>
      </requirements>
    </package>
    <author>
      <name>@tentonipete</name>
      <website>auros.co.uk</website>
    </author>
    <readme>
      <![CDATA[Document Type Picker datatype that enables back office user to select one or many document types.]]>
    </readme>
  </info>
  <DocumentTypes />
  <Templates />
  <Stylesheets />
  <Macros />
  <DictionaryItems />
  <Languages />
  <DataTypes>
    <DataType Name=""Document Type Picker"" Id=""790aff36-7fed-47fb-8bcd-9c91ce43ba24"" Definition=""3593d8e7-8b35-47b9-beda-5e830ca8c93c"" />
  </DataTypes>
</umbPackage>";

        [Test]
        public void Test()
        {
            // Arrange
            const string pagePath = "Test.umb";

            var packageExtraction = new Mock<IPackageExtraction>();

            string test;
            packageExtraction.Setup(a => a.ReadTextFileFromArchive(pagePath, Constants.Packaging.PackageXmlFileName, out test)).Returns(Xml);
            
            var fileService = new Mock<IFileService>();
            var macroService = new Mock<IMacroService>();
            var packagingService = new Mock<IPackagingService>();

            var sut = new PackageInstallation(packagingService.Object, macroService.Object, fileService.Object, packageExtraction.Object);

            // Act
            InstallationSummary installationSummary = sut.InstallPackage(pagePath, -1);

            // Assert
            Assert.IsNotNull(installationSummary);
            //Assert.Inconclusive("Lots of more tests can be written");
        }

    }
}
