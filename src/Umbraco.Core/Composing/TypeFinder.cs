using System.Collections.Concurrent;
using System.Reflection;
using System.Security;
using System.Text;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration.UmbracoSettings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Composing;

/// <inheritdoc cref="ITypeFinder" />
public class TypeFinder : ITypeFinder
{
    // TODO: Kill this

    /// <summary>
    ///     this is our assembly filter to filter out known types that def don't contain types we'd like to find or plugins
    /// </summary>
    /// <remarks>
    ///     NOTE the comma vs period... comma delimits the name in an Assembly FullName property so if it ends with comma then
    ///     its an exact name match
    ///     NOTE this means that "foo." will NOT exclude "foo.dll" but only "foo.*.dll"
    /// </remarks>
    internal static readonly string[] KnownAssemblyExclusionFilter =
    {
        "mscorlib,", "netstandard,", "System,", "Antlr3.", "AutoMapper,", "AutoMapper.", "Autofac,", // DI
        "Autofac.", "AzureDirectory,", "Castle.", // DI, tests
        "ClientDependency.", "CookComputing.", "CSharpTest.", // BTree for NuCache
        "DataAnnotationsExtensions,", "DataAnnotationsExtensions.", "Dynamic,", "Examine,", "Examine.",
        "HtmlAgilityPack,", "HtmlAgilityPack.", "HtmlDiff,", "ICSharpCode.", "Iesi.Collections,", // used by NHibernate
        "JetBrains.Annotations,", "LightInject.", // DI
        "LightInject,", "Lucene.", "Markdown,", "Microsoft.", "MiniProfiler,", "Moq,", "MySql.", "NHibernate,",
        "NHibernate.", "Newtonsoft.", "NPoco,", "NuGet.", "RouteDebugger,", "Semver.", "Serilog.", "Serilog,",
        "ServiceStack.", "SqlCE4Umbraco,", "Superpower,", // used by Serilog
        "System.", "TidyNet,", "TidyNet.", "WebDriver,", "itextsharp,", "mscorlib,", "NUnit,", "NUnit.", "NUnit3.",
        "Selenium.", "ImageProcessor", "MiniProfiler.", "Owin,", "SQLite",
        "ReSharperTestRunner32", // used by resharper testrunner
    };

    private static readonly ConcurrentDictionary<string, Type?> TypeNamesCache = new();

    private readonly IAssemblyProvider _assemblyProvider;
    private readonly object _localFilteredAssemblyCacheLocker = new();
    private readonly ILogger<TypeFinder> _logger;
    private readonly List<string> _notifiedLoadExceptionAssemblies = new();

    private readonly ITypeFinderConfig? _typeFinderConfig;

    private string[]? _assembliesAcceptingLoadExceptions;
    private volatile HashSet<Assembly>? _localFilteredAssemblyCache;

    public TypeFinder(ILogger<TypeFinder> logger, IAssemblyProvider assemblyProvider, ITypeFinderConfig? typeFinderConfig = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _assemblyProvider = assemblyProvider;
        _typeFinderConfig = typeFinderConfig;
    }

    /// <inheritdoc />
    public IEnumerable<Assembly> AssembliesToScan
    {
        get
        {
            lock (_localFilteredAssemblyCacheLocker)
            {
                if (_localFilteredAssemblyCache != null)
                {
                    return _localFilteredAssemblyCache;
                }

                IEnumerable<Assembly> assemblies = GetFilteredAssemblies(null, KnownAssemblyExclusionFilter);
                _localFilteredAssemblyCache = new HashSet<Assembly>(assemblies);
                return _localFilteredAssemblyCache;
            }
        }
    }

    // used for benchmark tests
    internal bool QueryWithReferencingAssemblies { get; set; } = true;

    private string[] AssembliesAcceptingLoadExceptions
    {
        get
        {
            if (_assembliesAcceptingLoadExceptions is not null)
            {
                return _assembliesAcceptingLoadExceptions;
            }

            _assembliesAcceptingLoadExceptions =
                _typeFinderConfig?.AssembliesAcceptingLoadExceptions.Where(x => !x.IsNullOrWhiteSpace()).ToArray() ??
                Array.Empty<string>();

            return _assembliesAcceptingLoadExceptions;
        }
    }

