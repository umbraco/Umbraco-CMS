using System;
using System.Collections.Concurrent;
using System.Reflection;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Provides utilities and extension methods to handle object types.
    /// </summary>
    public static class ObjectTypes
    {
        // must be concurrent to avoid thread collisions!
        private static readonly ConcurrentDictionary<UmbracoObjectTypes, Guid> UmbracoGuids = new ConcurrentDictionary<UmbracoObjectTypes, Guid>();
        private static readonly ConcurrentDictionary<UmbracoObjectTypes, string> UmbracoUdiTypes = new ConcurrentDictionary<UmbracoObjectTypes, string>();
        private static readonly ConcurrentDictionary<UmbracoObjectTypes, string> UmbracoFriendlyNames = new ConcurrentDictionary<UmbracoObjectTypes, string>();
        private static readonly ConcurrentDictionary<UmbracoObjectTypes, Type> UmbracoTypes = new ConcurrentDictionary<UmbracoObjectTypes, Type>();
        private static readonly ConcurrentDictionary<Guid, string> GuidUdiTypes = new ConcurrentDictionary<Guid, string>();
        private static readonly ConcurrentDictionary<Guid, UmbracoObjectTypes> GuidObjectTypes = new ConcurrentDictionary<Guid, UmbracoObjectTypes>();
        private static readonly ConcurrentDictionary<Guid, Type> GuidTypes = new ConcurrentDictionary<Guid, Type>();

        private static FieldInfo GetEnumField(string name)
        {
            return typeof (UmbracoObjectTypes).GetField(name, BindingFlags.Public | BindingFlags.Static);
        }

        private static FieldInfo GetEnumField(Guid guid)
        {
            var fields = typeof (UmbracoObjectTypes).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);
                if (attribute != null && attribute.ObjectId == guid) return field;
            }

            return null;
        }

        /// <summary>
        /// Gets the Umbraco object type corresponding to a name.
        /// </summary>
        public static UmbracoObjectTypes GetUmbracoObjectType(string name)
        {
            return (UmbracoObjectTypes) Enum.Parse(typeof (UmbracoObjectTypes), name, false);
        }

        #region Guid object type utilities

        /// <summary>
        /// Gets the Umbraco object type corresponding to an object type Guid.
        /// </summary>
        public static UmbracoObjectTypes GetUmbracoObjectType(Guid objectType)
        {
            return GuidObjectTypes.GetOrAdd(objectType, t =>
            {
                var field = GetEnumField(objectType);
                if (field == null) return UmbracoObjectTypes.Unknown;

                return (UmbracoObjectTypes) field.GetValue(null);
            });
        }

        /// <summary>
        /// Gets the Udi type corresponding to an object type Guid.
        /// </summary>
        public static string GetUdiType(Guid objectType)
        {
            return GuidUdiTypes.GetOrAdd(objectType, t =>
            {
                var field = GetEnumField(objectType);
                if (field == null) return Constants.UdiEntityType.Unknown;

                var attribute = field.GetCustomAttribute<UmbracoUdiTypeAttribute>(false);
                return attribute?.UdiType ?? Constants.UdiEntityType.Unknown;
            });
        }

        /// <summary>
        /// Gets the CLR type corresponding to an object type Guid.
        /// </summary>
        public static Type GetClrType(Guid objectType)
        {
            return GuidTypes.GetOrAdd(objectType, t =>
            {
                var field = GetEnumField(objectType);
                if (field == null) return null;

                var attribute = field.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);
                return attribute?.ModelType;
            });
        }

        #endregion

        #region UmbracoObjectTypes extension methods

        /// <summary>
        /// Gets the object type Guid corresponding to this Umbraco object type.
        /// </summary>
        public static Guid GetGuid(this UmbracoObjectTypes objectType)
        {
            return UmbracoGuids.GetOrAdd(objectType, t =>
            {
                var field = GetEnumField(t.ToString());
                var attribute = field.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);

                return attribute?.ObjectId ?? Guid.Empty;
            });
        }

        /// <summary>
        /// Gets the Udi type corresponding to this Umbraco object type.
        /// </summary>
        public static string GetUdiType(this UmbracoObjectTypes objectType)
        {
            return UmbracoUdiTypes.GetOrAdd(objectType, t =>
            {
                var field = GetEnumField(t.ToString());
                var attribute = field.GetCustomAttribute<UmbracoUdiTypeAttribute>(false);

                return attribute?.UdiType ?? Constants.UdiEntityType.Unknown;
            });
        }

        /// <summary>
        /// Gets the name corresponding to this Umbraco object type.
        /// </summary>
        public static string GetName(this UmbracoObjectTypes objectType)
        {
            return Enum.GetName(typeof (UmbracoObjectTypes), objectType);
        }

        /// <summary>
        /// Gets the friendly name corresponding to this Umbraco object type.
        /// </summary>
        public static string GetFriendlyName(this UmbracoObjectTypes objectType)
        {
            return UmbracoFriendlyNames.GetOrAdd(objectType, t =>
            {
                var field = GetEnumField(t.ToString());
                var attribute = field.GetCustomAttribute<FriendlyNameAttribute>(false);

                return attribute?.ToString() ?? string.Empty;
            });
        }

        /// <summary>
        /// Gets the CLR type corresponding to this Umbraco object type.
        /// </summary>
        public static Type GetClrType(this UmbracoObjectTypes objectType)
        {
            return UmbracoTypes.GetOrAdd(objectType, t =>
            {
                var field = GetEnumField(t.ToString());
                var attribute = field.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);

                return attribute?.ModelType;
            });
        }

        #endregion
    }
}
