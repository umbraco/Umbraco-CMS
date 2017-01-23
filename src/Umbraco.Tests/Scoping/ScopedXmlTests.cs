using System;
using Moq;
using NUnit.Framework;
using umbraco;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Scoping
{
    [TestFixture]
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    public class ScopedXmlTests : BaseDatabaseFactoryTest
    {
        [Test]
        public void Test()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.Content);
            contentMock.Setup(x => x.XmlCacheEnabled).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            var contentType = new ContentType(-1) { Alias = "contenttype" };
            ApplicationContext.Services.ContentTypeService.Save(contentType);

            var eventHandler = new CacheRefresherEventHandler();
            eventHandler.OnApplicationStarted(null, ApplicationContext);

            var xml = content.Instance.XmlContent;
            Assert.IsNotNull(xml);
            ApplicationContext.Services.ContentService.CreateContentWithIdentity("name", -1, "contenttype");

            Console.WriteLine(xml.OuterXml);
            Console.WriteLine(content.Instance.XmlContent.OuterXml);
        }
    }
}
