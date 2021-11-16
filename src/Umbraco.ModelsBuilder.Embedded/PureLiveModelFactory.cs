using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.WebPages.Razor;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.ModelsBuilder.Embedded.Building;
using Umbraco.ModelsBuilder.Embedded.Configuration;
using File = System.IO.File;

namespace Umbraco.ModelsBuilder.Embedded
{
    internal class PureLiveModelFactory : ILivePublishedModelFactory2, IRegisteredObject, IDisposable
    {
        private Assembly _modelsAssembly;
        private Infos _infos = new Infos { ModelInfos = null, ModelTypeMap = new Dictionary<string, Type>() };
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private volatile bool _hasModels; // volatile 'cos reading outside lock
        private bool _pendingRebuild;
        private readonly IProfilingLogger _logger;
        private readonly FileSystemWatcher _watcher;
        private int _ver, _skipver;
        private readonly int _debugLevel;
        private BuildManager _theBuildManager;
        private bool _disposedValue;
        private readonly Lazy<UmbracoServices> _umbracoServices; // fixme: this is because of circular refs :(
        private UmbracoServices UmbracoServices => _umbracoServices.Value;

        private static readonly Regex AssemblyVersionRegex = new Regex("AssemblyVersion\\(\"[0-9]+.[0-9]+.[0-9]+.[0-9]+\"\\)", RegexOptions.Compiled);
        private const string ProjVirt = "~/App_Data/Models/all.generated.cs";
        private static readonly string[] OurFiles = { "models.hash", "models.generated.cs", "all.generated.cs", "all.dll.path", "models.err" };

        private readonly IModelsBuilderConfig _config;
        private readonly ModelsGenerationError _errors;

        public PureLiveModelFactory(Lazy<UmbracoServices> umbracoServices, IProfilingLogger logger, IModelsBuilderConfig config)
        {
            _umbracoServices = umbracoServices;
            _logger = logger;
            _config = config;
            _errors = new ModelsGenerationError(config);
            _ver = 1; // zero is for when we had no version
            _skipver = -1; // nothing to skip

            RazorBuildProvider.CodeGenerationStarted += RazorBuildProvider_CodeGenerationStarted;

            if (!HostingEnvironment.IsHosted) return;

            var modelsDirectory = _config.ModelsDirectory;
            if (!Directory.Exists(modelsDirectory))
                Directory.CreateDirectory(modelsDirectory);

            // BEWARE! if the watcher is not properly released then for some reason the
            // BuildManager will start confusing types - using a 'registered object' here
            // though we should probably plug into Umbraco's MainDom - which is internal
            HostingEnvironment.RegisterObject(this);
            _watcher = new FileSystemWatcher(modelsDirectory);
            _watcher.Changed += WatcherOnChanged;
            _watcher.EnableRaisingEvents = true;

            // get it here, this need to be fast
            _debugLevel = _config.DebugLevel;
        }

        #region ILivePublishedModelFactory

        /// <inheritdoc />
        public object SyncRoot { get; } = new object();

        /// <inheritdoc />
        public void Refresh()
        {
            ResetModels();
            EnsureModels();
        }

        #endregion

        #region IPublishedModelFactory

        public IPublishedElement CreateModel(IPublishedElement element)
        {
            // get models, rebuilding them if needed
            var infos = EnsureModels()?.ModelInfos;
            if (infos == null)
                return element;

            // be case-insensitive
            var contentTypeAlias = element.ContentType.Alias;

            // lookup model constructor (else null)
            infos.TryGetValue(contentTypeAlias, out var info);

            // create model
            return info == null ? element : info.Ctor(element);
        }

        // this runs only once the factory is ready
        // NOT when building models
        public Type MapModelType(Type type)
        {
            var infos = EnsureModels();
            return ModelType.Map(type, infos.ModelTypeMap);
        }

        // this runs only once the factory is ready
        // NOT when building models
        public IList CreateModelList(string alias)
        {
            var infos = EnsureModels();

            // fail fast
            if (infos == null)
                return new List<IPublishedElement>();

            if (!infos.ModelInfos.TryGetValue(alias, out var modelInfo))
                return new List<IPublishedElement>();

            var ctor = modelInfo.ListCtor;
            if (ctor != null) return ctor();

            var listType = typeof(List<>).MakeGenericType(modelInfo.ModelType);
            ctor = modelInfo.ListCtor = ReflectionUtilities.EmitConstructor<Func<IList>>(declaring: listType);
            return ctor();
        }

        /// <inheritdoc />
        public bool Enabled => _config.Enable;

