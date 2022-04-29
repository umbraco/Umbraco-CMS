using System;
using System.Collections.Generic;
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

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
        _inner.ConfigureAppConfiguration(configureDelegate);

    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
        _inner.ConfigureContainer(configureDelegate);

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
        _inner.ConfigureHostConfiguration(configureDelegate);

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
        _inner.ConfigureServices(configureDelegate);

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        where TContainerBuilder : notnull =>
        _inner.UseServiceProviderFactory(factory);

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        where TContainerBuilder : notnull =>
        _inner.UseServiceProviderFactory(factory);

    public IDictionary<object, object> Properties => _inner.Properties;

    public IHost Build()
    {
        IHost host = _inner.Build();

        _onBuild?.Invoke(host);

        return host;
    }
}
