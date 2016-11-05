using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Services
{
    [TestFixture, RequiresSTA]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class RelationServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void Can_Create_RelationType_Without_Name()
        {
            var rs = ServiceContext.RelationService;
            var rt = new RelationType(new Guid(Constants.ObjectTypes.Document), new Guid(Constants.ObjectTypes.Document), "repeatedEventOccurence");

            Assert.DoesNotThrow(() => rs.Save(rt));

            Assert.AreEqual(rt.Name, "repeatedEventOccurence");
        }
    }
}