        /// <inheritdoc />
        public void Reset()
        {
            if (_config.Enable)
                ResetModels();
        }

        #endregion

        #region Compilation

        // deadlock note
        //
        // when RazorBuildProvider_CodeGenerationStarted runs, the thread has Monitor.Enter-ed the BuildManager
        // singleton instance, through a call to CompilationLock.GetLock in BuildManager.GetVPathBuildResultInternal,
        // and now wants to lock _locker.
        // when EnsureModels runs, the thread locks _locker and then wants BuildManager to compile, which in turns
        // requires that the BuildManager can Monitor.Enter-ed itself.
        // so:
        //
        // T1 - needs to ensure models, locks _locker
        // T2 - needs to compile a view, locks BuildManager
        //      hits RazorBuildProvider_CodeGenerationStarted
        //      wants to lock _locker, wait
        // T1 - needs to compile models, using BuildManager
        //      wants to lock itself, wait
        // <deadlock>
        //
        // until ASP.NET kills the long-running request (thread abort)
        //
        // problem is, we *want* to suspend views compilation while the models assembly is being changed else we
        // end up with views compiled and cached with the old assembly, while models come from the new assembly,
        // which gives more YSOD. so we *have* to lock _locker in RazorBuildProvider_CodeGenerationStarted.
        //
        // one "easy" solution consists in locking the BuildManager *before* _locker in EnsureModels, thus ensuring
        // we always lock in the same order, and getting rid of deadlocks - but that requires having access to the
        // current BuildManager instance, which is BuildManager.TheBuildManager, which is an internal property.
        //
        // well, that's what we are doing in this class' TheBuildManager property, using reflection.

        private void RazorBuildProvider_CodeGenerationStarted(object sender, EventArgs e)
        {
            try
            {
                _locker.EnterReadLock();

                // just be safe - can happen if the first view is not an Umbraco view,
                // or if something went wrong and we don't have an assembly at all
                if (_modelsAssembly == null) return;

                if (_debugLevel > 0)
                    _logger.Debug<PureLiveModelFactory>("RazorBuildProvider.CodeGenerationStarted");
                if (!(sender is RazorBuildProvider provider)) return;

                // add the assembly, and add a dependency to a text file that will change on each
                // compilation as in some environments (could not figure which/why) the BuildManager
                // would not re-compile the views when the models assembly is rebuilt.
                provider.AssemblyBuilder.AddAssemblyReference(_modelsAssembly);
                provider.AddVirtualPathDependency(ProjVirt);
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }
        }

        // tells the factory that it should build a new generation of models
        private void ResetModels()
        {
            _logger.Debug<PureLiveModelFactory>("Resetting models.");

            try
            {
                _locker.EnterWriteLock();

                _hasModels = false;
                _pendingRebuild = true;

                var modelsDirectory = _config.ModelsDirectory;
                if (!Directory.Exists(modelsDirectory))
                    Directory.CreateDirectory(modelsDirectory);

                // clear stuff
                var modelsHashFile = Path.Combine(modelsDirectory, "models.hash");
                //var modelsSrcFile = Path.Combine(modelsDirectory, "models.generated.cs");
                //var projFile = Path.Combine(modelsDirectory, "all.generated.cs");
                var dllPathFile = Path.Combine(modelsDirectory, "all.dll.path");

                if (File.Exists(dllPathFile)) File.Delete(dllPathFile);
                if (File.Exists(modelsHashFile)) File.Delete(modelsHashFile);
            }
            finally
            {
                if (_locker.IsWriteLockHeld)
                    _locker.ExitWriteLock();
            }
        }

        // gets "the" build manager
        private BuildManager TheBuildManager
        {
            get
            {
                if (_theBuildManager != null) return _theBuildManager;
                var prop = typeof(BuildManager).GetProperty("TheBuildManager", BindingFlags.NonPublic | BindingFlags.Static);
                if (prop == null)
                    throw new InvalidOperationException("Could not get BuildManager.TheBuildManager property.");
                _theBuildManager = (BuildManager)prop.GetValue(null);
                return _theBuildManager;
            }
        }

