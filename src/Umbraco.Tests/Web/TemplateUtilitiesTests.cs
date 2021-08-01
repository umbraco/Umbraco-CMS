using System;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Tests.Web
{

    [TestFixture]
    public class TemplateUtilitiesTests
    {
        [SetUp]
        public void SetUp()
        {
            Current.Reset();

            // FIXME: now UrlProvider depends on EntityService for GetUrl(guid) - this is bad
            // should not depend on more than IdkMap maybe - fix this!
            var entityService = new Mock<IEntityService>();
            entityService.Setup(x => x.GetId(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>())).Returns(Attempt<int>.Fail());
            var serviceContext = ServiceContext.CreatePartial(entityService: entityService.Object);

            // FIXME: bad in a unit test - but Udi has a static ctor that wants it?!
            var factory = new Mock<IFactory>();
            factory.Setup(x => x.GetInstance(typeof(TypeLoader))).Returns(
                new TypeLoader(NoAppCache.Instance, IOHelper.MapPath("~/App_Data/TEMP"), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())));
            factory.Setup(x => x.GetInstance(typeof (ServiceContext))).Returns(serviceContext);

            var settings = SettingsForTests.GetDefaultUmbracoSettings();
            factory.Setup(x => x.GetInstance(typeof(IUmbracoSettingsSection))).Returns(settings);

            Current.Factory = factory.Object;

            Umbraco.Web.Composing.Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();

            Udi.ResetUdiTypes();
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        
    }
}
