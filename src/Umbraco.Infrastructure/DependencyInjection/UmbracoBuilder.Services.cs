using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Packaging;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;
using Umbraco.Cms.Infrastructure.Templates;
using Umbraco.Extensions;
using CacheInstructionService = Umbraco.Cms.Core.Services.Implement.CacheInstructionService;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco services
    /// </summary>
    internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        // register the service context
        builder.Services.AddSingleton<ServiceContext>();

        // register the special idk map
        builder.Services.AddUnique<IIdKeyMap, IdKeyMap>();

        builder.Services.AddUnique<IAuditService, AuditService>();
        builder.Services.AddUnique<ICacheInstructionService, CacheInstructionService>();
        builder.Services.AddUnique<IBasicAuthService, BasicAuthService>();
        builder.Services.AddUnique<IDataTypeService, DataTypeService>();
        builder.Services.AddUnique<IPackagingService, PackagingService>();
        builder.Services.AddUnique<IServerRegistrationService, ServerRegistrationService>();
        builder.Services.AddUnique<ITwoFactorLoginService, TwoFactorLoginService>();
        builder.Services.AddTransient(CreateLocalizedTextServiceFileSourcesFactory);
        builder.Services.AddUnique(factory => CreatePackageRepository(factory, "createdPackages.config"));
        builder.Services.AddUnique<ICreatedPackagesRepository, CreatedPackageSchemaRepository>();
        builder.Services.AddSingleton(CreatePackageDataInstallation);
        builder.Services.AddUnique<IPackageInstallation, PackageInstallation>();
        builder.Services.AddUnique<IHtmlMacroParameterParser, HtmlMacroParameterParser>();
        builder.Services.AddTransient<IExamineIndexCountService, ExamineIndexCountService>();
        builder.Services.AddUnique<IUserDataService, SystemInformationTelemetryProvider>();
        builder.Services.AddTransient<IUsageInformationService, UsageInformationService>();
        builder.Services.AddSingleton<IEditorConfigurationParser, EditorConfigurationParser>();

        return builder;
    }

    private static PackagesRepository CreatePackageRepository(IServiceProvider factory, string packageRepoFileName)
        => new(
            factory.GetRequiredService<IContentService>(),
            factory.GetRequiredService<IContentTypeService>(),
            factory.GetRequiredService<IDataTypeService>(),
            factory.GetRequiredService<IFileService>(),
            factory.GetRequiredService<IMacroService>(),
            factory.GetRequiredService<ILocalizationService>(),
            factory.GetRequiredService<IHostingEnvironment>(),
            factory.GetRequiredService<IEntityXmlSerializer>(),
            factory.GetRequiredService<IOptions<GlobalSettings>>(),
            factory.GetRequiredService<IMediaService>(),
            factory.GetRequiredService<IMediaTypeService>(),
            factory.GetRequiredService<MediaFileManager>(),
            factory.GetRequiredService<FileSystems>(),
            packageRepoFileName);

    // Factory registration is only required because of ambiguous constructor
    private static PackageDataInstallation CreatePackageDataInstallation(IServiceProvider factory)
        => new(
            factory.GetRequiredService<IDataValueEditorFactory>(),
            factory.GetRequiredService<ILogger<PackageDataInstallation>>(),
            factory.GetRequiredService<IFileService>(),
            factory.GetRequiredService<IMacroService>(),
            factory.GetRequiredService<ILocalizationService>(),
            factory.GetRequiredService<IDataTypeService>(),
            factory.GetRequiredService<IEntityService>(),
            factory.GetRequiredService<IContentTypeService>(),
            factory.GetRequiredService<IContentService>(),
            factory.GetRequiredService<PropertyEditorCollection>(),
            factory.GetRequiredService<IScopeProvider>(),
            factory.GetRequiredService<IShortStringHelper>(),
            factory.GetRequiredService<IConfigurationEditorJsonSerializer>(),
            factory.GetRequiredService<IMediaService>(),
            factory.GetRequiredService<IMediaTypeService>());

    private static LocalizedTextServiceFileSources CreateLocalizedTextServiceFileSourcesFactory(
        IServiceProvider container)
    {
        IHostingEnvironment hostingEnvironment = container.GetRequiredService<IHostingEnvironment>();
        var subPath = WebPath.Combine(Constants.SystemDirectories.Umbraco, "config", "lang");
        var mainLangFolder = new DirectoryInfo(hostingEnvironment.MapPathContentRoot(subPath));

        return new LocalizedTextServiceFileSources(
            container.GetRequiredService<ILogger<LocalizedTextServiceFileSources>>(),
            container.GetRequiredService<AppCaches>(),
            mainLangFolder,
            container.GetServices<LocalizedTextServiceSupplementaryFileSource>(),
            new EmbeddedFileProvider(typeof(IAssemblyProvider).Assembly, "Umbraco.Cms.Core.EmbeddedResources.Lang")
                .GetDirectoryContents(string.Empty));
    }
}