    /// <summary>
    ///     Finds any classes derived from the assignTypeFrom Type that contain the attribute TAttribute
    /// </summary>
    /// <param name="assignTypeFrom"></param>
    /// <param name="attributeType"></param>
    /// <param name="assemblies"></param>
    /// <param name="onlyConcreteClasses"></param>
    /// <returns></returns>
    public IEnumerable<Type> FindClassesOfTypeWithAttribute(
        Type assignTypeFrom,
        Type attributeType,
        IEnumerable<Assembly>? assemblies = null,
        bool onlyConcreteClasses = true)
    {
        IEnumerable<Assembly> assemblyList = assemblies ?? AssembliesToScan;

        return GetClassesWithBaseType(assignTypeFrom, assemblyList, onlyConcreteClasses,

            // the additional filter will ensure that any found types also have the attribute applied.
            t => t.GetCustomAttributes(attributeType, false).Any());
    }

    /// <summary>
    ///     Returns all types found of in the assemblies specified of type T
    /// </summary>
    /// <param name="assignTypeFrom"></param>
    /// <param name="assemblies"></param>
    /// <param name="onlyConcreteClasses"></param>
    /// <returns></returns>
    public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly>? assemblies = null, bool onlyConcreteClasses = true)
    {
        IEnumerable<Assembly> assemblyList = assemblies ?? AssembliesToScan;

        return GetClassesWithBaseType(assignTypeFrom, assemblyList, onlyConcreteClasses);
    }

    /// <summary>
    ///     Finds any classes with the attribute.
    /// </summary>
    /// <param name="attributeType">The attribute type </param>
    /// <param name="assemblies">The assemblies.</param>
    /// <param name="onlyConcreteClasses">if set to <c>true</c> only concrete classes.</param>
    /// <returns></returns>
    public IEnumerable<Type> FindClassesWithAttribute(
        Type attributeType,
        IEnumerable<Assembly>? assemblies = null,
        bool onlyConcreteClasses = true)
    {
        IEnumerable<Assembly> assemblyList = assemblies ?? AssembliesToScan;

        return GetClassesWithAttribute(attributeType, assemblyList, onlyConcreteClasses);
    }

    /// <summary>
    ///     Returns a Type for the string type name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual Type? GetTypeByName(string name)
    {
        // NOTE: This will not find types in dynamic assemblies unless those assemblies are already loaded
        // into the appdomain.

        // This is exactly what the BuildManager does, if the type is an assembly qualified type
        // name it will find it.
        if (TypeNameContainsAssembly(name))
        {
            return Type.GetType(name);
        }

        // It didn't parse, so try loading from each already loaded assembly and cache it
        return TypeNamesCache.GetOrAdd(name, s =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(x => x.GetType(s))
                .FirstOrDefault(x => x != null));
    }

    #region Private methods

    // borrowed from aspnet System.Web.UI.Util
    private static bool TypeNameContainsAssembly(string typeName) => CommaIndexInTypeName(typeName) > 0;

    private bool AcceptsLoadExceptions(Assembly a)
    {
        if (AssembliesAcceptingLoadExceptions.Length == 0)
        {
            return false;
        }

        if (AssembliesAcceptingLoadExceptions.Length == 1 && AssembliesAcceptingLoadExceptions[0] == "*")
        {
            return true;
        }

        var name = a.GetName().Name; // simple name of the assembly
        return AssembliesAcceptingLoadExceptions.Any(pattern =>
        {
            if (pattern.Length > name?.Length)
            {
                return false; // pattern longer than name
            }

            if (pattern.Length == name?.Length)
            {
                return pattern.InvariantEquals(name); // same length, must be identical
            }

            if (pattern[pattern.Length] != '.')
            {
                return false; // pattern is shorter than name, must end with dot
            }

            return name?.StartsWith(pattern) ?? false; // and name must start with pattern
        });
    }

    private IEnumerable<Assembly> GetAllAssemblies() => _assemblyProvider.Assemblies;

    /// <summary>
    ///     Return a distinct list of found local Assemblies and excluding the ones passed in and excluding the exclusion list
    ///     filter
    /// </summary>
    /// <param name="excludeFromResults"></param>
    /// <param name="exclusionFilter"></param>
    /// <returns></returns>
    private IEnumerable<Assembly> GetFilteredAssemblies(
        IEnumerable<Assembly>? excludeFromResults = null,
        string[]? exclusionFilter = null)
    {
        if (excludeFromResults == null)
        {
            excludeFromResults = new HashSet<Assembly>();
        }

        if (exclusionFilter == null)
        {
            exclusionFilter = new string[] { };
        }

        return GetAllAssemblies()
            .Where(x => excludeFromResults.Contains(x) == false
                        && exclusionFilter.Any(f => x.FullName?.StartsWith(f) ?? false) == false);
    }

    // borrowed from aspnet System.Web.UI.Util
    private static int CommaIndexInTypeName(string typeName)
    {
        var num1 = typeName.LastIndexOf(',');
        if (num1 < 0)
        {
            return -1;
        }

        var num2 = typeName.LastIndexOf(']');
        if (num2 > num1)
        {
            return -1;
        }

        return typeName.IndexOf(',', num2 + 1);
    }

    private static void AppendCouldNotLoad(StringBuilder sb, Assembly a, bool getAll)
    {
        sb.Append("Could not load ");
        sb.Append(getAll ? "all" : "exported");
        sb.Append(" types from \"");
        sb.Append(a.FullName);
        sb.AppendLine("\" due to LoaderExceptions, skipping:");
    }

    private IEnumerable<Type> GetClassesWithAttribute(
        Type attributeType,
        IEnumerable<Assembly> assemblies,
        bool onlyConcreteClasses)
    {
        if (typeof(Attribute).IsAssignableFrom(attributeType) == false)
        {
            throw new ArgumentException("Type " + attributeType + " is not an Attribute type.");
        }

        var candidateAssemblies = new HashSet<Assembly>(assemblies);
        var attributeAssemblyIsCandidate = candidateAssemblies.Contains(attributeType.Assembly);
        candidateAssemblies.Remove(attributeType.Assembly);
        var types = new List<Type>();

        var stack = new Stack<Assembly>();
        stack.Push(attributeType.Assembly);

        if (!QueryWithReferencingAssemblies)
        {
            foreach (Assembly a in candidateAssemblies)
            {
                stack.Push(a);
            }
        }

        while (stack.Count > 0)
        {
            Assembly assembly = stack.Pop();

            IReadOnlyList<Type>? assemblyTypes = null;
            if (assembly != attributeType.Assembly || attributeAssemblyIsCandidate)
            {
                // get all assembly types that can be assigned to baseType
                try
                {
                    assemblyTypes = GetTypesWithFormattedException(assembly)
                        .ToList(); // in try block
                }
                catch (TypeLoadException ex)
                {
                    _logger.LogError(
                        ex,
                        "Could not query types on {Assembly} assembly, this is most likely due to this assembly not being compatible with the current Umbraco version",
                        assembly);
                    continue;
                }

                types.AddRange(assemblyTypes.Where(x =>
                    x.IsClass // only classes
                    && (x.IsAbstract == false || x.IsSealed == false) // ie non-static, static is abstract and sealed
                    && x.IsNestedPrivate == false // exclude nested private
                    && (onlyConcreteClasses == false || x.IsAbstract == false) // exclude abstract
                    && x.GetCustomAttribute<HideFromTypeFinderAttribute>() == null // exclude hidden
                    && x.GetCustomAttributes(attributeType, false).Any())); // marked with the attribute
            }

            if (assembly != attributeType.Assembly &&
                assemblyTypes?.Where(attributeType.IsAssignableFrom).Any() == false)
            {
                continue;
            }

            if (QueryWithReferencingAssemblies)
            {
                foreach (Assembly referencing in TypeHelper.GetReferencingAssemblies(assembly, candidateAssemblies))
                {
                    candidateAssemblies.Remove(referencing);
                    stack.Push(referencing);
                }
            }
        }

        return types;
    }

    /// <summary>
    ///     Finds types that are assignable from the assignTypeFrom parameter and will scan for these types in the assembly
    ///     list passed in, however we will only scan assemblies that have a reference to the assignTypeFrom Type or any type
    ///     deriving from the base type.
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="assemblies"></param>
    /// <param name="onlyConcreteClasses"></param>
    /// <param name="additionalFilter">
    ///     An additional filter to apply for what types will actually be included in the return
    ///     value
    /// </param>
    /// <returns></returns>
    private IEnumerable<Type> GetClassesWithBaseType(
        Type baseType,
        IEnumerable<Assembly> assemblies,
        bool onlyConcreteClasses,
        Func<Type, bool>? additionalFilter = null)
    {
        var candidateAssemblies = new HashSet<Assembly>(assemblies);
        var baseTypeAssemblyIsCandidate = candidateAssemblies.Contains(baseType.Assembly);
        candidateAssemblies.Remove(baseType.Assembly);
        var types = new List<Type>();

        var stack = new Stack<Assembly>();
        stack.Push(baseType.Assembly);

        if (!QueryWithReferencingAssemblies)
        {
            foreach (Assembly a in candidateAssemblies)
            {
                stack.Push(a);
            }
        }

        while (stack.Count > 0)
        {
            Assembly assembly = stack.Pop();

            // get all assembly types that can be assigned to baseType
            IReadOnlyList<Type>? assemblyTypes = null;
            if (assembly != baseType.Assembly || baseTypeAssemblyIsCandidate)
            {
                try
                {
                    assemblyTypes = GetTypesWithFormattedException(assembly)
                        .Where(baseType.IsAssignableFrom)
                        .ToList(); // in try block
                }
                catch (TypeLoadException ex)
                {
                    _logger.LogError(
                        ex,
                        "Could not query types on {Assembly} assembly, this is most likely due to this assembly not being compatible with the current Umbraco version",
                        assembly);
                    continue;
                }

                types.AddRange(assemblyTypes.Where(x =>
                    x.IsClass // only classes
                    && (x.IsAbstract == false || x.IsSealed == false) // ie non-static, static is abstract and sealed
                    && x.IsNestedPrivate == false // exclude nested private
                    && (onlyConcreteClasses == false || x.IsAbstract == false) // exclude abstract
                    && x.GetCustomAttribute<HideFromTypeFinderAttribute>(false) == null // exclude hidden
                    && (additionalFilter == null || additionalFilter(x)))); // filter
            }

            if (assembly != baseType.Assembly && (assemblyTypes?.All(x => x.IsSealed) ?? false))
            {
                continue;
            }

            if (QueryWithReferencingAssemblies)
            {
                foreach (Assembly referencing in TypeHelper.GetReferencingAssemblies(assembly, candidateAssemblies))
                {
                    candidateAssemblies.Remove(referencing);
                    stack.Push(referencing);
                }
            }
        }

        return types;
    }

    private IEnumerable<Type> GetTypesWithFormattedException(Assembly a)
    {
        // if the assembly is dynamic, do not try to scan it
        if (a.IsDynamic)
        {
            return Enumerable.Empty<Type>();
        }

        var getAll = a.GetCustomAttribute<AllowPartiallyTrustedCallersAttribute>() == null;

        try
        {
            // we need to detect if an assembly is partially trusted, if so we cannot go interrogating all of it's types
            // only its exported types, otherwise we'll get exceptions.
            return getAll ? a.GetTypes() : a.GetExportedTypes();
        }

        // GetExportedTypes *can* throw TypeLoadException!
        catch (TypeLoadException ex)
        {
            var sb = new StringBuilder();
            AppendCouldNotLoad(sb, a, getAll);
            AppendLoaderException(sb, ex);

            // rethrow as ReflectionTypeLoadException (for consistency) with new message
            throw new ReflectionTypeLoadException(new Type[0], new Exception[] { ex }, sb.ToString());
        }

        // GetTypes throws ReflectionTypeLoadException
        catch (ReflectionTypeLoadException rex)
        {
            var sb = new StringBuilder();
            AppendCouldNotLoad(sb, a, getAll);
            foreach (Exception loaderException in rex.LoaderExceptions.WhereNotNull())
            {
                AppendLoaderException(sb, loaderException);
            }

            var ex = new ReflectionTypeLoadException(rex.Types, rex.LoaderExceptions, sb.ToString());

            // rethrow with new message, unless accepted
            if (AcceptsLoadExceptions(a) == false)
            {
                throw ex;
            }

            // log a warning, and return what we can
            lock (_notifiedLoadExceptionAssemblies)
            {
                if (a.FullName is not null && _notifiedLoadExceptionAssemblies.Contains(a.FullName) == false)
                {
                    _notifiedLoadExceptionAssemblies.Add(a.FullName);
                    _logger.LogWarning(ex, "Could not load all types from {TypeName}.", a.GetName().Name);
                }
            }

            return rex.Types.WhereNotNull().ToArray();
        }
    }

    private static void AppendLoaderException(StringBuilder sb, Exception loaderException)
    {
        sb.Append(". ");
        sb.Append(loaderException.GetType().FullName);

        if (loaderException is TypeLoadException tloadex)
        {
            sb.Append(" on ");
            sb.Append(tloadex.TypeName);
        }

        sb.Append(": ");
        sb.Append(loaderException.Message);
        sb.AppendLine();
    }

    #endregion
}
