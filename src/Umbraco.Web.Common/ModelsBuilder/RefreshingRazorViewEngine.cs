using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;

/*
 * OVERVIEW:
 *
 * The CSharpCompiler is responsible for the actual compilation of razor at runtime.
 * It creates a CSharpCompilation instance to do the compilation. This is where DLL references
 * are applied. However, the way this works is not flexible for dynamic assemblies since the references
 * are only discovered and loaded once before the first compilation occurs. This is done here:
 * https://github.com/dotnet/aspnetcore/blob/114f0f6d1ef1d777fb93d90c87ac506027c55ea0/src/Mvc/Mvc.Razor.RuntimeCompilation/src/CSharpCompiler.cs#L79
 * The CSharpCompiler is internal and cannot be replaced or extended, however it's references come from:
 * RazorReferenceManager. Unfortunately this is also internal and cannot be replaced, though it can be extended
 * using MvcRazorRuntimeCompilationOptions, except this is the place where references are only loaded once which
 * is done with a LazyInitializer. See https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs#L35.
 *
 * The way that RazorReferenceManager works is by resolving references from the ApplicationPartsManager - either by
 * an application part that is specifically an ICompilationReferencesProvider or an AssemblyPart. So to fulfill this
 * requirement, we add the MB assembly to the assembly parts manager within the InMemoryModelFactory when the assembly
 * is (re)generated. But due to the above restrictions, when re-generating, this will have no effect since the references
 * have already been resolved with the LazyInitializer in the RazorReferenceManager.
 *
 * The services that can be replaced are: IViewCompilerProvider (default is the internal RuntimeViewCompilerProvider) and
 * IViewCompiler (default is the internal RuntimeViewCompiler). There is one specific public extension point that I was
 * hoping would solve all of the problems which was IMetadataReferenceFeature (implemented by LazyMetadataReferenceFeature
 * which uses RazorReferencesManager) which is a razor feature that you can add
 * to the RazorProjectEngine. It is used to resolve roslyn references and by default is backed by RazorReferencesManager.
 * Unfortunately, this service is not used by the CSharpCompiler, it seems to only be used by some tag helper compilations.
 *
 * There are caches at several levels, all of which are not publicly accessible APIs (apart from RazorViewEngine.ViewLookupCache
 * which is possible to clear by casting and then calling cache.Compact(100); but that doesn't get us far enough).
 *
 * For this to work, several caches must be cleared:
 * - RazorViewEngine.ViewLookupCache
 * - RazorReferencesManager._compilationReferences
 * - RazorPageActivator._activationInfo (though this one may be optional)
 * - RuntimeViewCompiler._cache
 *
 * What are our options?
 *
 * a) We can copy a ton of code into our application: CSharpCompiler, RuntimeViewCompilerProvider, RuntimeViewCompiler and
 *    RazorReferenceManager (probably more depending on the extent of Internal references).
 * b) We can use reflection to try to access all of the above resources and try to forcefully clear caches and reset initialization flags.
 * c) We hack these replace-able services with our own implementations that wrap the default services. To do this
 *    requires re-resolving the original services from a pre-built DI container. In effect this re-creates these
 *    services from scratch which means there is no caches.
 *
 * ... Option C works, we will use that but need to verify how this affects memory since ideally the old services will be GC'd.
 *
 * Option C, how its done:
 * - Before we add our custom razor services to the container, we make a copy of the services collection which is the snapshot of registered services
 *   with razor defaults before ours are added.
 * - We replace the default implementation of IRazorViewEngine with our own. This is a wrapping service that wraps the default RazorViewEngine instance.
 *   The ctor for this service takes in a Factory method to re-construct the default RazorViewEngine and all of it's dependency graph.
 * - When the models change, the Factory is invoked and the default razor services are all re-created, thus clearing their caches and the newly
 *   created instance is wrapped. The RazorViewEngine is the only service that needs to be replaced and wrapped for this to work because it's dependency
 *   graph includes all of the above mentioned services, all the way up to the RazorProjectEngine and it's LazyMetadataReferenceFeature.
 */

namespace Umbraco.Cms.Web.Common.ModelsBuilder;

/// <summary>
///     Custom <see cref="IRazorViewEngine" /> that wraps aspnetcore's default implementation
/// </summary>
/// <remarks>
///     This is used so that when new models are built, the entire razor stack is re-constructed so all razor
///     caches and assembly references, etc... are cleared.
/// </remarks>
internal class RefreshingRazorViewEngine : IRazorViewEngine, IDisposable
{
    private readonly Func<IRazorViewEngine> _defaultRazorViewEngineFactory;
    private readonly InMemoryModelFactory _inMemoryModelFactory;
    private readonly ReaderWriterLockSlim _locker = new();
    private IRazorViewEngine _current;
    private bool _disposedValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RefreshingRazorViewEngine" /> class.
    /// </summary>
    /// <param name="defaultRazorViewEngineFactory">
    ///     A factory method used to re-construct the default aspnetcore <see cref="RazorViewEngine" />
    /// </param>
    /// <param name="inMemoryModelFactory">The <see cref="InMemoryModelFactory" /></param>
    public RefreshingRazorViewEngine(
        Func<IRazorViewEngine> defaultRazorViewEngineFactory,
        InMemoryModelFactory inMemoryModelFactory)
    {
        _inMemoryModelFactory = inMemoryModelFactory;
        _defaultRazorViewEngineFactory = defaultRazorViewEngineFactory;
        _current = _defaultRazorViewEngineFactory();
        _inMemoryModelFactory.ModelsChanged += InMemoryModelFactoryModelsChanged;
    }

    public void Dispose() =>

        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);

    public RazorPageResult FindPage(ActionContext context, string pageName)
    {
        _locker.EnterReadLock();
        try
        {
            return _current.FindPage(context, pageName);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public string? GetAbsolutePath(string? executingFilePath, string? pagePath)
    {
        _locker.EnterReadLock();
        try
        {
            return _current.GetAbsolutePath(executingFilePath, pagePath);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public RazorPageResult GetPage(string executingFilePath, string pagePath)
    {
        _locker.EnterReadLock();
        try
        {
            return _current.GetPage(executingFilePath, pagePath);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
    {
        _locker.EnterReadLock();
        try
        {
            return _current.FindView(context, viewName, isMainPage);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public ViewEngineResult GetView(string? executingFilePath, string viewPath, bool isMainPage)
    {
        _locker.EnterReadLock();
        try
        {
            return _current.GetView(executingFilePath, viewPath, isMainPage);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _inMemoryModelFactory.ModelsChanged -= InMemoryModelFactoryModelsChanged;
                _locker.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    ///     When the models change, re-construct the razor stack
    /// </summary>
    private void InMemoryModelFactoryModelsChanged(object? sender, EventArgs e)
    {
        _locker.EnterWriteLock();
        try
        {
            _current = _defaultRazorViewEngineFactory();
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }
}
