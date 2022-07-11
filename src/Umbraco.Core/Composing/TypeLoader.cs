using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Logging;
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
    private readonly object _locko = new();
    private readonly ILogger<TypeLoader> _logger;

    private readonly Dictionary<CompositeTypeTypeKey, TypeList> _types = new();

    private IEnumerable<Assembly>? _assemblies;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TypeLoader" /> class.
    /// </summary>
    [Obsolete("Please use an alternative constructor.")]
    public TypeLoader(
        ITypeFinder typeFinder,
        IRuntimeHash runtimeHash,
        IAppPolicyCache runtimeCache,
        DirectoryInfo localTempPath,
        ILogger<TypeLoader> logger,
        IProfiler profiler,
        IEnumerable<Assembly>? assembliesToScan = null)
        : this(typeFinder, logger, assembliesToScan)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TypeLoader" /> class.
    /// </summary>
    [Obsolete("Please use an alternative constructor.")]
    public TypeLoader(
        ITypeFinder typeFinder,
        IRuntimeHash runtimeHash,
        IAppPolicyCache runtimeCache,
        DirectoryInfo localTempPath,
        ILogger<TypeLoader> logger,
        IProfiler profiler,
        bool detectChanges,
        IEnumerable<Assembly>? assembliesToScan = null)
        : this(typeFinder, logger, assembliesToScan)
    {
    }

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

    /// <summary>
    ///     Sets a type list.
    /// </summary>
    /// <remarks>For unit tests.</remarks>
    // internal for tests
    [Obsolete("This will be removed in a future version.")]
    public void AddTypeList(TypeList typeList)
    {
        Type tobject = typeof(object); // CompositeTypeTypeKey does not support null values
        _types[new CompositeTypeTypeKey(typeList.BaseType ?? tobject, typeList.AttributeType ?? tobject)] = typeList;
    }

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

    // internal for tests
    [Obsolete("This will be removed in a future version.")]
    public Attempt<IEnumerable<string>> TryGetCached(Type baseType, Type attributeType) =>
        Attempt<IEnumerable<string>>.Fail();

    // internal for tests
    [Obsolete("This will be removed in a future version.")]
    public Dictionary<(string, string), IEnumerable<string>>? ReadCache() => null;

    // internal for tests
    [Obsolete("This will be removed in a future version.")]
    public string? GetTypesListFilePath() => null;

    // internal for tests
    [Obsolete("This will be removed in a future version.")]
    public void WriteCache()
    {
    }

    /// <summary>
    ///     Clears cache.
    /// </summary>
    /// <remarks>Generally only used for resetting cache, for example during the install process.</remarks>
    [Obsolete("This will be removed in a future version.")]
    public void ClearTypesCache()
    {
    }

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
        if (_logger == null)
        {
            throw new InvalidOperationException("Cannot get types from a test/blank type loader.");
        }

        // do not cache anything from specific assemblies
        cache &= specificAssemblies == null;

        // if not IDiscoverable, directly get types
        if (!typeof(IDiscoverable).IsAssignableFrom(typeof(T)))
        {
            // warn
            _logger.LogDebug(
                "Running a full, " + (cache ? string.Empty : "non-") +
                "cached, scan for non-discoverable type {TypeName} (slow).",
                typeof(T).FullName);

            return GetTypesInternal(
                typeof(T),
                null,
                () => TypeFinder.FindClassesOfType<T>(specificAssemblies ?? AssembliesToScan),
                "scanning assemblies",
                cache);
        }

        // get IDiscoverable and always cache
        IEnumerable<Type> discovered = GetTypesInternal(
            typeof(IDiscoverable),
            null,
            () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
            "scanning assemblies",
            true);

        // warn
        if (!cache)
        {
            _logger.LogDebug(
                "Running a non-cached, filter for discoverable type {TypeName} (slowish).",
                typeof(T).FullName);
        }

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
        if (_logger == null)
        {
            throw new InvalidOperationException("Cannot get types from a test/blank type loader.");
        }

        // do not cache anything from specific assemblies
        cache &= specificAssemblies == null;

        // if not IDiscoverable, directly get types
        if (!typeof(IDiscoverable).IsAssignableFrom(typeof(T)))
        {
            _logger.LogDebug(
                "Running a full, " + (cache ? string.Empty : "non-") +
                "cached, scan for non-discoverable type {TypeName} / attribute {AttributeName} (slow).",
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
        IEnumerable<Type> discovered = GetTypesInternal(
            typeof(IDiscoverable),
            null,
            () => TypeFinder.FindClassesOfType<IDiscoverable>(AssembliesToScan),
            "scanning assemblies",
            true);

        // warn
        if (!cache)
        {
            _logger.LogDebug(
                "Running a non-cached, filter for discoverable type {TypeName}  / attribute {AttributeName} (slowish).",
                typeof(T).FullName,
                typeof(TAttribute).FullName);
        }

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
        if (_logger == null)
        {
            throw new InvalidOperationException("Cannot get types from a test/blank type loader.");
        }

        // do not cache anything from specific assemblies
        cache &= specificAssemblies == null;

        if (!cache)
        {
            _logger.LogDebug(
                "Running a full, non-cached, scan for types / attribute {AttributeName} (slow).",
                typeof(TAttribute).FullName);
        }

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
        lock (_locko)
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
        Type tobject = typeof(object); // CompositeTypeTypeKey does not support null values
        var listKey = new CompositeTypeTypeKey(baseType ?? tobject, attributeType ?? tobject);
        TypeList? typeList = null;

        if (cache)
        {
            _types.TryGetValue(listKey, out typeList); // else null
        }

        // if caching and found, return
        if (typeList != null)
        {
            // need to put some logging here to try to figure out why this is happening: http://issues.umbraco.org/issue/U4-3505
            _logger.LogDebug("Getting {TypeName}: found a cached type list.", GetName(baseType, attributeType));
            return typeList.Types;
        }

        // else proceed,
        typeList = new TypeList(baseType, attributeType);

        // either we had to scan, or we could not get the types from the cache file - scan now
        _logger.LogDebug("Getting {TypeName}: " + action + ".", GetName(baseType, attributeType));

        foreach (Type t in finder())
        {
            typeList.Add(t);
        }

        // if we are to cache the results, do so
        if (cache)
        {
            var added = _types.ContainsKey(listKey) == false;
            if (added)
            {
                _types[listKey] = typeList;
            }

            _logger.LogDebug("Got {TypeName}, caching ({CacheType}).", GetName(baseType, attributeType), added.ToString().ToLowerInvariant());
        }
        else
        {
            _logger.LogDebug("Got {TypeName}.", GetName(baseType, attributeType));
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

        public TypeList(Type? baseType, Type? attributeType)
        {
            BaseType = baseType;
            AttributeType = attributeType;
        }

        public Type? BaseType { get; }

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

    /// <summary>
    ///     Represents the error that occurs when a type was not found in the cache type list with the specified
    ///     TypeResolutionKind.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    internal class CachedTypeNotFoundInFileException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
        /// </summary>
        public CachedTypeNotFoundInFileException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CachedTypeNotFoundInFileException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        ///     The exception that is the cause of the current exception, or a null reference (
        ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
        /// </param>
        public CachedTypeNotFoundInFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CachedTypeNotFoundInFileException" /> class.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object
        ///     data about the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual
        ///     information about the source or destination.
        /// </param>
        protected CachedTypeNotFoundInFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion
}
