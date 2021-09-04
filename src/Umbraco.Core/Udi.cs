using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Umbraco.Core.Deploy;
using Umbraco.Core.Composing;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents an entity identifier.
    /// </summary>
    /// <remarks>An Udi can be fully qualified or "closed" eg umb://document/{guid} or "open" eg umb://document.</remarks>
    [TypeConverter(typeof(UdiTypeConverter))]
    public abstract class Udi : IComparable<Udi>
    {
        // notes - see U4-10409
        // if this class is used during application pre-start it cannot scans the assemblies,
        // this is addressed by lazily-scanning, with the following caveats:
        // - parsing a root udi still requires a scan and therefore still breaks
        // - parsing an invalid udi ("umb://should-be-guid/<not-a-guid>") corrupts KnowUdiTypes

        private static volatile bool _scanned;
        private static readonly object ScanLocker = new object();
        private static ConcurrentDictionary<string, UdiType> _udiTypes;
        private static readonly ConcurrentDictionary<string, Udi> RootUdis = new ConcurrentDictionary<string, Udi>();
        internal readonly Uri UriValue; // internal for UdiRange

        /// <summary>
        /// Initializes a new instance of the Udi class.
        /// </summary>
        /// <param name="entityType">The entity type part of the identifier.</param>
        /// <param name="stringValue">The string value of the identifier.</param>
        protected Udi(string entityType, string stringValue)
        {
            EntityType = entityType;
            UriValue = new Uri(stringValue);
        }

        /// <summary>
        /// Initializes a new instance of the Udi class.
        /// </summary>
        /// <param name="uriValue">The uri value of the identifier.</param>
        protected Udi(Uri uriValue)
        {
            EntityType = uriValue.Host;
            UriValue = uriValue;
        }

        static Udi()
        {
            // initialize with known (built-in) Udi types
            // we will add scanned types later on
            _udiTypes = new ConcurrentDictionary<string, UdiType>(Constants.UdiEntityType.GetTypes());
        }

        // for tests, totally unsafe
        internal static void ResetUdiTypes()
        {
            _udiTypes = new ConcurrentDictionary<string, UdiType>(Constants.UdiEntityType.GetTypes());
            _scanned = false;
        }

        /// <summary>
        /// Gets the entity type part of the identifier.
        /// </summary>
        public string EntityType { get; private set; }

        public int CompareTo(Udi other)
        {
            return string.Compare(UriValue.ToString(), other.UriValue.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public override string ToString()
        {
            // UriValue is created in the ctor and is never null
            // use AbsoluteUri here and not ToString else it's not encoded!
            return UriValue.AbsoluteUri;
        }

        /// <summary>
        /// Converts the string representation of an entity identifier into the equivalent Udi instance.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>An Udi instance that contains the value that was parsed.</returns>
        public static Udi Parse(string s)
        {
            Udi udi;
            ParseInternal(s, false, false, out udi);
            return udi;
        }

        /// <summary>
        /// Converts the string representation of an entity identifier into the equivalent Udi instance.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <param name="knownTypes">A value indicating whether to only deal with known types.</param>
        /// <returns>An Udi instance that contains the value that was parsed.</returns>
        /// <remarks>
        /// <para>If <paramref name="knownTypes"/> is <c>true</c>, and the string could not be parsed because
        /// the entity type was not known, the method succeeds but sets <c>udi</c>to an
        /// <see cref="UnknownTypeUdi"/> value.</para>
        /// <para>If <paramref name="knownTypes"/> is <c>true</c>, assemblies are not scanned for types,
        /// and therefore only builtin types may be known. Unless scanning already took place.</para>
        /// </remarks>
        public static Udi Parse(string s, bool knownTypes)
        {
            Udi udi;
            ParseInternal(s, false, knownTypes, out udi);
            return udi;
        }

        /// <summary>
        /// Converts the string representation of an entity identifier into the equivalent Udi instance.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <param name="udi">An Udi instance that contains the value that was parsed.</param>
        /// <returns>A boolean value indicating whether the string could be parsed.</returns>
        public static bool TryParse(string s, out Udi udi)
        {
            return ParseInternal(s, true, false, out udi);
        }

        /// <summary>
        /// Converts the string representation of an entity identifier into the equivalent Udi instance.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <param name="knownTypes">A value indicating whether to only deal with known types.</param>
        /// <param name="udi">An Udi instance that contains the value that was parsed.</param>
        /// <returns>A boolean value indicating whether the string could be parsed.</returns>
        /// <remarks>
        /// <para>If <paramref name="knownTypes"/> is <c>true</c>, and the string could not be parsed because
        /// the entity type was not known, the method returns <c>false</c> but still sets <c>udi</c>
        /// to an <see cref="UnknownTypeUdi"/> value.</para>
        /// <para>If <paramref name="knownTypes"/> is <c>true</c>, assemblies are not scanned for types,
        /// and therefore only builtin types may be known. Unless scanning already took place.</para>
        /// </remarks>
        public static bool TryParse(string s, bool knownTypes, out Udi udi)
        {
            return ParseInternal(s, true, knownTypes, out udi);
        }

        private static bool ParseInternal(string s, bool tryParse, bool knownTypes, out Udi udi)
        {
            if (knownTypes == false)
                EnsureScanForUdiTypes();

            udi = null;
            Uri uri;

            if (Uri.IsWellFormedUriString(s, UriKind.Absolute) == false
                || Uri.TryCreate(s, UriKind.Absolute, out uri) == false)
            {
                if (tryParse) return false;
                throw new FormatException(string.Format("String \"{0}\" is not a valid udi.", s));
            }

            var entityType = uri.Host;
            UdiType udiType;
            if (_udiTypes.TryGetValue(entityType, out udiType) == false)
            {
                if (knownTypes)
                {
                    // not knowing the type is not an error
                    // just return the unknown type udi
                    udi = UnknownTypeUdi.Instance;
                    return false;
                }
                if (tryParse) return false;
                throw new FormatException(string.Format("Unknown entity type \"{0}\".", entityType));
            }

            var path = uri.AbsolutePath.TrimStart(Constants.CharArrays.ForwardSlash);

            if (udiType == UdiType.GuidUdi)
            {
                if (path == string.Empty)
                {
                    udi = GetRootUdi(uri.Host);
                    return true;
                }
                Guid guid;
                if (Guid.TryParse(path, out guid) == false)
                {
                    if (tryParse) return false;
                    throw new FormatException(string.Format("String \"{0}\" is not a valid udi.", s));
                }
                udi = new GuidUdi(uri.Host, guid);
                return true;
            }

            if (udiType == UdiType.StringUdi)
            {
                udi = path == string.Empty ? GetRootUdi(uri.Host) : new StringUdi(uri.Host, Uri.UnescapeDataString(path));
                return true;
            }

            if (tryParse) return false;
            throw new InvalidOperationException(string.Format("Invalid udi type \"{0}\".", udiType));
        }

        private static Udi GetRootUdi(string entityType)
        {
            EnsureScanForUdiTypes();

            return RootUdis.GetOrAdd(entityType, x =>
            {
                UdiType udiType;
                if (_udiTypes.TryGetValue(x, out udiType) == false)
                    throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType));
                return udiType == UdiType.StringUdi
                    ? (Udi)new StringUdi(entityType, string.Empty)
                    : new GuidUdi(entityType, Guid.Empty);
            });
        }

        /// <summary>
        /// When required scan assemblies for known UDI types based on <see cref="IServiceConnector"/> instances
        /// </summary>
        /// <remarks>
        /// This is only required when needing to resolve root udis
        /// </remarks>
        private static void EnsureScanForUdiTypes()
        {
            if (_scanned) return;

            lock (ScanLocker)
            {
                // Scan for unknown UDI types
                // there is no way we can get the "registered" service connectors, as registration
                // happens in Deploy, not in Core, and the Udi class belongs to Core - therefore, we
                // just pick every service connectors - just making sure that not two of them
                // would register the same entity type, with different udi types (would not make
                // much sense anyways).
                var connectors = Current.HasFactory ? (Current.TypeLoader?.GetTypes<IServiceConnector>() ?? Enumerable.Empty<Type>()) : Enumerable.Empty<Type>();
                var result = new Dictionary<string, UdiType>();
                foreach (var connector in connectors)
                {
                    var attrs = connector.GetCustomAttributes<UdiDefinitionAttribute>(false);
                    foreach (var attr in attrs)
                    {
                        UdiType udiType;
                        if (result.TryGetValue(attr.EntityType, out udiType) && udiType != attr.UdiType)
                            throw new Exception(string.Format("Entity type \"{0}\" is declared by more than one IServiceConnector, with different UdiTypes.", attr.EntityType));
                        result[attr.EntityType] = attr.UdiType;
                    }
                }

                // merge these into the known list
                foreach (var item in result)
                    _udiTypes.TryAdd(item.Key, item.Value);

                _scanned = true;
            }
        }

        /// <summary>
        /// Creates a root Udi for an entity type.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>The root Udi for the entity type.</returns>
        public static Udi Create(string entityType)
        {
            return GetRootUdi(entityType);
        }

        /// <summary>
        /// Creates a string Udi.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The string Udi for the entity type and identifier.</returns>
        public static Udi Create(string entityType, string id)
        {
            UdiType udiType;
            if (_udiTypes.TryGetValue(entityType, out udiType) == false)
                throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType), "entityType");

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", "id");
            if (udiType != UdiType.StringUdi)
                throw new InvalidOperationException(string.Format("Entity type \"{0}\" does not have string udis.", entityType));

            return new StringUdi(entityType, id);
        }

        /// <summary>
        /// Creates a Guid Udi.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>The Guid Udi for the entity type and identifier.</returns>
        public static Udi Create(string entityType, Guid id)
        {
            UdiType udiType;
            if (_udiTypes.TryGetValue(entityType, out udiType) == false)
                throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType), "entityType");

            if (udiType != UdiType.GuidUdi)
                throw new InvalidOperationException(string.Format("Entity type \"{0}\" does not have guid udis.", entityType));
            if (id == default(Guid))
                throw new ArgumentException("Cannot be an empty guid.", "id");

            return new GuidUdi(entityType, id);
        }

        internal static Udi Create(Uri uri)
        {
            // if it's a know type go fast and use ctors
            // else fallback to parsing the string (and guess the type)

            UdiType udiType;
            if (_udiTypes.TryGetValue(uri.Host, out udiType) == false)
                throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", uri.Host), "uri");

            if (udiType == UdiType.GuidUdi)
                return new GuidUdi(uri);
            if (udiType == UdiType.StringUdi)
                return new StringUdi(uri);

            throw new ArgumentException(string.Format("Uri \"{0}\" is not a valid udi.", uri));
        }

        public void EnsureType(params string[] validTypes)
        {
            if (validTypes.Contains(EntityType) == false)
                throw new Exception(string.Format("Unexpected entity type \"{0}\".", EntityType));
        }

        /// <summary>
        /// Gets a value indicating whether this Udi is a root Udi.
        /// </summary>
        /// <remarks>A root Udi points to the "root of all things" for a given entity type, e.g. the content tree root.</remarks>
        public abstract bool IsRoot { get; }

        /// <summary>
        /// Ensures that this Udi is not a root Udi.
        /// </summary>
        /// <returns>This Udi.</returns>
        /// <exception cref="Exception">When this Udi is a Root Udi.</exception>
        public Udi EnsureNotRoot()
        {
            if (IsRoot) throw new Exception("Root Udi.");
            return this;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Udi;
            return other != null && GetType() == other.GetType() && UriValue == other.UriValue;
        }

        public override int GetHashCode()
        {
            return UriValue.GetHashCode();
        }

        public static bool operator ==(Udi udi1, Udi udi2)
        {
            if (ReferenceEquals(udi1, udi2)) return true;
            if ((object)udi1 == null || (object)udi2 == null) return false;
            return udi1.Equals(udi2);
        }

        public static bool operator !=(Udi udi1, Udi udi2)
        {
            return (udi1 == udi2) == false;
        }

        internal class UnknownTypeUdi : Udi
        {
            private UnknownTypeUdi()
                : base("unknown", "umb://unknown/")
            { }

            public static readonly UnknownTypeUdi Instance = new UnknownTypeUdi();

            public override bool IsRoot
            {
                get { return false; }
            }
        }
    }
}
