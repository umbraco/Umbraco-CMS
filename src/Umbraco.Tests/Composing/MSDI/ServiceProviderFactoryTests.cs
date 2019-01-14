using System;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;

namespace Umbraco.Tests.Composing.MSDI
{
    [TestFixture]
    public class ServiceProviderFactoryTests
    {
        [Test]
        public void CreatesCoreLightinjectContainer()
        {
            var services = RegisterFactory.Create();
            var provider = services.CreateFactory();
            Assert.That(provider, Is
                .InstanceOf<Umbraco.Web.Composing.LightInject.LightInjectContainer>().And
                .InstanceOf<IFactory>().And
                .InstanceOf<IServiceProvider>()
            );
        }

        [Test]
        public void CreatesWebLightinjectContainer()
        {
            var services = LightInjectContainer.Create();
            var provider = LightInjectContainer.CreateFactory(services);
            Assert.That(provider, Is
                .InstanceOf<LightInjectContainer>().And
                .InstanceOf<IFactory>().And
                .InstanceOf<IServiceProvider>()
            );
        }
    }
}
