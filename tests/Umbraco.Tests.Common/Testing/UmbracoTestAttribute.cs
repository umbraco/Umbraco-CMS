// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Common.Testing;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, /*AllowMultiple = false,*/ Inherited = false)]
public class UmbracoTestAttribute : TestOptionAttributeBase
{
    private readonly Settable<bool> _boot = new();

    private readonly Settable<UmbracoTestOptions.Database> _database = new();

    private readonly Settable<UmbracoTestOptions.Logger> _logger = new();

    private readonly Settable<bool> _mapper = new();

    private readonly Settable<bool> _publishedRepositoryEvents = new();

    private readonly Settable<UmbracoTestOptions.TypeLoader> _typeLoader = new();

    private readonly Settable<bool> _withApplication = new();

    /// <summary>
    ///     Gets or sets a value indicating whether tests are "WithApplication".
    /// </summary>
    /// <remarks>
    ///     <para>Default is false.</para>
    ///     <para>This is for tests that inherited from TestWithApplicationBase.</para>
    ///     <para>Implies Mapper = true (, ResetPluginManager = false).</para>
    /// </remarks>
    public bool WithApplication { get => _withApplication.ValueOrDefault(false); set => _withApplication.Set(value); }

    /// <summary>
    ///     Gets or sets a value indicating whether to compose and initialize the mapper.
    /// </summary>
    /// <remarks>Default is false unless WithApplication is true, in which case default is true.</remarks>
    public bool Mapper { get => _mapper.ValueOrDefault(WithApplication); set => _mapper.Set(value); }

    /// <summary>
    ///     Gets or sets a value indicating whether the LEGACY XML Cache used in tests should bind to repository events
    /// </summary>
    public bool PublishedRepositoryEvents
    {
        get => _publishedRepositoryEvents.ValueOrDefault(false);
        set => _publishedRepositoryEvents.Set(value);
    }

    /// <summary>
    ///     Gets or sets a value indicating the required logging support.
    /// </summary>
    /// <remarks>Default is to mock logging.</remarks>
    public UmbracoTestOptions.Logger Logger
    {
        get => _logger.ValueOrDefault(UmbracoTestOptions.Logger.Mock);
        set => _logger.Set(value);
    }

    /// <summary>
    ///     Gets or sets a value indicating the required database support.
    /// </summary>
    /// <remarks>Default is no database support.</remarks>
    public UmbracoTestOptions.Database Database
    {
        get => _database.ValueOrDefault(UmbracoTestOptions.Database.None);
        set => _database.Set(value);
    }

    /// <summary>
    ///     Gets or sets a value indicating the required plugin manager support.
    /// </summary>
    /// <remarks>Default is to use the global tests plugin manager.</remarks>
    public UmbracoTestOptions.TypeLoader TypeLoader
    {
        get => _typeLoader.ValueOrDefault(UmbracoTestOptions.TypeLoader.Default);
        set => _typeLoader.Set(value);
    }

    public bool Boot { get => _boot.ValueOrDefault(false); set => _boot.Set(value); }

    protected override TestOptionAttributeBase Merge(TestOptionAttributeBase other)
    {
        if (!(other is UmbracoTestAttribute attr))
        {
            throw new ArgumentException(nameof(other));
        }

        base.Merge(other);
        _boot.Set(attr.Boot);
        _mapper.Set(attr._mapper);
        _publishedRepositoryEvents.Set(attr._publishedRepositoryEvents);
        _logger.Set(attr._logger);
        _database.Set(attr._database);
        _typeLoader.Set(attr._typeLoader);

        return this;
    }
}
