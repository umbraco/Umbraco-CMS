// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Web.Website.ViewEngines;

namespace Umbraco.Tests.Runtimes
{
    [TestFixture]
    public class ProfilingViewEngineWrapperMvcViewOptionsSetupTests
    {
        private ProfilingViewEngineWrapperMvcViewOptionsSetup Sut =>
            new ProfilingViewEngineWrapperMvcViewOptionsSetup(Mock.Of<IProfiler>());

        [Test]
        public void WrapViewEngines_HasEngines_WrapsAll()
        {
            var options = new MvcViewOptions()
            {
                ViewEngines =
                {
                    Mock.Of<IRenderViewEngine>(),
                    Mock.Of<IPluginViewEngine>(),
                }
            };

            Sut.Configure(options);

            Assert.That(options.ViewEngines.Count, Is.EqualTo(2));
            Assert.That(options.ViewEngines[0], Is.InstanceOf<ProfilingViewEngine>());
            Assert.That(options.ViewEngines[1], Is.InstanceOf<ProfilingViewEngine>());
        }

        [Test]
        public void WrapViewEngines_HasEngines_KeepsSortOrder()
        {
            var options = new MvcViewOptions()
            {
                ViewEngines =
                {
                    Mock.Of<IRenderViewEngine>(),
                    Mock.Of<IPluginViewEngine>(),
                }
            };

            Sut.Configure(options);

            Assert.That(options.ViewEngines.Count, Is.EqualTo(2));
            Assert.That(((ProfilingViewEngine)options.ViewEngines[0]).Inner, Is.InstanceOf<IRenderViewEngine>());
            Assert.That(((ProfilingViewEngine)options.ViewEngines[1]).Inner, Is.InstanceOf<IPluginViewEngine>());
        }

        [Test]
        public void WrapViewEngines_HasProfiledEngine_AddsSameInstance()
        {
            var profiledEngine = new ProfilingViewEngine(Mock.Of<IPluginViewEngine>(), Mock.Of<IProfiler>());
            var options = new MvcViewOptions()
            {
                ViewEngines =
                {
                    profiledEngine
                }
            };

            Sut.Configure(options);

            Assert.That(options.ViewEngines[0], Is.SameAs(profiledEngine));
        }

        [Test]
        public void WrapViewEngines_CollectionIsNull_DoesNotThrow() => Assert.DoesNotThrow(() => Sut.Configure(new MvcViewOptions()));
    }
}
