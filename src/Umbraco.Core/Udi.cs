using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Umbraco.Core.Deploy;

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
        private static readonly Lazy<ConcurrentDictionary<string, UdiType>> KnownUdiTypes;
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
            // for non-known Udi types we'll try to parse a GUID and if that doesn't work, we'll decide that it's a string
            KnownUdiTypes = new Lazy<ConcurrentDictionary<string, UdiType>>(() => new ConcurrentDictionary<string, UdiType>(Constants.UdiEntityType.GetTypes()));
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
            ParseInternal(s, false, out udi);
            return udi;
        }

        public static bool TryParse(string s, out Udi udi)
        {
            return ParseInternal(s, true, out udi);
        }

        private static UdiType GetUdiType(Uri uri, out string path)
        {
            path = uri.AbsolutePath.TrimStart('/');

            UdiType udiType;
            if (KnownUdiTypes.Value.TryGetValue(uri.Host, out udiType))
            {
                return udiType;
            }

            // if it's empty and it's not in our known list then we don't know
            if (path.IsNullOrWhiteSpace())
                return UdiType.Unknown;

            // try to parse into a Guid
            // (note: root udis use Guid.Empty so this is ok)
            Guid guidId;
            if (Guid.TryParse(path, out guidId))
            {
                //add it to our known list
                KnownUdiTypes.Value.TryAdd(uri.Host, UdiType.GuidUdi);
                return UdiType.GuidUdi;
            }

            // add it to our known list - if it's not a GUID then it must a string
            KnownUdiTypes.Value.TryAdd(uri.Host, UdiType.StringUdi);
            return UdiType.StringUdi;
        }

        private static bool ParseInternal(string s, bool tryParse, out Udi udi)
        {
            udi = null;
            Uri uri;

            if (Uri.IsWellFormedUriString(s, UriKind.Absolute) == false
                || Uri.TryCreate(s, UriKind.Absolute, out uri) == false)
            {
                if (tryParse) return false;
                throw new FormatException(string.Format("String \"{0}\" is not a valid udi.", s));
            }

            // if it's a known entity type, GetUdiType will return it
            // else it will try to guess based on the path, and register the type as known
            string path;
            var udiType = GetUdiType(uri, out path);

            if (path.IsNullOrWhiteSpace())
            {
                // path is empty which indicates we need to return the root udi
                udi = GetRootUdi(uri.Host);
                return true;
            }

            // if the path is not empty, type should not be unknown
            if (udiType == UdiType.Unknown)
                throw new FormatException(string.Format("Could not determine the Udi type for string \"{0}\".", s));

            if (udiType == UdiType.GuidUdi)
            {
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
                udi = new StringUdi(uri.Host, Uri.UnescapeDataString(path));
                return true;
            }
            if (tryParse) return false;
            throw new InvalidOperationException("Internal error.");
        }

        private static Udi GetRootUdi(string entityType)
        {
            ScanAllUdiTypes();

            return RootUdis.GetOrAdd(entityType, x =>
            {
                UdiType udiType;
                if (KnownUdiTypes.Value.TryGetValue(x, out udiType) == false)
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
        private static void ScanAllUdiTypes()
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
                var connectors = PluginManager.Current.ResolveTypes<IServiceConnector>();
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

                //merge these into the known list
                foreach (var item in result)
                {
                    KnownUdiTypes.Value.TryAdd(item.Key, item.Value);
                }

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
            if (KnownUdiTypes.Value.TryGetValue(entityType, out udiType) && udiType != UdiType.StringUdi)
                throw new InvalidOperationException(string.Format("Entity type \"{0}\" is not a StringUdi.", entityType));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", "id");

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
            if (KnownUdiTypes.Value.TryGetValue(entityType, out udiType) && udiType != UdiType.GuidUdi)
                throw new InvalidOperationException(string.Format("Entity type \"{0}\" is not a GuidUdi.", entityType));

            if (id == default(Guid))
                throw new ArgumentException("Cannot be an empty guid.", "id");
            return new GuidUdi(entityType, id);
        }

        internal static Udi Create(Uri uri)
        {
            // if it's a know type go fast and use ctors
            // else fallback to parsing the string (and guess the type)

            UdiType udiType;
            if (KnownUdiTypes.Value.TryGetValue(uri.Host, out udiType) == false)
                return Parse(uri.ToString());

            if (udiType == UdiType.GuidUdi)
                return new GuidUdi(uri);
            if (udiType == UdiType.GuidUdi)
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
    }

}
