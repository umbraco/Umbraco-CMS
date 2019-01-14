using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.MSDI;

namespace Umbraco.Tests.Composing.MSDI
{
    [TestFixture]
    public class ServiceCollectionFactoryTests
    {
        [Test]
        public void CreatesDefaultCollection()
        {
            var services = RegisterFactory.Create();
            Assert.That(services, Is
                .InstanceOf<DefaultServiceCollection>().And
                .InstanceOf<IRegister>().And
                .InstanceOf<IServiceCollection>()
            );
        }

        [Test]
        public void CreatesViaConfiguredFactoryIfAny()
        {
            Assert.Inconclusive();
        }
    }
}
