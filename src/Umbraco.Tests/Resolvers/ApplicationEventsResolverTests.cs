using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Resolvers
{
    [TestFixture]
    public class ApplicationEventsResolverTests
    {
        [Test]
        public void Core_Event_Handler_Weight_Test()
        {
            //from the 'umbraco' (Umbraco.Web) assembly
            Assert.AreEqual(-100, ApplicationEventsResolver.GetObjectWeightInternal(new GridPropertyEditor(), 100));
            //from the 'Umbraco.Core' assembly
            Assert.AreEqual(-100, ApplicationEventsResolver.GetObjectWeightInternal(new IdentityModelMappings(), 100));
            //from the 'Umbraco.Test' assembly
            Assert.AreEqual(-100, ApplicationEventsResolver.GetObjectWeightInternal(new MyTestEventHandler(), 100));

            //from the 'umbraco.BusinessLogic' assembly - which we are not checking for and not setting as the negative of the default
            Assert.AreEqual(100, ApplicationEventsResolver.GetObjectWeightInternal(new ApplicationRegistrar(), 100));
        }

        private class MyTestEventHandler : ApplicationEventHandler
        {

        }
    }
}
