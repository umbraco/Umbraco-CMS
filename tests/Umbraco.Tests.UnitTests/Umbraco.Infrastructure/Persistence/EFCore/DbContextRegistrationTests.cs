using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.EFCore;

[TestFixture]
public class DbContextRegistrationTests
{
    private const string ProviderName = "TestProvider";

    [Test]
    public void AddRegistrar_BeforeDbContext_InvokesRegistrar()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration();
        var registrar = new Mock<IDbContextServiceRegistrar>();

        sut.AddRegistrar(services, registrar.Object);
        sut.RegisterDbContextType<TestDbContextA>(services);

        registrar.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
    }

    [Test]
    public void RegisterDbContextType_BeforeRegistrar_InvokesRegistrar()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration();
        var registrar = new Mock<IDbContextServiceRegistrar>();

        sut.RegisterDbContextType<TestDbContextA>(services);
        sut.AddRegistrar(services, registrar.Object);

        registrar.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
    }

    [Test]
    public void MultipleDbContexts_SingleRegistrar_InvokesForEach()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration();
        var registrar = new Mock<IDbContextServiceRegistrar>();

        sut.RegisterDbContextType<TestDbContextA>(services);
        sut.RegisterDbContextType<TestDbContextB>(services);
        sut.AddRegistrar(services, registrar.Object);

        registrar.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
        registrar.Verify(x => x.RegisterServices<TestDbContextB>(services), Times.Once);
    }

    [Test]
    public void SingleDbContext_MultipleRegistrars_InvokesEach()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration();
        var registrar1 = new Mock<IDbContextServiceRegistrar>();
        var registrar2 = new Mock<IDbContextServiceRegistrar>();

        sut.AddRegistrar(services, registrar1.Object);
        sut.AddRegistrar(services, registrar2.Object);
        sut.RegisterDbContextType<TestDbContextA>(services);

        registrar1.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
        registrar2.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
    }

    [Test]
    public void CanHandle_MatchingProvider_InvokesRegistrar()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration(ProviderName);
        var registrar = new Mock<IDbContextServiceRegistrar>();
        registrar.Setup(x => x.CanHandle(ProviderName)).Returns(true);

        sut.AddRegistrar(services, registrar.Object);
        sut.RegisterDbContextType<TestDbContextA>(services);

        registrar.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
    }

    [Test]
    public void CanHandle_NonMatchingProvider_DoesNotInvokeRegistrar()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration(ProviderName);
        var registrar = new Mock<IDbContextServiceRegistrar>();
        registrar.Setup(x => x.CanHandle(ProviderName)).Returns(false);

        sut.AddRegistrar(services, registrar.Object);
        sut.RegisterDbContextType<TestDbContextA>(services);

        registrar.Verify(x => x.RegisterServices<TestDbContextA>(It.IsAny<IServiceCollection>()), Times.Never);
    }

    [Test]
    public void NullProviderName_SkipsCanHandleCheck()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration(providerName: null);
        var registrar = new Mock<IDbContextServiceRegistrar>();

        sut.AddRegistrar(services, registrar.Object);
        sut.RegisterDbContextType<TestDbContextA>(services);

        registrar.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
        registrar.Verify(x => x.CanHandle(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void MixedOrdering_AllCombinationsRegistered()
    {
        var services = new ServiceCollection();
        var sut = new DbContextRegistration();
        var registrar1 = new Mock<IDbContextServiceRegistrar>();
        var registrar2 = new Mock<IDbContextServiceRegistrar>();

        // Interleave: registrar1, contextA, registrar2, contextB
        sut.AddRegistrar(services, registrar1.Object);
        sut.RegisterDbContextType<TestDbContextA>(services);
        sut.AddRegistrar(services, registrar2.Object);
        sut.RegisterDbContextType<TestDbContextB>(services);

        // All 4 combinations should be invoked exactly once
        registrar1.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
        registrar1.Verify(x => x.RegisterServices<TestDbContextB>(services), Times.Once);
        registrar2.Verify(x => x.RegisterServices<TestDbContextA>(services), Times.Once);
        registrar2.Verify(x => x.RegisterServices<TestDbContextB>(services), Times.Once);
    }

    private class TestDbContextA : DbContext
    {
    }

    private class TestDbContextB : DbContext
    {
    }
}
