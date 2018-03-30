using System;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class RelationServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void Can_Create_RelationType_Without_Name()
        {
            var rs = ServiceContext.RelationService;
            var rt = new RelationType(Constants.ObjectTypes.Document, Constants.ObjectTypes.Document, "repeatedEventOccurence");

            Assert.DoesNotThrow(() => rs.Save(rt));

            Assert.AreEqual(rt.Name, "repeatedEventOccurence");
        }
    }
}
