using System.Reflection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Collections;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Provides methods to find and instantiate types.
/// </summary>
/// <remarks>
///     <para>
///         This class should be used to get all types, the <see cref="ITypeFinder" /> class should never be used
///         directly.
///     </para>
///     <para>In most cases this class is not used directly but through extension methods that retrieve specific types.</para>
/// </remarks>
public sealed class TypeLoader
{
    private readonly Lock _typesLock = new();
    private readonly ILogger<TypeLoader> _logger;

    private readonly Dictionary<CompositeTypeTypeKey, TypeList> _types = new();

    private IEnumerable<Assembly>? _assemblies;

    private bool IsDebugEnabled => _logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug);

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeLoader"/> class.
    /// </summary>
    /// <param name="typeFinder">The type finder used to discover types.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="assembliesToScan">Optional set of assemblies to scan.</param>
    public TypeLoader(
        ITypeFinder typeFinder,
        ILogger<TypeLoader> logger,
        IEnumerable<Assembly>? assembliesToScan = null)
    {
        TypeFinder = typeFinder ?? throw new ArgumentNullException(nameof(typeFinder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _assemblies = assembliesToScan;
    }

    /// <summary>
    ///     Returns the underlying <see cref="ITypeFinder" />
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public ITypeFinder TypeFinder { get; }

    /// <summary>
    ///     Gets or sets the set of assemblies to scan.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If not explicitly set, defaults to all assemblies except those that are know to not have any of the
    ///         types we might scan. Because we only scan for application types, this means we can safely exclude GAC
    ///         assemblies
    ///         for example.
    ///     </para>
    ///     <para>This is for unit tests.</para>
    /// </remarks>
    // internal for tests
    [Obsolete("This will be removed in a future version.")]
    public IEnumerable<Assembly> AssembliesToScan => _assemblies ??= TypeFinder.AssembliesToScan;

    /// <summary>
    ///     Gets the type lists.
    /// </summary>
    /// <remarks>For unit tests.</remarks>
    // internal for tests
    [Obsolete("This will be removed in a future version.")]
    public IEnumerable<TypeList> TypeLists => _types.Values;

    #region Get Assembly Attributes

    /// <summary>
    ///     Gets the assembly attributes of the specified <paramref name="attributeTypes" />.
    /// </summary>
    /// <param name="attributeTypes">The attribute types.</param>
    /// <returns>
    ///     The assembly attributes of the specified types.
    /// </returns>
    /// <exception cref="ArgumentNullException">attributeTypes</exception>
    public IEnumerable<Attribute> GetAssemblyAttributes(params Type[] attributeTypes)
    {
        if (attributeTypes == null)
        {
            throw new ArgumentNullException(nameof(attributeTypes));
        }

        return AssembliesToScan.SelectMany(a => attributeTypes.SelectMany(at => a.GetCustomAttributes(at))).ToList();
    }

    #endregion

    #region Cache

    #endregion

    #region Get Types

    /// <summary>
    ///     Gets class types inheriting from or implementing the specified type
    /// </summary>
    /// <typeparam name="T">The type to inherit from or implement.</typeparam>
    /// <param name="cache">Indicates whether to use cache for type resolution.</param>
    /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
    /// <returns>All class types inheriting from or implementing the specified type.</returns>
    /// <remarks>Caching is disabled when using specific assemblies.</remarks>
    public IEnumerable<Type> GetTypes<T>(bool cache = true, IEnumerable<Assembly>? specificAssemblies = null)
    {
        EnsureInitialized();

        // do not cache anything from specific assemblies
        cache &= specificAssemblies == null;

        // if not IDiscoverable, directly get types
        if (!typeof(IDiscoverable).IsAssignableFrom(typeof(T)))
        {
            LogDebugIf(
                true,
                "Running a full, {CacheStatus}cached, scan for non-discoverable type {TypeName} (slow).",
                CacheStatus(cache),
                typeof(T).FullName);

            return GetTypesInternal(
                typeof(T),
                null,
                () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                "scanning assemblies",
                cache);
        }

        // get IDiscoverable and always cache
        IEnumerable<Type> discovered = GetDiscoverableTypes();

        LogDebugIf(
            !cache,
            "Running a non-cached, filter for discoverable type {TypeName} (slowish).",
            typeof(T).FullName);

        // filter the cached discovered types (and maybe cache the result)
        return GetTypesInternal(
            typeof(T),
            null,
            () => discovered.Where(x => typeof(T).IsAssignableFrom(x)),
            "filtering IDiscoverable",
            cache);
    }

    /// <summary>
    ///     Gets class types inheriting from or implementing the specified type and marked with the specified attribute.
    /// </summary>
    /// <typeparam name="T">The type to inherit from or implement.</typeparam>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="cache">Indicates whether to use cache for type resolution.</param>
    /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
    /// <returns>All class types inheriting from or implementing the specified type and marked with the specified attribute.</returns>
    /// <remarks>Caching is disabled when using specific assemblies.</remarks>
    public IEnumerable<Type> GetTypesWithAttribute<T, TAttribute>(
        bool cache = true,
        IEnumerable<Assembly>? specificAssemblies = null)
        where TAttribute : Attribute
    {
        EnsureInitialized();

        // do not cache anything from specific assemblies
        cache &= specificAssemblies == null;

        // if not IDiscoverable, directly get types
        if (!typeof(IDiscoverable).IsAssignableFrom(typeof(T)))
        {
            LogDebugIf(
                true,
                "Running a full, {CacheStatus}cached, scan for non-discoverable type {TypeName} / attribute {AttributeName} (slow).",
                CacheStatus(cache),
                typeof(T).FullName,
                typeof(TAttribute).FullName);

            return GetTypesInternal(
                typeof(T),
                typeof(TAttribute),
                () => TypeFinder.FindClassesOfTypeWithAttribute<T, TAttribute>(specificAssemblies ?? AssembliesToScan),
                "scanning assemblies",
                cache);
        }

        // get IDiscoverable and always cache
        IEnumerable<Type> discovered = GetDiscoverableTypes();

        LogDebugIf(
            !cache,
            "Running a non-cached, filter for discoverable type {TypeName} / attribute {AttributeName} (slowish).",
            typeof(T).FullName,
            typeof(TAttribute).FullName);

        // filter the cached discovered types (and maybe cache the result)
        return GetTypesInternal(
            typeof(T),
            typeof(TAttribute),
            () => discovered
                .Where(x => typeof(T).IsAssignableFrom(x))
                .Where(x => x.GetCustomAttributes<TAttribute>(false).Any()),
            "filtering IDiscoverable",
            cache);
    }

    /// <summary>
    ///     Gets class types marked with the specified attribute.
    /// </summary>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="cache">Indicates whether to use cache for type resolution.</param>
    /// <param name="specificAssemblies">A set of assemblies for type resolution.</param>
    /// <returns>All class types marked with the specified attribute.</returns>
    /// <remarks>Caching is disabled when using specific assemblies.</remarks>
    public IEnumerable<Type> GetAttributedTypes<TAttribute>(
        bool cache = true,
        IEnumerable<Assembly>? specificAssemblies = null)
        where TAttribute : Attribute
    {
        EnsureInitialized();

        // do not cache anything from specific assemblies
        cache &= specificAssemblies == null;

        LogDebugIf(
            !cache,
            "Running a full, non-cached, scan for types / attribute {AttributeName} (slow).",
            typeof(TAttribute).FullName);

        return GetTypesInternal(
            typeof(object),
            typeof(TAttribute),
            () => TypeFinder.FindClassesWithAttribute<TAttribute>(specificAssemblies ?? AssembliesToScan),
            "scanning assemblies",
            cache);
    }

    private static string GetName(Type? baseType, Type? attributeType)
    {
        var s = attributeType == null ? string.Empty : "[" + attributeType + "]";
        s += baseType;
        return s;
    }

    private void EnsureInitialized()
    {
        if (_logger == null)
        {
            throw new InvalidOperationException("Cannot get types from a test/blank type loader.");
        }
    }

    private IEnumerable<Type> GetDiscoverableTypes() =>
        GetTypesInternal(
            typeof(IDiscoverable),
            null,
            () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
            "scanning assemblies",
            true);

    /// <summary>
    /// Logs a debug message if the specified condition is true and debug logging is enabled.
    /// </summary>
    /// <param name="condition">The condition that must be true to log.</param>
    /// <param name="message">The log message template.</param>
    /// <param name="args">The message arguments.</param>
    private void LogDebugIf(bool condition, string message, params object?[] args)
    {
        if (condition && IsDebugEnabled)
        {
            _logger.LogDebug(message, args);
        }
    }

    private string CacheStatus(bool cache) => cache ? string.Empty : "non-";

    private IEnumerable<Type> GetTypesInternal(
        Type baseType,
        Type? attributeType,
        Func<IEnumerable<Type>> finder,
        string action,
        bool cache)
    {
        // using an upgradeable lock makes little sense here as only one thread can enter the upgradeable
        // lock at a time, and we don't have non-upgradeable readers, and quite probably the type
        // loader is mostly not going to be used in any kind of massively multi-threaded scenario - so,
        // a plain lock is enough
        lock (_typesLock)
        {
            return GetTypesInternalLocked(baseType, attributeType, finder, action, cache);
        }
    }

    private IEnumerable<Type> GetTypesInternalLocked(
        Type? baseType,
        Type? attributeType,
        Func<IEnumerable<Type>> finder,
        string action,
        bool cache)
    {
        // check if the TypeList already exists, if so return it, if not we'll create it
        Type objectType = typeof(object); // CompositeTypeTypeKey does not support null values
        var listKey = new CompositeTypeTypeKey(baseType ?? objectType, attributeType ?? objectType);

        // need to put some logging here to try to figure out why this is happening: http://issues.umbraco.org/issue/U4-3505
        if (cache && _types.TryGetValue(listKey, out TypeList? cachedList))
        {
            LogDebugIf(true, "Getting {TypeName}: found a cached type list.", GetName(baseType, attributeType));
            return cachedList.Types;
        }

        // else proceed
        var typeList = new TypeList(baseType, attributeType);

        // either we had to scan, or we could not get the types from the cache file - scan now
        LogDebugIf(true, "Getting {TypeName}: " + action + ".", GetName(baseType, attributeType));

        foreach (Type t in finder())
        {
            typeList.Add(t);
        }

        // if we are to cache the results, do so
        if (cache)
        {
            var added = _types.TryAdd(listKey, typeList);
            LogDebugIf(true, "Got {TypeName}, caching ({CacheType}).", GetName(baseType, attributeType), added.ToString().ToLowerInvariant());
        }
        else
        {
            LogDebugIf(true, "Got {TypeName}.", GetName(baseType, attributeType));
        }

        return typeList.Types;
    }

    #endregion

    #region Nested classes and stuff

    /// <summary>
    ///     Represents a list of types obtained by looking for types inheriting/implementing a
    ///     specified type, and/or marked with a specified attribute type.
    /// </summary>
    public sealed class TypeList
    {
        private readonly HashSet<Type> _types = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeList"/> class.
        /// </summary>
        /// <param name="baseType">The base type to filter by.</param>
        /// <param name="attributeType">The attribute type to filter by.</param>
        public TypeList(Type? baseType, Type? attributeType)
        {
            BaseType = baseType;
            AttributeType = attributeType;
        }

        /// <summary>
        /// Gets the base type used for filtering.
        /// </summary>
        public Type? BaseType { get; }

        /// <summary>
        /// Gets the attribute type used for filtering.
        /// </summary>
        public Type? AttributeType { get; }

        /// <summary>
        ///     Gets the types.
        /// </summary>
        public IEnumerable<Type> Types => _types;

        /// <summary>
        ///     Adds a type.
        /// </summary>
        public void Add(Type type)
        {
            if (BaseType?.IsAssignableFrom(type) == false)
            {
                throw new ArgumentException(
                    "Base type " + BaseType + " is not assignable from type " + type + ".",
                    nameof(type));
            }

            _types.Add(type);
        }
    }

    #endregion
}
