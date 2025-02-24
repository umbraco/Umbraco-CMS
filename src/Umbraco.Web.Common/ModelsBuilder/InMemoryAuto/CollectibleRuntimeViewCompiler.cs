using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

internal class CollectibleRuntimeViewCompiler : IViewCompiler
{
    private readonly Lock _cacheLock = new();
    private readonly Dictionary<string, CompiledViewDescriptor> _precompiledViews;
    private readonly ConcurrentDictionary<string, string> _normalizedPathCache;
    private readonly IFileProvider _fileProvider;
    private readonly RazorProjectEngine _projectEngine;
    private IMemoryCache _cache;
    private readonly ILogger<CollectibleRuntimeViewCompiler> _logger;
    private readonly UmbracoRazorReferenceManager _referenceManager;
    private readonly CompilationOptionsProvider _compilationOptionsProvider;
    private readonly InMemoryAssemblyLoadContextManager _loadContextManager;

    public CollectibleRuntimeViewCompiler(
        IFileProvider fileProvider,
        RazorProjectEngine projectEngine,
        IList<CompiledViewDescriptor> precompiledViews,
        ILogger<CollectibleRuntimeViewCompiler> logger,
        UmbracoRazorReferenceManager referenceManager,
        CompilationOptionsProvider compilationOptionsProvider,
        InMemoryAssemblyLoadContextManager loadContextManager)
    {
        if (precompiledViews == null)
        {
            throw new ArgumentNullException(nameof(precompiledViews));
        }

        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _projectEngine = projectEngine ?? throw new ArgumentNullException(nameof(projectEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _referenceManager = referenceManager;
        _compilationOptionsProvider = compilationOptionsProvider;
        _loadContextManager = loadContextManager;

        _normalizedPathCache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        // This is our L0 cache, and is a durable store. Views migrate into the cache as they are requested
        // from either the set of known precompiled views, or by being compiled.
        _cache = new MemoryCache(new MemoryCacheOptions());

        // We need to validate that the all of the precompiled views are unique by path (case-insensitive).
        // We do this because there's no good way to canonicalize paths on windows, and it will create
        // problems when deploying to linux. Rather than deal with these issues, we just don't support
        // views that differ only by case.
        _precompiledViews = new Dictionary<string, CompiledViewDescriptor>(
            precompiledViews.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (CompiledViewDescriptor precompiledView in precompiledViews)
        {
            _logger.LogDebug("Initializing Razor view compiler with compiled view: '{ViewName}'", precompiledView.RelativePath);
            // View ordering has precedence semantics, a view with a higher precedence was
            // already added to the list.
            _precompiledViews.TryAdd(precompiledView.RelativePath, precompiledView);
        }

        if (_precompiledViews.Count == 0)
        {
            _logger.LogDebug("Initializing Razor view compiler with no compiled views");
        }
    }

    internal void ClearCache()
    {
        // I'm pretty sure this is not necessary, since it should be an atomic operation,
        // but let's make sure that we don't end up resolving any views while clearing the cache.
        lock (_cacheLock)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }
    }

    public Task<CompiledViewDescriptor> CompileAsync(string relativePath)
    {
        if (relativePath == null)
        {
            throw new ArgumentNullException(nameof(relativePath));
        }

        // Attempt to lookup the cache entry using the passed in path. This will succeed if the path is already
        // normalized and a cache entry exists.
        if (_cache.TryGetValue<Task<CompiledViewDescriptor>>(relativePath, out Task<CompiledViewDescriptor>? cachedResult) && cachedResult is not null)
        {
            return cachedResult;
        }

        var normalizedPath = GetNormalizedPath(relativePath);
        if (_cache.TryGetValue(normalizedPath, out cachedResult) && cachedResult is not null)
        {
            return cachedResult;
        }

        // Entry does not exist. Attempt to create one.
        cachedResult = OnCacheMiss(normalizedPath);
        return cachedResult;
    }

    private Task<CompiledViewDescriptor> OnCacheMiss(string normalizedPath)
    {
        ViewCompilerWorkItem item;
        TaskCompletionSource<CompiledViewDescriptor> taskSource;
        MemoryCacheEntryOptions cacheEntryOptions;

        // Safe races cannot be allowed when compiling Razor pages. To ensure only one compilation request succeeds
        // per file, we'll lock the creation of a cache entry. Creating the cache entry should be very quick. The
        // actual work for compiling files happens outside the critical section.
        lock (_cacheLock)
        {
            // Double-checked locking to handle a possible race.
            if (_cache.TryGetValue<Task<CompiledViewDescriptor>>(normalizedPath, out Task<CompiledViewDescriptor>? result) && result is not null)
            {
                return result;
            }

            if (_precompiledViews.TryGetValue(normalizedPath, out CompiledViewDescriptor? precompiledView))
            {
                _logger.LogTrace("Located compiled view for view at path '{Path}'", normalizedPath);
                item = CreatePrecompiledWorkItem(normalizedPath, precompiledView);
            }
            else
            {
                item = CreateRuntimeCompilationWorkItem(normalizedPath);
            }

            // At this point, we've decided what to do - but we should create the cache entry and
            // release the lock first.
            cacheEntryOptions = new MemoryCacheEntryOptions();

            Debug.Assert(item.ExpirationTokens != null);
            for (var i = 0; i < item.ExpirationTokens.Count; i++)
            {
                cacheEntryOptions.ExpirationTokens.Add(item.ExpirationTokens[i]);
            }

            taskSource = new TaskCompletionSource<CompiledViewDescriptor>(creationOptions: TaskCreationOptions.RunContinuationsAsynchronously);
            if (item.SupportsCompilation)
            {
                // We'll compile in just a sec, be patient.
            }
            else
            {
                // If we can't compile, we should have already created the descriptor
                Debug.Assert(item.Descriptor != null);
                taskSource.SetResult(item.Descriptor);
            }

            _cache.Set(normalizedPath, taskSource.Task, cacheEntryOptions);
        }

        // Now the lock has been released so we can do more expensive processing.
        if (item.SupportsCompilation)
        {
            Debug.Assert(taskSource != null);

            if (item.Descriptor?.Item != null &&
                ChecksumValidator.IsItemValid(_projectEngine.FileSystem, item.Descriptor.Item))
            {
                // If the item has checksums to validate, we should also have a precompiled view.
                Debug.Assert(item.Descriptor != null);

                taskSource.SetResult(item.Descriptor);
                return taskSource.Task;
            }

            _logger.LogTrace("Invalidating compiled view at path '{Path}' with a file since the checksum did not match", item.NormalizedPath);
            try
            {
                CompiledViewDescriptor descriptor = CompileAndEmit(normalizedPath);
                descriptor.ExpirationTokens = cacheEntryOptions.ExpirationTokens;
                taskSource.SetResult(descriptor);
            }
            catch (Exception ex)
            {
                taskSource.SetException(ex);
            }
        }

        return taskSource.Task;
    }

    private ViewCompilerWorkItem CreatePrecompiledWorkItem(string normalizedPath, CompiledViewDescriptor precompiledView)
    {
        // We have a precompiled view - but we're not sure that we can use it yet.
        //
        // We need to determine first if we have enough information to 'recompile' this view. If that's the case
        // we'll create change tokens for all of the files.
        //
        // Then we'll attempt to validate if any of those files have different content than the original sources
        // based on checksums.
        if (precompiledView.Item == null || !ChecksumValidator.IsRecompilationSupported(precompiledView.Item))
        {
            return new ViewCompilerWorkItem()
            {
                // If we don't have a checksum for the primary source file we can't recompile.
                SupportsCompilation = false,

                ExpirationTokens = Array.Empty<IChangeToken>(), // Never expire because we can't recompile.
                Descriptor = precompiledView, // This will be used as-is.
            };
        }

        var item = new ViewCompilerWorkItem()
        {
            SupportsCompilation = true,

            Descriptor = precompiledView, // This might be used, if the checksums match.

            // Used to validate and recompile
            NormalizedPath = normalizedPath,

            ExpirationTokens = GetExpirationTokens(precompiledView),
        };

        // We also need to create a new descriptor, because the original one doesn't have expiration tokens on
        // it. These will be used by the view location cache, which is like an L1 cache for views (this class is
        // the L2 cache).
        item.Descriptor = new CompiledViewDescriptor()
        {
            ExpirationTokens = item.ExpirationTokens,
            Item = precompiledView.Item,
            RelativePath = precompiledView.RelativePath,
        };

        return item;
    }

    private ViewCompilerWorkItem CreateRuntimeCompilationWorkItem(string normalizedPath)
    {
        IList<IChangeToken> expirationTokens = new List<IChangeToken>
            {
                _fileProvider.Watch(normalizedPath),
            };

        RazorProjectItem projectItem = _projectEngine.FileSystem.GetItem(normalizedPath, fileKind: null);
        if (!projectItem.Exists)
        {
            _logger.LogTrace("Could not find a file for view at path '{Path}'", normalizedPath);
            // If the file doesn't exist, we can't do compilation right now - we still want to cache
            // the fact that we tried. This will allow us to re-trigger compilation if the view file
            // is added.
            return new ViewCompilerWorkItem()
            {
                // We don't have enough information to compile
                SupportsCompilation = false,

                Descriptor = new CompiledViewDescriptor()
                {
                    RelativePath = normalizedPath,
                    ExpirationTokens = expirationTokens,
                },

                // We can try again if the file gets created.
                ExpirationTokens = expirationTokens,
            };
        }

        _logger.LogTrace("Found file at path '{Path}'", normalizedPath);

        GetChangeTokensFromImports(expirationTokens, projectItem);

        return new ViewCompilerWorkItem()
        {
            SupportsCompilation = true,

            NormalizedPath = normalizedPath,
            ExpirationTokens = expirationTokens,
        };
    }

    private IList<IChangeToken> GetExpirationTokens(CompiledViewDescriptor precompiledView)
    {
        IReadOnlyList<IRazorSourceChecksumMetadata> checksums = precompiledView.Item.GetChecksumMetadata();
        var expirationTokens = new List<IChangeToken>(checksums.Count);

        for (var i = 0; i < checksums.Count; i++)
        {
            // We rely on Razor to provide the right set of checksums. Trust the compiler, it has to do a good job,
            // so it probably will.
            expirationTokens.Add(_fileProvider.Watch(checksums[i].Identifier));
        }

        return expirationTokens;
    }

    private void GetChangeTokensFromImports(IList<IChangeToken> expirationTokens, RazorProjectItem projectItem)
    {
        // OK this means we can do compilation. For now let's just identify the other files we need to watch
        // so we can create the cache entry. Compilation will happen after we release the lock.
        IImportProjectFeature[] importFeature = _projectEngine.ProjectFeatures.OfType<IImportProjectFeature>().ToArray();
        foreach (IImportProjectFeature feature in importFeature)
        {
            foreach (RazorProjectItem? file in feature.GetImports(projectItem))
            {
                if (file.FilePath != null)
                {
                    expirationTokens.Add(_fileProvider.Watch(file.FilePath));
                }
            }
        }
    }

    protected virtual CompiledViewDescriptor CompileAndEmit(string relativePath)
    {
        RazorProjectItem projectItem = _projectEngine.FileSystem.GetItem(relativePath, fileKind: null);
        RazorCodeDocument codeDocument = _projectEngine.Process(projectItem);
        RazorCSharpDocument cSharpDocument = codeDocument.GetCSharpDocument();

        if (cSharpDocument.Diagnostics.Count > 0)
        {
            throw CompilationExceptionFactory.Create(
                codeDocument,
                cSharpDocument.Diagnostics);
        }

        Assembly assembly = CompileAndEmit(codeDocument, cSharpDocument.GeneratedCode);

        // Anything we compile from source will use Razor 2.1 and so should have the new metadata.
        var loader = new RazorCompiledItemLoader();
        RazorCompiledItem item = loader.LoadItems(assembly).Single();
        return new CompiledViewDescriptor(item);
    }

    internal Assembly CompileAndEmit(RazorCodeDocument codeDocument, string generatedCode)
    {
        var startTimestamp = _logger.IsEnabled(LogLevel.Debug) ? Stopwatch.GetTimestamp() : 0;

        var assemblyName = Path.GetRandomFileName();
        CSharpCompilation compilation = CreateCompilation(generatedCode, assemblyName);

        EmitOptions emitOptions = _compilationOptionsProvider.EmitOptions;
        var emitPdbFile = _compilationOptionsProvider.EmitPdb && emitOptions.DebugInformationFormat != DebugInformationFormat.Embedded;


        using (var assemblyStream = new MemoryStream())
        using (MemoryStream? pdbStream = emitPdbFile ? new MemoryStream() : null)
        {
            EmitResult result = compilation.Emit(
                assemblyStream,
                pdbStream,
                options: _compilationOptionsProvider.EmitOptions);

            if (result.Success is false)
            {
                UmbracoCompilationException compilationException = CompilationExceptionFactory.Create(
                    codeDocument,
                    generatedCode,
                    assemblyName,
                    result.Diagnostics);

                LogCompilationFailure(compilationException);

                throw compilationException;
            }

            assemblyStream.Seek(0, SeekOrigin.Begin);
            pdbStream?.Seek(0, SeekOrigin.Begin);

            Assembly assembly = _loadContextManager.LoadCollectibleAssemblyFromStream(assemblyStream, pdbStream);

            return assembly;
        }
    }

    private void LogCompilationFailure(UmbracoCompilationException compilationException)
    {
        IEnumerable<string>? messages = compilationException.CompilationFailures?
            .WhereNotNull()
            .SelectMany(x => x.Messages!)
            .WhereNotNull()
            .Select(x => x.FormattedMessage)
            .WhereNotNull();

        foreach (var message in messages ?? Enumerable.Empty<string>())
        {
            _logger.LogError(compilationException, "Compilation error occured with message: {ErrorMessage}", message);
        }
    }

    private CSharpCompilation CreateCompilation(string compilationContent, string assemblyName)
    {
        IReadOnlyList<MetadataReference> refs = _referenceManager.CompilationReferences;

        var sourceText = SourceText.From(compilationContent, Encoding.UTF8);
        SyntaxTree syntaxTree = SyntaxFactory
            .ParseSyntaxTree(sourceText, _compilationOptionsProvider.ParseOptions)
            .WithFilePath(assemblyName);

        CSharpCompilation compilation = CSharpCompilation
                .Create(assemblyName)
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(refs)
                .WithOptions(_compilationOptionsProvider.CSharpCompilationOptions);

        // We'll add the reference to the InMemory assembly directly, this means we don't have to hack around with assembly parts.
        // We might be asked to compile views before the InMemory models assembly is created tho (if you replace the no-nodes for instance)
        // In this case we'll just skip the InMemory models assembly reference
        if (_loadContextManager.ModelsAssemblyLocation is null)
        {
            _logger.LogInformation("No InMemory models assembly available, skipping reference");
            return compilation;
        }

        PortableExecutableReference inMemoryAutoReference = MetadataReference.CreateFromFile(_loadContextManager.ModelsAssemblyLocation);
        compilation = compilation.AddReferences(inMemoryAutoReference);
        return compilation;
    }

    private string GetNormalizedPath(string relativePath)
    {
        Debug.Assert(relativePath != null);
        if (relativePath.Length == 0)
        {
            return relativePath;
        }

        if (!_normalizedPathCache.TryGetValue(relativePath, out var normalizedPath))
        {
            normalizedPath = NormalizePath(relativePath);
            _normalizedPathCache[relativePath] = normalizedPath;
        }

        return normalizedPath;
    }

    // Taken from: https://github.com/dotnet/aspnetcore/blob/a450cb69b5e4549f5515cdb057a68771f56cefd7/src/Mvc/Mvc.Razor/src/ViewPath.cs
    // This normalizes the relative path to the view, ensuring that it matches with what we have as keys in _precompiledViews
    private string NormalizePath(string path)
    {
        var addLeadingSlash = path[0] != '\\' && path[0] != '/';
        var transformSlashes = path.IndexOf('\\') != -1;

        if (!addLeadingSlash && !transformSlashes)
        {
            return path;
        }

        var length = path.Length;
        if (addLeadingSlash)
        {
            length++;
        }

        return string.Create(length, (path, addLeadingSlash), (span, tuple) =>
        {
            (string pathValue, bool addLeadingSlashValue) = tuple;
            var spanIndex = 0;

            if (addLeadingSlashValue)
            {
                span[spanIndex++] = '/';
            }

            foreach (var ch in pathValue)
            {
                span[spanIndex++] = ch == '\\' ? '/' : ch;
            }
        });
    }

    private sealed class ViewCompilerWorkItem
    {
        public bool SupportsCompilation { get; set; } = default!;

        public string NormalizedPath { get; set; } = default!;

        public IList<IChangeToken> ExpirationTokens { get; set; } = default!;

        public CompiledViewDescriptor Descriptor { get; set; } = default!;
    }
}
