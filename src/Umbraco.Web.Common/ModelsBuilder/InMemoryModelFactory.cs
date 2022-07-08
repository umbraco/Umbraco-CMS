using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Web.Common.ModelsBuilder
{
    internal class InMemoryModelFactory : IAutoPublishedModelFactory, IRegisteredObject, IDisposable
    {
        private static readonly Regex s_usingRegex = new Regex("^using(.*);", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex s_aattrRegex = new Regex("^\\[assembly:(.*)\\]", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex s_assemblyVersionRegex = new Regex("AssemblyVersion\\(\"[0-9]+.[0-9]+.[0-9]+.[0-9]+\"\\)", RegexOptions.Compiled);
        private static readonly string[] s_ourFiles = { "models.hash", "models.generated.cs", "all.generated.cs", "all.dll.path", "models.err", "Compiled" };
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<InMemoryModelFactory> _logger;
        private readonly FileSystemWatcher? _watcher;
        private readonly Lazy<UmbracoServices> _umbracoServices; // fixme: this is because of circular refs :(
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IApplicationShutdownRegistry _hostingLifetime;
        private readonly ModelsGenerationError _errors;
        private readonly IPublishedValueFallback _publishedValueFallback;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly Lazy<string> _pureLiveDirectory = null!;
        private readonly int _debugLevel;
        private Infos _infos = new Infos { ModelInfos = null, ModelTypeMap = new Dictionary<string, Type>() };
        private volatile bool _hasModels; // volatile 'cos reading outside lock
        private bool _pendingRebuild;
        private int _ver;
        private int? _skipver;
        private RoslynCompiler? _roslynCompiler;
        private UmbracoAssemblyLoadContext? _currentAssemblyLoadContext;
        private ModelsBuilderSettings _config;
        private bool _disposedValue;

        public InMemoryModelFactory(
            Lazy<UmbracoServices> umbracoServices,
            IProfilingLogger profilingLogger,
            ILogger<InMemoryModelFactory> logger,
            IOptionsMonitor<ModelsBuilderSettings> config,
            IHostingEnvironment hostingEnvironment,
            IApplicationShutdownRegistry hostingLifetime,
            IPublishedValueFallback publishedValueFallback,
            ApplicationPartManager applicationPartManager)
        {
            _umbracoServices = umbracoServices;
            _profilingLogger = profilingLogger;
            _logger = logger;
            _config = config.CurrentValue;
            _hostingEnvironment = hostingEnvironment;
            _hostingLifetime = hostingLifetime;
            _publishedValueFallback = publishedValueFallback;
            _applicationPartManager = applicationPartManager;
            _errors = new ModelsGenerationError(config, _hostingEnvironment);
            _ver = 1; // zero is for when we had no version
            _skipver = -1; // nothing to skip

            if (!hostingEnvironment.IsHosted)
            {
                return;
            }

            config.OnChange(x => _config = x);
            _pureLiveDirectory = new Lazy<string>(PureLiveDirectoryAbsolute);

            if (!Directory.Exists(_pureLiveDirectory.Value))
            {
                Directory.CreateDirectory(_pureLiveDirectory.Value);
            }

            // BEWARE! if the watcher is not properly released then for some reason the
            // BuildManager will start confusing types - using a 'registered object' here
            // though we should probably plug into Umbraco's MainDom - which is internal
            _hostingLifetime.RegisterObject(this);
            _watcher = new FileSystemWatcher(_pureLiveDirectory.Value);
            _watcher.Changed += WatcherOnChanged;
            _watcher.EnableRaisingEvents = true;

            // get it here, this need to be fast
            _debugLevel = _config.DebugLevel;

            AssemblyLoadContext.Default.Resolving += OnResolvingDefaultAssemblyLoadContext;
        }

        public event EventHandler? ModelsChanged;

        /// <summary>
        /// Gets the currently loaded Live models assembly
        /// </summary>
        /// <remarks>
        /// Can be null
        /// </remarks>
        public Assembly? CurrentModelsAssembly { get; private set; }

        /// <inheritdoc />
        public object SyncRoot { get; } = new object();

        private UmbracoServices UmbracoServices => _umbracoServices.Value;

        /// <summary>
        /// Gets the RoslynCompiler
        /// </summary>
        private RoslynCompiler RoslynCompiler
        {
            get
            {
                if (_roslynCompiler != null)
                {
                    return _roslynCompiler;
                }

                _roslynCompiler = new RoslynCompiler();
                return _roslynCompiler;
            }
        }

        /// <inheritdoc />
        public bool Enabled => _config.ModelsMode == ModelsMode.InMemoryAuto;

        /// <summary>
        /// Handle the event when a reference cannot be resolved from the default context and return our custom MB assembly reference if we have one
        /// </summary>
        /// <remarks>
        /// This is required because the razor engine will only try to load things from the default context, it doesn't know anything
        /// about our context so we need to proxy.
        /// </remarks>
        private Assembly? OnResolvingDefaultAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
            => assemblyName.Name == RoslynCompiler.GeneratedAssemblyName
                ? _currentAssemblyLoadContext?.LoadFromAssemblyName(assemblyName)
                : null;

        public IPublishedElement CreateModel(IPublishedElement element)
        {
            // get models, rebuilding them if needed
            Dictionary<string, ModelInfo>? infos = EnsureModels().ModelInfos;
            if (infos == null)
            {
                return element;
            }

            // be case-insensitive
            var contentTypeAlias = element.ContentType.Alias;

            // lookup model constructor (else null)
            infos.TryGetValue(contentTypeAlias, out var info);

            // create model
            return info is null || info.Ctor is null ? element : info.Ctor(element, _publishedValueFallback);
        }

        /// <inheritdoc />
        public Type GetModelType(string? alias)
        {
            Infos infos = EnsureModels();

            // fail fast
            if (alias is null ||
                infos.ModelInfos is null ||
                !infos.ModelInfos.TryGetValue(alias, out ModelInfo? modelInfo) ||
                modelInfo.ModelType is null)
            {
                return typeof(IPublishedElement);
            }

            return modelInfo.ModelType;
        }

        // this runs only once the factory is ready
        // NOT when building models
        public Type MapModelType(Type type)
        {
            Infos infos = EnsureModels();
            return ModelType.Map(type, infos.ModelTypeMap);
        }

        // this runs only once the factory is ready
        // NOT when building models
        public IList CreateModelList(string? alias)
        {
            Infos infos = EnsureModels();

            // fail fast
            if (alias is null || infos.ModelInfos is null || !infos.ModelInfos.TryGetValue(alias, out ModelInfo? modelInfo))
            {
                return new List<IPublishedElement>();
            }

            Func<IList>? ctor = modelInfo.ListCtor;
            if (ctor != null)
            {
                return ctor();
            }

            if (modelInfo.ModelType is null)
            {
                return new List<IPublishedElement>();
            }

            Type listType = typeof(List<>).MakeGenericType(modelInfo.ModelType);
            ctor = modelInfo.ListCtor = ReflectionUtilities.EmitConstructor<Func<IList>>(declaring: listType);

            return ctor is null ? new List<IPublishedElement>() : ctor();
        }

        /// <inheritdoc />
        public void Reset()
        {
            if (Enabled)
            {
                ResetModels();
            }
        }

        // tells the factory that it should build a new generation of models
        private void ResetModels()
        {
            _logger.LogDebug("Resetting models.");

            try
            {
                _locker.EnterWriteLock();

                _hasModels = false;
                _pendingRebuild = true;

                if (!Directory.Exists(_pureLiveDirectory.Value))
                {
                    Directory.CreateDirectory(_pureLiveDirectory.Value);
                }

                // clear stuff
                var modelsHashFile = Path.Combine(_pureLiveDirectory.Value, "models.hash");
                var dllPathFile = Path.Combine(_pureLiveDirectory.Value, "all.dll.path");

                if (File.Exists(dllPathFile))
                {
                    File.Delete(dllPathFile);
                }

                if (File.Exists(modelsHashFile))
                {
                    File.Delete(modelsHashFile);
                }
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        // ensure that the factory is running with the lastest generation of models
        internal Infos EnsureModels()
        {
            if (_debugLevel > 0)
            {
                _logger.LogDebug("Ensuring models.");
            }

            // don't use an upgradeable lock here because only 1 thread at a time could enter it
            try
            {
                _locker.EnterReadLock();
                if (_hasModels)
                {
                    return _infos;
                }
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                {
                    _locker.ExitReadLock();
                }
            }

            try
            {
                _locker.EnterUpgradeableReadLock();

                if (_hasModels)
                {
                    return _infos;
                }

                _locker.EnterWriteLock();

                // we don't have models,
                // either they haven't been loaded from the cache yet
                // or they have been reseted and are pending a rebuild
                using (_profilingLogger.DebugDuration<InMemoryModelFactory>("Get models.", "Got models."))
                {
                    try
                    {
                        Assembly assembly = GetModelsAssembly(_pendingRebuild);

                        CurrentModelsAssembly = assembly;

                        // Raise the model changing event.
                        // NOTE: That on first load, if there is content, this will execute before the razor view engine
                        // has loaded which means it hasn't yet bound to this event so there's no need to worry about if
                        // it will be eagerly re-generated unecessarily on first render. BUT we should be aware that if we
                        // change this to use the event aggregator that will no longer be the case.
                        ModelsChanged?.Invoke(this, new EventArgs());

                        IEnumerable<Type> types = assembly.ExportedTypes.Where(x => x.Inherits<PublishedContentModel>() || x.Inherits<PublishedElementModel>());
                        _infos = RegisterModels(types);
                        _errors.Clear();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            _logger.LogError(e, "Failed to build models.");
                            _logger.LogWarning("Running without models."); // be explicit
                            _errors.Report("Failed to build InMemory models.", e);
                        }
                        finally
                        {
                            CurrentModelsAssembly = null;
                            _infos = new Infos { ModelInfos = null, ModelTypeMap = new Dictionary<string, Type>() };
                        }
                    }

                    // don't even try again
                    _hasModels = true;
                }

                return _infos;
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                {
                    _locker.ExitWriteLock();
                }

                if (_locker.IsUpgradeableReadLockHeld)
                {
                    _locker.ExitUpgradeableReadLock();
                }
            }
        }

        public string PureLiveDirectoryAbsolute() => _hostingEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.TempData + "/InMemoryAuto");

        // This is NOT thread safe but it is only called from within a lock
        private Assembly ReloadAssembly(string pathToAssembly)
        {
            // If there's a current AssemblyLoadContext, unload it before creating a new one.
            if (!(_currentAssemblyLoadContext is null))
            {
                _currentAssemblyLoadContext.Unload();

                // we need to remove the current part too
                ApplicationPart? currentPart = _applicationPartManager.ApplicationParts.FirstOrDefault(x => x.Name == RoslynCompiler.GeneratedAssemblyName);
                if (currentPart != null)
                {
                    _applicationPartManager.ApplicationParts.Remove(currentPart);
                }
            }

            // We must create a new assembly load context
            // as long as theres a reference to the assembly load context we can't delete the assembly it loaded
            _currentAssemblyLoadContext = new UmbracoAssemblyLoadContext();

            // NOTE: We cannot use in-memory assemblies due to the way the razor engine works which must use
            // application parts in order to add references to it's own CSharpCompiler.
            // These parts must have real paths since that is how the references are loaded. In that
            // case we'll need to work on temp files so that the assembly isn't locked.

            // Get a temp file path
            // NOTE: We cannot use Path.GetTempFileName(), see this issue:
            // https://github.com/dotnet/AspNetCore.Docs/issues/3589 which can cause issues, this is recommended instead
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.Copy(pathToAssembly, tempFile, true);

            // Load it in
            Assembly assembly = _currentAssemblyLoadContext.LoadFromAssemblyPath(tempFile);

            // Add the assembly to the application parts - this is required because this is how
            // the razor ReferenceManager resolves what to load, see
            // https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Razor.RuntimeCompilation/src/RazorReferenceManager.cs#L53
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            foreach (ApplicationPart applicationPart in partFactory.GetApplicationParts(assembly))
            {
                _applicationPartManager.ApplicationParts.Add(applicationPart);
            }

            return assembly;
        }

        // This is NOT thread safe but it is only called from within a lock
        private Assembly GetModelsAssembly(bool forceRebuild)
        {
            if (!Directory.Exists(_pureLiveDirectory.Value))
            {
                Directory.CreateDirectory(_pureLiveDirectory.Value);
            }

            IList<TypeModel> typeModels = UmbracoServices.GetAllTypes();
            var currentHash = TypeModelHasher.Hash(typeModels);
            var modelsHashFile = Path.Combine(_pureLiveDirectory.Value, "models.hash");
            var modelsSrcFile = Path.Combine(_pureLiveDirectory.Value, "models.generated.cs");
            var projFile = Path.Combine(_pureLiveDirectory.Value, "all.generated.cs");
            var dllPathFile = Path.Combine(_pureLiveDirectory.Value, "all.dll.path");

            // caching the generated models speeds up booting
            // currentHash hashes both the types & the user's partials
            if (!forceRebuild)
            {
                _logger.LogDebug("Looking for cached models.");
                if (File.Exists(modelsHashFile) && File.Exists(projFile))
                {
                    var cachedHash = File.ReadAllText(modelsHashFile);
                    if (currentHash != cachedHash)
                    {
                        _logger.LogDebug("Found obsolete cached models.");
                        forceRebuild = true;
                    }

                    // else cachedHash matches currentHash, we can try to load an existing dll
                }
                else
                {
                    _logger.LogDebug("Could not find cached models.");
                    forceRebuild = true;
                }
            }

            Assembly assembly;
            if (!forceRebuild)
            {
                // try to load the dll directly (avoid rebuilding)
                //
                // ensure that the .dll file does not have a corresponding .dll.delete file
                // as that would mean the the .dll file is going to be deleted and should not
                // be re-used - that should not happen in theory, but better be safe
                if (File.Exists(dllPathFile))
                {
                    var dllPath = File.ReadAllText(dllPathFile);

                    _logger.LogDebug($"Cached models dll at {dllPath}.");

                    if (File.Exists(dllPath) && !File.Exists(dllPath + ".delete"))
                    {
                        assembly = ReloadAssembly(dllPath);

                        ModelsBuilderAssemblyAttribute? attr = assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();
                        if (attr != null && attr.IsInMemory && attr.SourceHash == currentHash)
                        {
                            // if we were to resume at that revision, then _ver would keep increasing
                            // and that is probably a bad idea - so, we'll always rebuild starting at
                            // ver 1, but we remember we want to skip that one - so we never end up
                            // with the "same but different" version of the assembly in memory
                            _skipver = assembly.GetName().Version?.Revision;

                            _logger.LogDebug("Loading cached models (dll).");
                            return assembly;
                        }

                        _logger.LogDebug("Cached models dll cannot be loaded (invalid assembly).");
                    }
                    else if (!File.Exists(dllPath))
                    {
                        _logger.LogDebug("Cached models dll does not exist.");
                    }
                    else if (File.Exists(dllPath + ".delete"))
                    {
                        _logger.LogDebug("Cached models dll is marked for deletion.");
                    }
                    else
                    {
                        _logger.LogDebug("Cached models dll cannot be loaded (why?).");
                    }
                }

                // must reset the version in the file else it would keep growing
                // loading cached modules only happens when the app restarts
                var text = File.ReadAllText(projFile);
                Match match = s_assemblyVersionRegex.Match(text);
                if (match.Success)
                {
                    text = text.Replace(match.Value, "AssemblyVersion(\"0.0.0." + _ver + "\")");
                    File.WriteAllText(projFile, text);
                }

                _ver++;
                try
                {
                    var assemblyPath = GetOutputAssemblyPath(currentHash);
                    RoslynCompiler.CompileToFile(projFile, assemblyPath);
                    assembly = ReloadAssembly(assemblyPath);
                    File.WriteAllText(dllPathFile, assembly.Location);
                    File.WriteAllText(modelsHashFile, currentHash);
                    TryDeleteUnusedAssemblies(dllPathFile);
                }
                catch
                {
                    ClearOnFailingToCompile(dllPathFile, modelsHashFile, projFile);
                    throw;
                }

                _logger.LogDebug("Loading cached models (source).");
                return assembly;
            }

            // need to rebuild
            _logger.LogDebug("Rebuilding models.");

            // generate code, save
            var code = GenerateModelsCode(typeModels);

            // add extra attributes,
            //  IsLive=true helps identifying Assemblies that contain live models
            //  AssemblyVersion is so that we have a different version for each rebuild
            var ver = _ver == _skipver ? ++_ver : _ver;
            _ver++;
            string mbAssemblyDirective = $@"[assembly:ModelsBuilderAssembly(IsInMemory = true, SourceHash = ""{currentHash}"")]
[assembly:System.Reflection.AssemblyVersion(""0.0.0.{ver}"")]";
            code = code.Replace("//ASSATTR", mbAssemblyDirective);
            File.WriteAllText(modelsSrcFile, code);

            // generate proj, save
            var projFiles = new Dictionary<string, string>
            {
                { "models.generated.cs", code },
            };
            var proj = GenerateModelsProj(projFiles);
            File.WriteAllText(projFile, proj);

            // compile and register
            try
            {
                var assemblyPath = GetOutputAssemblyPath(currentHash);
                RoslynCompiler.CompileToFile(projFile, assemblyPath);
                assembly = ReloadAssembly(assemblyPath);
                File.WriteAllText(dllPathFile, assemblyPath);
                File.WriteAllText(modelsHashFile, currentHash);
                TryDeleteUnusedAssemblies(dllPathFile);
            }
            catch
            {
                ClearOnFailingToCompile(dllPathFile, modelsHashFile, projFile);
                throw;
            }

            _logger.LogDebug("Done rebuilding.");
            return assembly;
        }

        private void TryDeleteUnusedAssemblies(string dllPathFile)
        {
            if (File.Exists(dllPathFile))
            {
                var dllPath = File.ReadAllText(dllPathFile);
                DirectoryInfo? dirInfo = new DirectoryInfo(dllPath).Parent;
                IEnumerable<FileInfo>? files = dirInfo?.GetFiles().Where(f => f.FullName != dllPath);
                if (files is null)
                {
                    return;
                }

                foreach (FileInfo file in files)
                {
                    try
                    {
                        File.Delete(file.FullName);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // The file is in use, we'll try again next time...
                        // This shouldn't happen anymore.
                    }
                }
            }
        }

        private string GetOutputAssemblyPath(string currentHash)
        {
            var dirInfo = new DirectoryInfo(Path.Combine(_pureLiveDirectory.Value, "Compiled"));
            if (!dirInfo.Exists)
            {
                Directory.CreateDirectory(dirInfo.FullName);
            }

            return Path.Combine(dirInfo.FullName, $"generated.cs{currentHash}.dll");
        }


        private void ClearOnFailingToCompile(string dllPathFile, string modelsHashFile, string projFile)
        {
            _logger.LogDebug("Failed to compile.");

            // the dll file reference still points to the previous dll, which is obsolete
            // now and will be deleted by ASP.NET eventually, so better clear that reference.
            // also touch the proj file to force views to recompile - don't delete as it's
            // useful to have the source around for debugging.
            try
            {
                if (File.Exists(dllPathFile))
                {
                    File.Delete(dllPathFile);
                }

                if (File.Exists(modelsHashFile))
                {
                    File.Delete(modelsHashFile);
                }

                if (File.Exists(projFile))
                {
                    File.SetLastWriteTime(projFile, DateTime.Now);
                }
            }
            catch
            { /* enough */
            }
        }

        private static Infos RegisterModels(IEnumerable<Type> types)
        {
            Type[] ctorArgTypes = new[] { typeof(IPublishedElement), typeof(IPublishedValueFallback) };
            var modelInfos = new Dictionary<string, ModelInfo>(StringComparer.InvariantCultureIgnoreCase);
            var map = new Dictionary<string, Type>();

            foreach (Type type in types)
            {
                ConstructorInfo? constructor = null;
                Type? parameterType = null;

                foreach (ConstructorInfo ctor in type.GetConstructors())
                {
                    ParameterInfo[] parms = ctor.GetParameters();
                    if (parms.Length == 2 && typeof(IPublishedElement).IsAssignableFrom(parms[0].ParameterType) && typeof(IPublishedValueFallback).IsAssignableFrom(parms[1].ParameterType))
                    {
                        if (constructor != null)
                        {
                            throw new InvalidOperationException($"Type {type.FullName} has more than one public constructor with one argument of type, or implementing, IPropertySet.");
                        }

                        constructor = ctor;
                        parameterType = parms[0].ParameterType;
                    }
                }

                if (constructor == null)
                {
                    throw new InvalidOperationException($"Type {type.FullName} is missing a public constructor with one argument of type, or implementing, IPropertySet.");
                }

                PublishedModelAttribute? attribute = type.GetCustomAttribute<PublishedModelAttribute>(false);
                var typeName = attribute == null ? type.Name : attribute.ContentTypeAlias;

                if (modelInfos.TryGetValue(typeName, out var modelInfo))
                {
                    throw new InvalidOperationException($"Both types {type.FullName} and {modelInfo.ModelType?.FullName} want to be a model type for content type with alias \"{typeName}\".");
                }

                // TODO: use Core's ReflectionUtilities.EmitCtor !!
                // Yes .. DynamicMethod is uber slow
                // TODO: But perhaps https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.constructorbuilder?view=netcore-3.1 is better still?
                // See CtorInvokeBenchmarks
                var meth = new DynamicMethod(string.Empty, typeof(IPublishedElement), ctorArgTypes, type.Module, true);
                ILGenerator gen = meth.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Newobj, constructor);
                gen.Emit(OpCodes.Ret);
                var func = (Func<IPublishedElement, IPublishedValueFallback, IPublishedElement>)meth.CreateDelegate(typeof(Func<IPublishedElement, IPublishedValueFallback, IPublishedElement>));

                modelInfos[typeName] = new ModelInfo { ParameterType = parameterType, Ctor = func, ModelType = type };
                map[typeName] = type;
            }

            return new Infos { ModelInfos = modelInfos.Count > 0 ? modelInfos : null, ModelTypeMap = map };
        }

        private string GenerateModelsCode(IList<TypeModel> typeModels)
        {
            if (!Directory.Exists(_pureLiveDirectory.Value))
            {
                Directory.CreateDirectory(_pureLiveDirectory.Value);
            }

            foreach (var file in Directory.GetFiles(_pureLiveDirectory.Value, "*.generated.cs"))
            {
                File.Delete(file);
            }

            var builder = new TextBuilder(_config, typeModels);

            var codeBuilder = new StringBuilder();
            builder.Generate(codeBuilder, builder.GetModelsToGenerate());
            var code = codeBuilder.ToString();

            return code;
        }

        private static string GenerateModelsProj(IDictionary<string, string> files)
        {
            // ideally we would generate a CSPROJ file but then we'd need a BuildProvider for csproj
            // trying to keep things simple for the time being, just write everything to one big file

            // group all 'using' at the top of the file (else fails)
            var usings = new List<string>();
            foreach (string k in files.Keys.ToList())
            {
                files[k] = s_usingRegex.Replace(files[k], m =>
                {
                    usings.Add(m.Groups[1].Value);
                    return string.Empty;
                });
            }

            // group all '[assembly:...]' at the top of the file (else fails)
            var aattrs = new List<string>();
            foreach (string k in files.Keys.ToList())
            {
                files[k] = s_aattrRegex.Replace(files[k], m =>
                {
                    aattrs.Add(m.Groups[1].Value);
                    return string.Empty;
                });
            }

            var text = new StringBuilder();
            foreach (var u in usings.Distinct())
            {
                text.Append("using ");
                text.Append(u);
                text.Append(";\r\n");
            }

            foreach (var a in aattrs)
            {
                text.Append("[assembly:");
                text.Append(a);
                text.Append("]\r\n");
            }

            text.Append("\r\n\r\n");
            foreach (KeyValuePair<string, string> f in files)
            {
                text.Append("// FILE: ");
                text.Append(f.Key);
                text.Append("\r\n\r\n");
                text.Append(f.Value);
                text.Append("\r\n\r\n\r\n");
            }

            text.Append("// EOF\r\n");

            return text.ToString();
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            var changed = args.Name;

            // don't reset when our files change because we are building!
            //
            // comment it out, and always ignore our files, because it seems that some
            // race conditions can occur on slow Cloud filesystems and then we keep
            // rebuilding

            // if (_building && OurFiles.Contains(changed))
            // {
            //    //_logger.LogInformation<InMemoryModelFactory>("Ignoring files self-changes.");
            //    return;
            // }

            // always ignore our own file changes
            if (s_ourFiles.Contains(changed))
            {
                return;
            }

            _logger.LogInformation("Detected files changes.");

            // don't reset while being locked
            lock (SyncRoot)
            {
                ResetModels();
            }
        }

        public void Stop(bool immediate)
        {
            Dispose();

            _hostingLifetime.UnregisterObject(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_watcher is not null)
                    {
                        _watcher.EnableRaisingEvents = false;
                        _watcher.Dispose();
                    }

                    _locker.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }

        internal class Infos
        {
            public Dictionary<string, Type>? ModelTypeMap { get; set; }

            public Dictionary<string, ModelInfo>? ModelInfos { get; set; }
        }

        internal class ModelInfo
        {
            public Type? ParameterType { get; set; }

            public Func<IPublishedElement, IPublishedValueFallback, IPublishedElement>? Ctor { get; set; }

            public Type? ModelType { get; set; }

            public Func<IList>? ListCtor { get; set; }
        }
    }
}
