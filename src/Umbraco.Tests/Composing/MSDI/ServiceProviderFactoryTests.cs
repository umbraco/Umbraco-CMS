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
        public void CreatesLightinjectContainer()
        {
            var services = RegisterFactory.Create();
            var provider = services.CreateFactory();
            Assert.That(provider, Is
                .InstanceOf<LightInjectContainer>().And
                .InstanceOf<IFactory>().And
                .InstanceOf<IServiceProvider>()
            );
        }
    }
}
