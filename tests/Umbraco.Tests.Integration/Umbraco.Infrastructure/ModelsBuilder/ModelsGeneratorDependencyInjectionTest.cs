using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

#nullable enable
namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.ModelsBuilder
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.None)]
    public class ModelsGeneratorDependencyInjectionTest : UmbracoIntegrationTest
    {
        [Test]
        public void ModelsGeneratorCanBeAccessedFromIoC()
        {
            IModelsGenerator? modelsGenerator = Services.GetService<IModelsGenerator>();
            Assert.IsNotNull(modelsGenerator);
        }
    }
}
