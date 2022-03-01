using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;

namespace Umbraco.Cms.Infrastructure.DistributedLocking;

/// <summary>
/// Only registers a single implementation (based on configuration).
/// </summary>
public class DistributedLockingCollectionBuilder : ICollectionBuilder
{
    private readonly IUmbracoBuilder _builder;

    private List<Type> _types = new();

    // HACK: WAT? meet the generic constraints, this ctor is never called.
    public DistributedLockingCollectionBuilder()
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once UnusedMember.Global (it's used by UmbracoBuilder)
    public DistributedLockingCollectionBuilder(IUmbracoBuilder builder)
    {
        _builder = builder;
    }

    public DistributedLockingCollectionBuilder AddDistributedLockingMechanism<T>()
        where T : IDistributedLockingMechanism
    {
        _types.Add(typeof(T));
        return this;
    }

    public void RegisterWith(IServiceCollection services)
    {
        var globalSettings = new GlobalSettings();
        _builder.Config.GetSection(Constants.Configuration.ConfigGlobal).Bind(globalSettings);

        Type selected = _types.FirstOrDefault(x => x.FullName?.EndsWith(globalSettings.DistributedLockingMechanism) ?? false);

        if (!string.IsNullOrEmpty(globalSettings.DistributedLockingMechanism) && selected == null)
        {
            throw new Exception($"DistributedLockingMechanism not found for {globalSettings.DistributedLockingMechanism}");
        }

        Type fallback = _types.First();
        services.AddSingleton(typeof(IDistributedLockingMechanism), selected ?? fallback);
    }
}
