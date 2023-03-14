using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Web.Common.Hosting;

internal class UmbracoHostBuilderDecorator : IHostBuilder
{
    private readonly IHostBuilder _inner;
    private readonly Action<IHost>? _onBuild;

    public UmbracoHostBuilderDecorator(IHostBuilder inner, Action<IHost>? onBuild = null)
    {
        _inner = inner;
        _onBuild = onBuild;
    }

    public IDictionary<object, object> Properties => _inner.Properties;

    public IHostBuilder
        ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _inner.ConfigureAppConfiguration(configureDelegate);
        return this;
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(
        Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        _inner.ConfigureContainer(configureDelegate);
        return this;
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        _inner.ConfigureHostConfiguration(configureDelegate);
        return this;
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        _inner.ConfigureServices(configureDelegate);
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        where TContainerBuilder : notnull
    {
        _inner.UseServiceProviderFactory(factory);
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
        Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        where TContainerBuilder : notnull
    {
        _inner.UseServiceProviderFactory(factory);
        return this;
    }

    public IHost Build()
    {
        IHost host = _inner.Build();

        _onBuild?.Invoke(host);

        return host;
    }
}
