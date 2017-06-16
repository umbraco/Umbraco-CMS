using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        private static readonly Lazy<Dictionary<string, UdiType>> UdiTypes;
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
            UdiTypes = new Lazy<Dictionary<string, UdiType>>(() =>
            {
                var result = new Dictionary<string, UdiType>();
                
                // known types:
                foreach (var fi in typeof(Constants.UdiEntityType).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    // IsLiteral determines if its value is written at 
                    //   compile time and not changeable
                    // IsInitOnly determine if the field can be set 
                    //   in the body of the constructor
                    // for C# a field which is readonly keyword would have both true 
                    //   but a const field would have only IsLiteral equal to true
                    if (fi.IsLiteral && fi.IsInitOnly == false)
                    {
                        var udiType = fi.GetCustomAttribute<Constants.UdiTypeAttribute>();

                        if (udiType == null) 
                            throw new InvalidOperationException("All Constants listed in UdiEntityType must be attributed with " + typeof(Constants.UdiTypeAttribute));
                        result[fi.GetValue(null).ToString()] = udiType.UdiType;
                    }                        
                }

                // Scan for unknown UDI types
                // there is no way we can get the "registered" service connectors, as registration
                // happens in Deploy, not in Core, and the Udi class belongs to Core - therefore, we
                // just pick every service connectors - just making sure that not two of them
                // would register the same entity type, with different udi types (would not make
                // much sense anyways).
                var connectors = PluginManager.Current.ResolveTypes<IServiceConnector>();
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

                return result;
            });                       
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

            var entityType = uri.Host;
            UdiType udiType;
            if (UdiTypes.Value.TryGetValue(entityType, out udiType) == false)
            {
                if (tryParse) return false;
                throw new FormatException(string.Format("Unknown entity type \"{0}\".", entityType));
            }
            var path = uri.AbsolutePath.TrimStart('/');
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
            throw new InvalidOperationException("Internal error.");
        }

        private static Udi GetRootUdi(string entityType)
        {
            return RootUdis.GetOrAdd(entityType, x =>
            {
                UdiType udiType;
                if (UdiTypes.Value.TryGetValue(x, out udiType) == false)
                    throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType));
                return udiType == UdiType.StringUdi
                    ? (Udi)new StringUdi(entityType, string.Empty)
                    : new GuidUdi(entityType, Guid.Empty);
            });
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
            if (UdiTypes.Value.TryGetValue(entityType, out udiType) == false)
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
            if (UdiTypes.Value.TryGetValue(entityType, out udiType) == false)
                throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType), "entityType");
            if (udiType != UdiType.GuidUdi)
                throw new InvalidOperationException(string.Format("Entity type \"{0}\" does not have guid udis.", entityType));
            if (id == default(Guid))
                throw new ArgumentException("Cannot be an empty guid.", "id");
            return new GuidUdi(entityType, id);
        }

        internal static Udi Create(Uri uri)
        {
            UdiType udiType;
            if (UdiTypes.Value.TryGetValue(uri.Host, out udiType) == false)
                throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", uri.Host), "uri");
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
