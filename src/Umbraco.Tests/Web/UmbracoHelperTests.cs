using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Current = Umbraco.Web.Composing.Current;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Tests.Web
{

    [TestFixture]
    public class UmbracoHelperTests
    {

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        private void SetUpDependencyContainer()
        {
            
            // FIXME: bad in a unit test - but Udi has a static ctor that wants it?!
            var container = new Mock<IServiceProvider>();
            
            var typeFinder = TestHelper.GetTypeFinder();
            var ioHelper = TestHelper.IOHelper;
            container
                .Setup(x => x.GetRequiredService(typeof(TypeLoader)))
                .Returns(new TypeLoader(
                    typeFinder,
                    NoAppCache.Instance,
                    new DirectoryInfo(ioHelper.MapPath("~/App_Data/TEMP")),
                    Mock.Of<ILogger<TypeLoader>>(),
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())
                    )
                );

            Current.Factory = container.Object;
        }
    }
}
