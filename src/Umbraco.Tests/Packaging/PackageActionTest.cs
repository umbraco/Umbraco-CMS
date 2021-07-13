using Moq;
using NUnit.Framework;
using System.Xml.Linq;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Packaging;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Packaging
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class PackageActionTest : TestWithDatabaseBase
    {
        private const string _packageName = "Dummy";
        private const string _alias = "DummyAlias";
        private const string _actionXml = "<action alias=\"" + _alias + "\" />";

        private Mock<IPackageAction> _dummyAction;
        private PackageActionCollection PackageActionCollection => new PackageActionCollection(new[] { _dummyAction.Object });
        private IPackageActionRunner PackageActionRunner => new PackageActionRunner(Logger, PackageActionCollection);

        public override void SetUp()
        {
            base.SetUp();

            _dummyAction = new Mock<IPackageAction>();
            _dummyAction.Setup(call => call.Alias()).Returns(_alias);
            /* Currently the return values of the Execute and Undo are not taken into account. Going forward, there
             * may need an optional attribute (default: false) which determines if a value of false should halt the
             * installation, i.e. skip remaining actions and return false from the IPackageActionRunner.
             * The exception logic will remain as it is today.
             */
            _dummyAction.Setup(call => call.Execute(It.IsAny<string>(), It.IsAny<XElement>())).Returns(true);
            _dummyAction.Setup(call => call.Undo(It.IsAny<string>(), It.IsAny<XElement>())).Returns(true);
        }

        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void GivenAnAction_RunPackageAction_ThenTheExecuteMethodIsCalledOnceDuringInstall()
        {
            // Arrange            
            var actionXml = XElement.Parse(_actionXml);

            // Act
            PackageActionRunner.RunPackageAction(_packageName, _alias, actionXml, out var errors);

            // Assert
            _dummyAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Once());
            _dummyAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never);
            Assert.IsEmpty(errors, "There should not be any errors. This may be an error in the Mock definition");
        }

        [Test]
        public void GivenAnAction_UndoPackageAction_ThenTheExecuteMethodIsCalledOnceDuringInstall()
        {
            // Arrange            
            var actionXml = XElement.Parse(_actionXml);

            // Act
            PackageActionRunner.UndoPackageAction(_packageName, _alias, actionXml, out var errors);

            // Assert
            _dummyAction.Verify(action => action.Execute(It.IsAny<string>(), It.IsAny<XElement>()), Times.Never());
            _dummyAction.Verify(action => action.Undo(It.IsAny<string>(), It.IsAny<XElement>()), Times.Once);
            Assert.IsEmpty(errors, "There should not be any errors. This may be an error in the Mock definition");
        }
    }
}