        // ensure that the factory is running with the lastest generation of models
        internal Infos EnsureModels()
        {
            if (_debugLevel > 0)
                _logger.Debug<PureLiveModelFactory>("Ensuring models.");

            // don't use an upgradeable lock here because only 1 thread at a time could enter it
            try
            {
                _locker.EnterReadLock();
                if (_hasModels)
                    return _infos;
            }
            finally
            {
                if (_locker.IsReadLockHeld)
                    _locker.ExitReadLock();
            }

            var buildManagerLocked = false;
            try
            {
                // always take the BuildManager lock *before* taking the _locker lock
                // to avoid possible deadlock situations (see notes above)
                Monitor.Enter(TheBuildManager, ref buildManagerLocked);

                _locker.EnterUpgradeableReadLock();

                if (_hasModels) return _infos;

                _locker.EnterWriteLock();

                // we don't have models,
                // either they haven't been loaded from the cache yet
                // or they have been reseted and are pending a rebuild

                using (_logger.DebugDuration<PureLiveModelFactory>("Get models.", "Got models."))
                {
                    try
                    {
                        var assembly = GetModelsAssembly(_pendingRebuild);

                        // the one below can be used to simulate an issue with BuildManager, ie it will register
                        // the models with the factory but NOT with the BuildManager, which will not recompile views.
                        // this is for U4-8043 which is an obvious issue but I cannot replicate
                        //_modelsAssembly = _modelsAssembly ?? assembly;

                        // the one below is the normal one
                        _modelsAssembly = assembly;

                        var types = assembly.ExportedTypes.Where(x => x.Inherits<PublishedContentModel>() || x.Inherits<PublishedElementModel>());
                        _infos = RegisterModels(types);
                        _errors.Clear();
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            _logger.Error<PureLiveModelFactory>(e, "Failed to build models.");
                            _logger.Warn<PureLiveModelFactory>("Running without models."); // be explicit
                            _errors.Report("Failed to build PureLive models.", e);
                        }
                        finally
                        {
                            _modelsAssembly = null;
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
                    _locker.ExitWriteLock();
                if (_locker.IsUpgradeableReadLockHeld)
                    _locker.ExitUpgradeableReadLock();
                if (buildManagerLocked)
                    Monitor.Exit(TheBuildManager);
            }
        }

        private Assembly GetModelsAssembly(bool forceRebuild)
        {
            var modelsDirectory = _config.ModelsDirectory;
            if (!Directory.Exists(modelsDirectory))
                Directory.CreateDirectory(modelsDirectory);

            var typeModels = UmbracoServices.GetAllTypes();
            var currentHash = TypeModelHasher.Hash(typeModels);
            var modelsHashFile = Path.Combine(modelsDirectory, "models.hash");
            var modelsSrcFile = Path.Combine(modelsDirectory, "models.generated.cs");
            var projFile = Path.Combine(modelsDirectory, "all.generated.cs");
            var dllPathFile = Path.Combine(modelsDirectory, "all.dll.path");

            // caching the generated models speeds up booting
            // currentHash hashes both the types & the user's partials

            if (!forceRebuild)
            {
                _logger.Debug<PureLiveModelFactory>("Looking for cached models.");
                if (File.Exists(modelsHashFile) && File.Exists(projFile))
                {
                    var cachedHash = File.ReadAllText(modelsHashFile);
                    if (currentHash != cachedHash)
                    {
                        _logger.Debug<PureLiveModelFactory>("Found obsolete cached models.");
                        forceRebuild = true;
                    }

                    // else cachedHash matches currentHash, we can try to load an existing dll
                }
                else
                {
                    _logger.Debug<PureLiveModelFactory>("Could not find cached models.");
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
                //
                // ensure that the .dll file is in the current codegen directory - when IIS
                // or Express does a full restart, it can switch to an entirely new codegen
                // directory, and then we end up referencing a dll which is *not* in that
                // directory, and BuildManager fails to instantiate views ("the view found
                // at ... was not created").
                //
                if (File.Exists(dllPathFile))
                {
                    var dllPath = File.ReadAllText(dllPathFile);
                    var codegen = HttpRuntime.CodegenDir;

                    _logger.Debug<PureLiveModelFactory>($"Cached models dll at {dllPath}.");

                    if (File.Exists(dllPath) && !File.Exists(dllPath + ".delete") && dllPath.StartsWith(codegen))
                    {
                        assembly = Assembly.LoadFile(dllPath);
                        var attr = assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();
                        if (attr != null && attr.PureLive && attr.SourceHash == currentHash)
                        {
                            // if we were to resume at that revision, then _ver would keep increasing
                            // and that is probably a bad idea - so, we'll always rebuild starting at
                            // ver 1, but we remember we want to skip that one - so we never end up
                            // with the "same but different" version of the assembly in memory
                            _skipver = assembly.GetName().Version.Revision;

                            _logger.Debug<PureLiveModelFactory>("Loading cached models (dll).");
                            return assembly;
                        }

                        _logger.Debug<PureLiveModelFactory>("Cached models dll cannot be loaded (invalid assembly).");
                    }
                    else if (!File.Exists(dllPath))
                        _logger.Debug<PureLiveModelFactory>("Cached models dll does not exist.");
                    else if (File.Exists(dllPath + ".delete"))
                        _logger.Debug<PureLiveModelFactory>("Cached models dll is marked for deletion.");
                    else if (!dllPath.StartsWith(codegen))
                        _logger.Debug<PureLiveModelFactory>("Cached models dll is in a different codegen directory.");
                    else
                        _logger.Debug<PureLiveModelFactory>("Cached models dll cannot be loaded (why?).");
                }

                // must reset the version in the file else it would keep growing
                // loading cached modules only happens when the app restarts
                var text = File.ReadAllText(projFile);
                var match = AssemblyVersionRegex.Match(text);
                if (match.Success)
                {
                    text = text.Replace(match.Value, "AssemblyVersion(\"0.0.0." + _ver + "\")");
                    File.WriteAllText(projFile, text);
                }

                // generate a marker file that will be a dependency
                // see note in RazorBuildProvider_CodeGenerationStarted
                // NO: using all.generated.cs as a dependency
                //File.WriteAllText(Path.Combine(modelsDirectory, "models.dep"), "VER:" + _ver);

                _ver++;
                try
                {
                    assembly = BuildManager.GetCompiledAssembly(ProjVirt);
                    File.WriteAllText(dllPathFile, assembly.Location);
                }
                catch
                {
                    ClearOnFailingToCompile(dllPathFile, modelsHashFile, projFile);
                    throw;
                }

                _logger.Debug<PureLiveModelFactory>("Loading cached models (source).");
                return assembly;
            }

            // need to rebuild
            _logger.Debug<PureLiveModelFactory>("Rebuilding models.");

            // generate code, save
            var code = GenerateModelsCode(typeModels);
            // add extra attributes,
            //  PureLiveAssembly helps identifying Assemblies that contain PureLive models
            //  AssemblyVersion is so that we have a different version for each rebuild
            var ver = _ver == _skipver ? ++_ver : _ver;
            _ver++;
            code = code.Replace("//ASSATTR", $@"[assembly:ModelsBuilderAssembly(PureLive = true, SourceHash = ""{currentHash}"")]
[assembly:System.Reflection.AssemblyVersion(""0.0.0.{ver}"")]");
            File.WriteAllText(modelsSrcFile, code);

            // generate proj, save
            var projFiles = new Dictionary<string, string>
            {
                { "models.generated.cs", code }
            };
            var proj = GenerateModelsProj(projFiles);
            File.WriteAllText(projFile, proj);

            // compile and register
            try
            {
                assembly = BuildManager.GetCompiledAssembly(ProjVirt);
                File.WriteAllText(dllPathFile, assembly.Location);
                File.WriteAllText(modelsHashFile, currentHash);
            }
            catch
            {
                ClearOnFailingToCompile(dllPathFile, modelsHashFile, projFile);
                throw;
            }

            _logger.Debug<PureLiveModelFactory>("Done rebuilding.");
            return assembly;
        }

        private void ClearOnFailingToCompile(string dllPathFile, string modelsHashFile, string projFile)
        {
            _logger.Debug<PureLiveModelFactory>("Failed to compile.");

            // the dll file reference still points to the previous dll, which is obsolete
            // now and will be deleted by ASP.NET eventually, so better clear that reference.
            // also touch the proj file to force views to recompile - don't delete as it's
            // useful to have the source around for debugging.
            try
            {
                if (File.Exists(dllPathFile)) File.Delete(dllPathFile);
                if (File.Exists(modelsHashFile)) File.Delete(modelsHashFile);
                if (File.Exists(projFile)) File.SetLastWriteTime(projFile, DateTime.Now);
            }
            catch { /* enough */ }
        }

        private static Infos RegisterModels(IEnumerable<Type> types)
        {
            var ctorArgTypes = new[] { typeof(IPublishedElement) };
            var modelInfos = new Dictionary<string, ModelInfo>(StringComparer.InvariantCultureIgnoreCase);
            var map = new Dictionary<string, Type>();

            foreach (var type in types)
            {
                ConstructorInfo constructor = null;
                Type parameterType = null;

                foreach (var ctor in type.GetConstructors())
                {
                    var parms = ctor.GetParameters();
                    if (parms.Length == 1 && typeof(IPublishedElement).IsAssignableFrom(parms[0].ParameterType))
                    {
                        if (constructor != null)
                            throw new InvalidOperationException($"Type {type.FullName} has more than one public constructor with one argument of type, or implementing, IPropertySet.");
                        constructor = ctor;
                        parameterType = parms[0].ParameterType;
                    }
                }

                if (constructor == null)
                    throw new InvalidOperationException($"Type {type.FullName} is missing a public constructor with one argument of type, or implementing, IPropertySet.");

                var attribute = type.GetCustomAttribute<PublishedModelAttribute>(false);
                var typeName = attribute == null ? type.Name : attribute.ContentTypeAlias;

                if (modelInfos.TryGetValue(typeName, out var modelInfo))
                    throw new InvalidOperationException($"Both types {type.FullName} and {modelInfo.ModelType.FullName} want to be a model type for content type with alias \"{typeName}\".");

                // TODO: use Core's ReflectionUtilities.EmitCtor !!
                // Yes .. DynamicMethod is uber slow
                // TODO: But perhaps https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.constructorbuilder?view=netcore-3.1 is better still?
                // See CtorInvokeBenchmarks
                var meth = new DynamicMethod(string.Empty, typeof(IPublishedElement), ctorArgTypes, type.Module, true);
                var gen = meth.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Newobj, constructor);
                gen.Emit(OpCodes.Ret);
                var func = (Func<IPublishedElement, IPublishedElement>)meth.CreateDelegate(typeof(Func<IPublishedElement, IPublishedElement>));

                modelInfos[typeName] = new ModelInfo { ParameterType = parameterType, Ctor = func, ModelType = type };
                map[typeName] = type;
            }

            return new Infos { ModelInfos = modelInfos.Count > 0 ? modelInfos : null, ModelTypeMap = map };
        }

        private string GenerateModelsCode(IList<TypeModel> typeModels)
        {
            var modelsDirectory = _config.ModelsDirectory;
            if (!Directory.Exists(modelsDirectory))
                Directory.CreateDirectory(modelsDirectory);

            foreach (var file in Directory.GetFiles(modelsDirectory, "*.generated.cs"))
                File.Delete(file);

            var builder = new TextBuilder(_config, typeModels);

            var codeBuilder = new StringBuilder();
            builder.Generate(codeBuilder, builder.GetModelsToGenerate());
            var code = codeBuilder.ToString();

            return code;
        }

        private static readonly Regex UsingRegex = new Regex("^using(.*);", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex AattrRegex = new Regex("^\\[assembly:(.*)\\]", RegexOptions.Compiled | RegexOptions.Multiline);

        private static string GenerateModelsProj(IDictionary<string, string> files)
        {
            // ideally we would generate a CSPROJ file but then we'd need a BuildProvider for csproj
            // trying to keep things simple for the time being, just write everything to one big file

            // group all 'using' at the top of the file (else fails)
            var usings = new List<string>();
            foreach (var k in files.Keys.ToList())
                files[k] = UsingRegex.Replace(files[k], m =>
                {
                    usings.Add(m.Groups[1].Value);
                    return string.Empty;
                });

            // group all '[assembly:...]' at the top of the file (else fails)
            var aattrs = new List<string>();
            foreach (var k in files.Keys.ToList())
                files[k] = AattrRegex.Replace(files[k], m =>
                {
                    aattrs.Add(m.Groups[1].Value);
                    return string.Empty;
                });

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
            foreach (var f in files)
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

        internal class Infos
        {
            public Dictionary<string, Type> ModelTypeMap { get; set; }
            public Dictionary<string, ModelInfo> ModelInfos { get; set; }
        }

        internal class ModelInfo
        {
            public Type ParameterType { get; set; }
            public Func<IPublishedElement, IPublishedElement> Ctor { get; set; }
            public Type ModelType { get; set; }
            public Func<IList> ListCtor { get; set; }
        }

        #endregion

        #region Watching

        private void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            var changed = args.Name;

            // don't reset when our files change because we are building!
            //
            // comment it out, and always ignore our files, because it seems that some
            // race conditions can occur on slow Cloud filesystems and then we keep
            // rebuilding

            //if (_building && OurFiles.Contains(changed))
            //{
            //    //_logger.Info<PureLiveModelFactory>("Ignoring files self-changes.");
            //    return;
            //}

            // always ignore our own file changes
            if (OurFiles.Contains(changed))
                return;

            _logger.Info<PureLiveModelFactory>("Detected files changes.");

            lock (SyncRoot) // don't reset while being locked
                ResetModels();
        }

        public void Stop(bool immediate)
        {
            Dispose();
            HostingEnvironment.UnregisterObject(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Dispose();
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

        #endregion
    }
}
