using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Exceptions;

namespace Umbraco.Cms.Web.Common.ModelsBuilder;

internal class UmbracoViewCompilerProvider : IViewCompilerProvider
{
    private readonly RazorProjectEngine _razorProjectEngine;
    private readonly InMemoryModelFactory _inMemoryModelFactory;
    private readonly UmbracoRazorReferenceManager _umbracoRazorReferenceManager;
    private readonly CompilationOptionsProvider _compilationOptionsProvider;

    private readonly ApplicationPartManager _applicationPartManager;

    // 0 private readonly RuntimeCompilationFileProvider _fileProvider;
    private readonly ILogger<CollectibleRuntimeViewCompiler> _logger;
    private readonly Func<IViewCompiler> _createCompiler;

    private object _initializeLock = new object();
    private bool _initialized;
    private IViewCompiler? _compiler;
    private readonly MvcRazorRuntimeCompilationOptions _options;


    public UmbracoViewCompilerProvider(
        ApplicationPartManager applicationPartManager,
        RazorProjectEngine razorProjectEngine,
        // RuntimeCompilationFileProvider fileProvider,
        ILoggerFactory loggerFactory,
        IOptions<MvcRazorRuntimeCompilationOptions> options,
        InMemoryModelFactory inMemoryModelFactory,
        UmbracoRazorReferenceManager umbracoRazorReferenceManager,
        CompilationOptionsProvider compilationOptionsProvider)
    {
        _applicationPartManager = applicationPartManager;
        _razorProjectEngine = razorProjectEngine;
        _inMemoryModelFactory = inMemoryModelFactory;
        _umbracoRazorReferenceManager = umbracoRazorReferenceManager;
        _compilationOptionsProvider = compilationOptionsProvider;
        _options = options.Value;
        // _fileProvider = fileProvider;

        _logger = loggerFactory.CreateLogger<CollectibleRuntimeViewCompiler>();
        _createCompiler = CreateCompiler;
    }

    public IViewCompiler GetCompiler()
    {
        return LazyInitializer.EnsureInitialized(
            ref _compiler,
            ref _initialized,
            ref _initializeLock,
            _createCompiler)!;
    }

    private IViewCompiler CreateCompiler()
    {
        var feature = new ViewsFeature();
        _applicationPartManager.PopulateFeature(feature);

        return new CollectibleRuntimeViewCompiler(
            GetCompositeFileProvider(_options),
            _razorProjectEngine,
            feature.ViewDescriptors,
            _logger,
            _inMemoryModelFactory,
            _umbracoRazorReferenceManager,
            _compilationOptionsProvider);
    }

    private static IFileProvider GetCompositeFileProvider(MvcRazorRuntimeCompilationOptions options)
    {
        var fileProviders = options.FileProviders;
        if (fileProviders.Count == 0)
        {
            throw new PanicException();
        }
        else if (fileProviders.Count == 1)
        {
            return fileProviders[0];
        }

        return new CompositeFileProvider(fileProviders);
    }
}
