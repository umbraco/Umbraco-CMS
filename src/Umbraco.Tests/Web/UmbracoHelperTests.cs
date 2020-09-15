﻿using System;
using System.IO;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Current = Umbraco.Web.Composing.Current;

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
                .Setup(x => x.GetInstance(typeof(TypeLoader)))
                .Returns(new TypeLoader(
                    typeFinder,
                    NoAppCache.Instance,
                    new DirectoryInfo(ioHelper.MapPath("~/App_Data/TEMP")),
                    new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())
                    )
                );

            Current.Factory = container.Object;
        }
    }
}
