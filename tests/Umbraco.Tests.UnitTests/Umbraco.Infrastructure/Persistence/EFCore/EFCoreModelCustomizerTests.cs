using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.EFCore;

[TestFixture]
public class EFCoreModelCustomizerTests
{
    [Test]
    public void EntityType_ReturnsTypeOfTEntity()
    {
        IEFCoreModelCustomizer customizer = new TestCustomizer();

        Assert.That(customizer.EntityType, Is.EqualTo(typeof(TestEntity)));
    }

    [Test]
    public void Apply_DispatchesToCustomizeWithEntityTypeBuilder()
    {
        var customizer = new TestCustomizer();
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<TestEntity>();

        ((IEFCoreModelCustomizer)customizer).Apply(modelBuilder);

        Assert.That(customizer.CustomizeWasCalled, Is.True);
    }

    [Test]
    public void Apply_ThrowsWhenEntityTypeNotInModel()
    {
        var customizer = new TestCustomizer();
        var modelBuilder = new ModelBuilder();

        Assert.That(
            () => ((IEFCoreModelCustomizer)customizer).Apply(modelBuilder),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void AddEFCoreModelCustomizer_RegistersAsIEFCoreModelCustomizer()
    {
        var services = new ServiceCollection();
        var builder = new Mock<IUmbracoBuilder>();
        builder.Setup(x => x.Services).Returns(services);

        builder.Object.AddEFCoreModelCustomizer<TestCustomizer>();

        Assert.That(services, Has.Exactly(1).Matches<ServiceDescriptor>(d =>
            d.ServiceType == typeof(IEFCoreModelCustomizer) &&
            d.ImplementationType == typeof(TestCustomizer) &&
            d.Lifetime == ServiceLifetime.Singleton));
    }

    private class TestEntity
    {
        public int Id { get; set; }
    }

    private class TestCustomizer : IEFCoreModelCustomizer<TestEntity>
    {
        public bool CustomizeWasCalled { get; private set; }

        public void Customize(EntityTypeBuilder<TestEntity> builder) => CustomizeWasCalled = true;
    }
}
