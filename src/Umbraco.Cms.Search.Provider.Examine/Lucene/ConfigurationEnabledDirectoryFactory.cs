// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Search.Provider.Examine.Lucene;

/// <summary>
///     An Examine directory factory implementation based on configured values
/// </summary>
public class ConfigurationEnabledDirectoryFactory : DirectoryFactoryBase
{
    private readonly IApplicationRoot _applicationRoot;
    private readonly IServiceProvider _services;
    private readonly ExamineSearchProviderSettings _settings;
    private IDirectoryFactory? _directoryFactory;

    public ConfigurationEnabledDirectoryFactory(
        IServiceProvider services,
        IOptions<ExamineSearchProviderSettings> settings,
        IApplicationRoot applicationRoot)
    {
        _services = services;
        _applicationRoot = applicationRoot;
        _settings = settings.Value;
    }

    protected override Directory CreateDirectory(LuceneIndex luceneIndex, bool forceUnlock)
    {
        _directoryFactory = CreateFactory();
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

        switch (_settings.LuceneDirectoryFactory)
        {
            case LuceneDirectoryFactory.SyncedTempFileSystemDirectoryFactory:
                return _services.GetRequiredService<SyncedFileSystemDirectoryFactory>();
            case LuceneDirectoryFactory.TempFileSystemDirectoryFactory:
                return _services.GetRequiredService<UmbracoTempEnvFileSystemDirectoryFactory>();
            case LuceneDirectoryFactory.Default:
            default:
                return _services.GetRequiredService<FileSystemDirectoryFactory>();
        }
    }
}
