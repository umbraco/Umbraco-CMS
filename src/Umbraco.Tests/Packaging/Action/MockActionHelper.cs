using Moq;
using System.Xml.Linq;
using Umbraco.Core.PackageActions;

namespace Umbraco.Tests.Packaging.Action
{
    internal static class MockActionHelper
    {
        public static Mock<IPackageAction> BuildMock(string alias)
        {
            var mock = new Mock<IPackageAction>();
            mock.Setup(call => call.Alias()).Returns(alias);
            /* Currently the return values of the Execute and Undo are not taken into account. Going forward, there
             * may need an optional attribute (default: false) which determines if a value of false should halt the
             * installation, i.e. skip remaining actions and return false from the IPackageActionRunner.
             * The exception logic will remain as it is today.
             */
            mock.Setup(call => call.Execute(It.IsAny<string>(), It.IsAny<XElement>())).Returns(true);
            mock.Setup(call => call.Undo(It.IsAny<string>(), It.IsAny<XElement>())).Returns(true);
            return mock;
        }
    }
}
