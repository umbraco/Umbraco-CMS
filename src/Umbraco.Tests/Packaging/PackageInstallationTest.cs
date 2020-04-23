using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium.Interactions;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Packaging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Tests.Packaging.Action;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.JavaScript;
using File = System.IO.File;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class PackageInstallationTest : TestWithDatabaseBase
    {
        private Guid _testBaseFolder;
        public override void SetUp()
        {
            base.SetUp();
            _testBaseFolder = Guid.NewGuid();
        }

        public override void TearDown()
        {
            base.TearDown();

            //clear out files/folders
            var path = IOHelper.MapPath("~/" + _testBaseFolder);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private CompiledPackageXmlParser Parser => new CompiledPackageXmlParser(new ConflictingPackageData(ServiceContext.MacroService, ServiceContext.FileService));

        private PackageDataInstallation PackageDataInstallation => new PackageDataInstallation(
            Logger, ServiceContext.FileService, ServiceContext.MacroService, ServiceContext.LocalizationService,
            ServiceContext.DataTypeService, ServiceContext.EntityService,
            ServiceContext.ContentTypeService, ServiceContext.ContentService,
            Factory.GetInstance<PropertyEditorCollection>(),
            Factory.GetInstance<IScopeProvider>());

        private PackageActionCollection PackageActions => GetPackageActionCollection(GetPackageActions(GetMockPackageActions()));

        private IPackageActionRunner PackageActionRunner => new PackageActionRunner(Logger, PackageActions);

        private IPackageInstallation PackageInstallation => new PackageInstallation(
            PackageDataInstallation,
            new PackageFileInstallation(Parser, ProfilingLogger),
            Parser, PackageActionRunner,
            applicationRootFolder: new DirectoryInfo(IOHelper.MapPath("~/" + _testBaseFolder))); //we don't want to extract package files to the real root, so extract to a test folder

        private const string DocumentTypePickerPackage = "Document_Type_Picker_1.1.umb";
        private const string HelloPackage = "Hello_1.0.0.zip";
        private const string ActionPackage = "PackageActions.Test-1.0.0.zip";

        [Test]
        public void Can_Read_Compiled_Package_1()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage)));
            Assert.IsNotNull(package);
            Assert.AreEqual(1, package.Files.Count);
            Assert.AreEqual("095e064b-ba4d-442d-9006-3050983c13d8.dll", package.Files[0].UniqueFileName);
            Assert.AreEqual("/bin", package.Files[0].OriginalPath);
            Assert.AreEqual("Auros.DocumentTypePicker.dll", package.Files[0].OriginalName);
            Assert.AreEqual("Document Type Picker", package.Name);
            Assert.AreEqual("1.1", package.Version);
            Assert.AreEqual("http://www.opensource.org/licenses/mit-license.php", package.LicenseUrl);
            Assert.AreEqual("MIT", package.License);
            Assert.AreEqual(3, package.UmbracoVersion.Major);
            Assert.AreEqual(RequirementsType.Legacy, package.UmbracoVersionRequirementsType);
            Assert.AreEqual("@tentonipete", package.Author);
            Assert.AreEqual("auros.co.uk", package.AuthorUrl);
            Assert.AreEqual("Document Type Picker datatype that enables back office user to select one or many document types.", package.Readme);
            Assert.AreEqual(1, package.DataTypes.Count());
        }

        [Test]
        public void Can_Read_Compiled_Package_2()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), HelloPackage)));
            Assert.IsNotNull(package);
            Assert.AreEqual(0, package.Files.Count);
            Assert.AreEqual("Hello", package.Name);
            Assert.AreEqual("1.0.0", package.Version);
            Assert.AreEqual("http://opensource.org/licenses/MIT", package.LicenseUrl);
            Assert.AreEqual("MIT License", package.License);
            Assert.AreEqual(8, package.UmbracoVersion.Major);
            Assert.AreEqual(0, package.UmbracoVersion.Minor);
            Assert.AreEqual(0, package.UmbracoVersion.Build);
            Assert.AreEqual(RequirementsType.Strict, package.UmbracoVersionRequirementsType);
            Assert.AreEqual("asdf", package.Author);
            Assert.AreEqual("http://hello.com", package.AuthorUrl);
            Assert.AreEqual("asdf", package.Readme);
            Assert.AreEqual(1, package.Documents.Count());
            Assert.AreEqual("root", package.Documents.First().ImportMode);
            Assert.AreEqual(1, package.DocumentTypes.Count());
            Assert.AreEqual(1, package.Templates.Count());
            Assert.AreEqual(1, package.DataTypes.Count());
        }

        [Test]
        public void Can_Read_Compiled_Package_Warnings()
        {
            //copy a file to the same path that the package will install so we can detect file conflicts
            var path = IOHelper.MapPath("~/" + _testBaseFolder);
            Console.WriteLine(path);

            var filePath = Path.Combine(path, "bin", "Auros.DocumentTypePicker.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, "test");

            //this is where our test zip file is
            var packageFile = Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage);
            Console.WriteLine(packageFile);

            var package = PackageInstallation.ReadPackage(new FileInfo(packageFile));
            var preInstallWarnings = package.Warnings;
            Assert.IsNotNull(preInstallWarnings);

            Assert.AreEqual(1, preInstallWarnings.FilesReplaced.Count());
            Assert.AreEqual("bin\\Auros.DocumentTypePicker.dll", preInstallWarnings.FilesReplaced.First());

            // TODO: More Asserts
        }

        [Test]
        public void Install_Files()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage)));

            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();
            def.Files = new List<string>(); //clear out the files of the def for testing, this should be populated by the install

            var result = PackageInstallation.InstallPackageFiles(def, package, -1).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("bin\\Auros.DocumentTypePicker.dll", result[0]);
            Assert.IsTrue(File.Exists(Path.Combine(IOHelper.MapPath("~/" + _testBaseFolder), result[0])));

            //make sure the def is updated too
            Assert.AreEqual(result.Count, def.Files.Count);
        }

        [Test]
        public void Install_Data()
        {
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), DocumentTypePickerPackage)));
            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();

            var summary = PackageInstallation.InstallPackageData(def, package, -1);

            Assert.AreEqual(1, summary.DataTypesInstalled.Count());

            //make sure the def is updated too
            Assert.AreEqual(summary.DataTypesInstalled.Count(), def.DataTypes.Count);
        }

        [Test]
        public void GivenAPackageWithActions_ThenAllActionsAreExecuted_OnlyUndoableInstallAndUninstallActionsAreStored()
        {
            // Arrange
            var package = PackageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), ActionPackage)));
            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();

            var packageActions = def.Actions;
            var actionsXml = XElement.Parse(packageActions);

            // Act
            var summary = PackageInstallation.InstallPackageData(def, package, -1);

            var revisedPackageActions = def.Actions;
            var revisedXml = XElement.Parse(revisedPackageActions);

            var excludedActions = actionsXml.Descendants("Action").Cast<XNode>().Except(revisedXml.Descendants("Action").Cast<XNode>(), new XNodeEqualityComparer());

            // Assert
            Assert.AreEqual(3, actionsXml.Elements().Count(), $"The {ActionPackage} package does not contain the expected number of actions.");

            Assert.IsEmpty(summary.ActionErrors, "There should not be any errors. This is a problem with the FakeAction definitions.");
            Assert.AreEqual(3, summary.Actions.Count(), "Not all actions have been executed");

            Assert.AreEqual(2, revisedXml.Elements().Count(), "The revisedXml should only contain two actions - an undoable install and an uninstall");

            Assert.AreEqual(1, excludedActions.Count(), "There excludedActions should only contain a single action - a non-undoable install action");
            var excludedAction = excludedActions.First().SafeCast<XElement>();
            var alias = excludedAction.Attribute("alias");
            Assert.AreEqual("Install", alias.Value, "The alias of the action should be \"install\"");
        }

        [Test]
        public void GivenAnAction_InstallPackageData_ThenOnlyTheInstallActionsAreCalled()
        {
            // Arrange
            var mockPackageActions = GetMockPackageActions();
            var packageActions = GetPackageActions(mockPackageActions);
            var packageInstallation = BuildPackageInstallation(BuildPackageActionRunner(packageActions));

            var package = packageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), ActionPackage)));
            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();

            var installAction = mockPackageActions.Where(m => m.Object.Alias() == "Install").First();
            var undoableInstallAction = mockPackageActions.Where(m => m.Object.Alias() == "UndoableInstall").First();
            var uninstallAction = mockPackageActions.Where(m => m.Object.Alias() == "Uninstall").First();

            // Act
            var summary = packageInstallation.InstallPackageData(def, package, -1);

            // Assert
            installAction.Verify(action => action.Alias(), Times.AtLeastOnce);
            installAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Once);
            installAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);

            undoableInstallAction.Verify(action => action.Alias(), Times.AtLeastOnce);
            undoableInstallAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Once);
            undoableInstallAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);

            uninstallAction.Verify(action => action.Alias(), Times.AtLeastOnce);
            uninstallAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);
            uninstallAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);
        }

        [Test]
        public void GivenAnAction_UninstallPackage_ThenOnlyTheUndoableInstallAndUninstallActionsAreCalled()
        {
            // Arrange
            var mockPackageActions = GetMockPackageActions();
            var packageActions = GetPackageActions(mockPackageActions);
            var packageInstallation = BuildPackageInstallation(BuildPackageActionRunner(packageActions));

            var package = packageInstallation.ReadPackage(
                //this is where our test zip file is
                new FileInfo(Path.Combine(IOHelper.MapPath("~/Packaging/packages"), ActionPackage)));
            var def = PackageDefinition.FromCompiledPackage(package);
            def.Id = 1;
            def.PackageId = Guid.NewGuid();

            var installAction = mockPackageActions.Where(m => m.Object.Alias() == "Install").First();
            var undoableInstallAction = mockPackageActions.Where(m => m.Object.Alias() == "UndoableInstall").First();
            var uninstallAction = mockPackageActions.Where(m => m.Object.Alias() == "Uninstall").First();

            // Act
            var summary = packageInstallation.UninstallPackage(def, -1);

            // Assert
            installAction.Verify(action => action.Alias(), Times.AtLeastOnce);
            installAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);
            installAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);

            undoableInstallAction.Verify(action => action.Alias(), Times.AtLeastOnce);
            undoableInstallAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);
            undoableInstallAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Once);

            uninstallAction.Verify(action => action.Alias(), Times.AtLeastOnce);
            uninstallAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Once);
            uninstallAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);
        }

        private IEnumerable<Mock<IPackageAction>> GetMockPackageActions()
        {
            return new List<Mock<IPackageAction>>()
            {
                MockActionHelper.BuildMock("Install"),
                MockActionHelper.BuildMock("UndoableInstall"),
                MockActionHelper.BuildMock("Uninstall")
            };
        }

        private IEnumerable<IPackageAction> GetPackageActions(IEnumerable<Mock<IPackageAction>> mocks)
        {
            return mocks.Select(m => m.Object);
        }

        private PackageActionCollection GetPackageActionCollection(IEnumerable<IPackageAction> packageActions)
        {
            return new PackageActionCollection(packageActions);
        }

        private IPackageActionRunner BuildPackageActionRunner(IEnumerable<IPackageAction> packageActions)
        {
            var packageActionsCollection = GetPackageActionCollection(packageActions);
            return new PackageActionRunner(Logger, packageActionsCollection);
        }

        private IPackageInstallation BuildPackageInstallation(IPackageActionRunner packageActionRunner)
        {
            return new PackageInstallation(
            PackageDataInstallation,
            new PackageFileInstallation(Parser, ProfilingLogger),
            Parser, packageActionRunner,
            applicationRootFolder: new DirectoryInfo(IOHelper.MapPath("~/" + _testBaseFolder)));
        }
    }
}
