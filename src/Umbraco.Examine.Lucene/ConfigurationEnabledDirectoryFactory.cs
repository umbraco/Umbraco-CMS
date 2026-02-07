// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// An Examine directory factory implementation based on configured values.
/// </summary>
public class ConfigurationEnabledDirectoryFactory : IDirectoryFactory
{
    private readonly IApplicationRoot _applicationRoot;
    private readonly IServiceProvider _services;
    private readonly IndexCreatorSettings _settings;
    private IDirectoryFactory? _directoryFactory;

    public ConfigurationEnabledDirectoryFactory(
        IServiceProvider services,
        IOptions<IndexCreatorSettings> settings,
        IApplicationRoot applicationRoot)
    {
        _services = services;
        _applicationRoot = applicationRoot;
        _settings = settings.Value;
    }

    public Directory CreateTaxonomyDirectory(LuceneIndex luceneIndex, bool forceUnlock)
    {
        _directoryFactory ??= CreateFactory();
        return _directoryFactory.CreateTaxonomyDirectory(luceneIndex, forceUnlock);
    }

    public Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
    {
        _directoryFactory ??= CreateFactory();
        return _directoryFactory.CreateDirectory(luceneIndex, forceUnlock);
    }

    /// <summary>
    ///     Creates a directory factory based on the configured value and ensures that
    /// </summary>
    private IDirectoryFactory CreateFactory()
    {
        DirectoryInfo dirInfo = _applicationRoot.ApplicationRoot;

        if (!dirInfo.Exists)
        {
            System.IO.Directory.CreateDirectory(dirInfo.FullName);
        }

        return _settings.LuceneDirectoryFactory switch
        {
            LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory => _services.GetRequiredService<SyncedFileSystemDirectoryFactory>(),
            LuceneDirectoryFactory.TempFileSystemDirectoryFactory => _services.GetRequiredService<UmbracoTempEnvFileSystemDirectoryFactory>(),
            _ => _services.GetRequiredService<FileSystemDirectoryFactory>(),
        };
    }
}
