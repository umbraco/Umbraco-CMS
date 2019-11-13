using System;
using System.Collections.Concurrent;

namespace Umbraco.Core
{
    public sealed class UdiParser
    {
        private static readonly ConcurrentDictionary<string, Udi> RootUdis = new ConcurrentDictionary<string, Udi>();
        internal static ConcurrentDictionary<string, UdiType> UdiTypes { get; private set; }

        static UdiParser()
        {
            // initialize with known (built-in) Udi types
            // we will add scanned types later on
            UdiTypes = new ConcurrentDictionary<string, UdiType>(UdiEntityTypeHelper.GetTypes());
        }

        // for tests, totally unsafe
        internal static void ResetUdiTypes()
        {
            UdiTypes = new ConcurrentDictionary<string, UdiType>(UdiEntityTypeHelper.GetTypes());
        }

        /// <summary>
        /// Converts the string representation of an entity identifier into the equivalent Udi instance.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>An Udi instance that contains the value that was parsed.</returns>
        public static Udi Parse(string s)
        {
            ParseInternal(s, false, false, out var udi);
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
            ParseInternal(s, false, knownTypes, out var udi);
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
        /// <param name="udi">An Udi instance that contains the value that was parsed.</param>
        /// <returns>A boolean value indicating whether the string could be parsed.</returns>
        public static bool TryParse<T>(string s, out T udi)
            where T : Udi
        {
            var result = ParseInternal(s, true, false, out var parsed);
            if (result && parsed is T)
            {
                udi = (T)parsed;
                return true;
            }

            udi = null;
            return false;
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
            udi = null;
            Uri uri;

            if (Uri.IsWellFormedUriString(s, UriKind.Absolute) == false
                || Uri.TryCreate(s, UriKind.Absolute, out uri) == false)
            {
                if (tryParse) return false;
                throw new FormatException(string.Format("String \"{0}\" is not a valid udi.", s));
            }

            var entityType = uri.Host;
            if (UdiTypes.TryGetValue(entityType, out var udiType) == false)
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

            var path = uri.AbsolutePath.TrimStart('/');

            if (udiType == UdiType.GuidUdi)
            {
                if (path == string.Empty)
                {
                    udi = GetRootUdi(uri.Host);
                    return true;
                }
                if (Guid.TryParse(path, out var guid) == false)
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

        internal static Udi GetRootUdi(string entityType)
        {
            return RootUdis.GetOrAdd(entityType, x =>
            {
                if (UdiTypes.TryGetValue(x, out var udiType) == false)
                    throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType));
                return udiType == UdiType.StringUdi
                    ? (Udi)new StringUdi(entityType, string.Empty)
                    : new GuidUdi(entityType, Guid.Empty);
            });
        }

        

        /// <summary>
        /// Registers a custom entity type.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="udiType"></param>
        public static void RegisterUdiType(string entityType, UdiType udiType) => UdiTypes.TryAdd(entityType, udiType);
    }
}
