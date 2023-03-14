using System.Collections.Concurrent;
using System.Reflection;
using Umbraco.Cms.Core.CodeAnnotations;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Provides utilities and extension methods to handle object types.
/// </summary>
public static class ObjectTypes
{
    // must be concurrent to avoid thread collisions!
    private static readonly ConcurrentDictionary<UmbracoObjectTypes, Guid> UmbracoGuids = new();
    private static readonly ConcurrentDictionary<UmbracoObjectTypes, string> UmbracoUdiTypes = new();
    private static readonly ConcurrentDictionary<UmbracoObjectTypes, string> UmbracoFriendlyNames = new();
    private static readonly ConcurrentDictionary<UmbracoObjectTypes, Type?> UmbracoTypes = new();
    private static readonly ConcurrentDictionary<Guid, string> GuidUdiTypes = new();
    private static readonly ConcurrentDictionary<Guid, UmbracoObjectTypes> GuidObjectTypes = new();
    private static readonly ConcurrentDictionary<Guid, Type?> GuidTypes = new();

    /// <summary>
    ///     Gets the Umbraco object type corresponding to a name.
    /// </summary>
    public static UmbracoObjectTypes GetUmbracoObjectType(string name) =>
        (UmbracoObjectTypes)Enum.Parse(typeof(UmbracoObjectTypes), name, true);

    private static FieldInfo? GetEnumField(string name) =>
        typeof(UmbracoObjectTypes).GetField(name, BindingFlags.Public | BindingFlags.Static);

    private static FieldInfo? GetEnumField(Guid guid)
    {
        FieldInfo[] fields = typeof(UmbracoObjectTypes).GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (FieldInfo field in fields)
        {
            UmbracoObjectTypeAttribute? attribute = field.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);
            if (attribute != null && attribute.ObjectId == guid)
            {
                return field;
            }
        }

        return null;
    }

    #region Guid object type utilities

    /// <summary>
    ///     Gets the Umbraco object type corresponding to an object type Guid.
    /// </summary>
    public static UmbracoObjectTypes GetUmbracoObjectType(Guid objectType) =>
        GuidObjectTypes.GetOrAdd(objectType, t =>
        {
            FieldInfo? field = GetEnumField(objectType);
            if (field == null)
            {
                return UmbracoObjectTypes.Unknown;
            }

            return (UmbracoObjectTypes?)field.GetValue(null) ?? UmbracoObjectTypes.Unknown;
        });

    /// <summary>
    ///     Gets the Udi type corresponding to an object type Guid.
    /// </summary>
    public static string GetUdiType(Guid objectType) =>
        GuidUdiTypes.GetOrAdd(objectType, t =>
        {
            FieldInfo? field = GetEnumField(objectType);
            if (field == null)
            {
                return Constants.UdiEntityType.Unknown;
            }

            UmbracoUdiTypeAttribute? attribute = field.GetCustomAttribute<UmbracoUdiTypeAttribute>(false);
            return attribute?.UdiType ?? Constants.UdiEntityType.Unknown;
        });

    /// <summary>
    ///     Gets the CLR type corresponding to an object type Guid.
    /// </summary>
    public static Type? GetClrType(Guid objectType) =>
        GuidTypes.GetOrAdd(objectType, t =>
        {
            FieldInfo? field = GetEnumField(objectType);
            if (field == null)
            {
                return null;
            }

            UmbracoObjectTypeAttribute? attribute = field.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);
            return attribute?.ModelType;
        });

    #endregion

    #region UmbracoObjectTypes extension methods

    /// <summary>
    ///     Gets the object type Guid corresponding to this Umbraco object type.
    /// </summary>
    public static Guid GetGuid(this UmbracoObjectTypes objectType) =>
        UmbracoGuids.GetOrAdd(objectType, t =>
        {
            FieldInfo? field = GetEnumField(t.ToString());
            UmbracoObjectTypeAttribute? attribute = field?.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);

            return attribute?.ObjectId ?? Guid.Empty;
        });

    /// <summary>
    ///     Gets the Udi type corresponding to this Umbraco object type.
    /// </summary>
    public static string GetUdiType(this UmbracoObjectTypes objectType) =>
        UmbracoUdiTypes.GetOrAdd(objectType, t =>
        {
            FieldInfo? field = GetEnumField(t.ToString());
            UmbracoUdiTypeAttribute? attribute = field?.GetCustomAttribute<UmbracoUdiTypeAttribute>(false);

            return attribute?.UdiType ?? Constants.UdiEntityType.Unknown;
        });

    /// <summary>
    ///     Gets the name corresponding to this Umbraco object type.
    /// </summary>
    public static string? GetName(this UmbracoObjectTypes objectType) =>
        Enum.GetName(typeof(UmbracoObjectTypes), objectType);

    /// <summary>
    ///     Gets the friendly name corresponding to this Umbraco object type.
    /// </summary>
    public static string GetFriendlyName(this UmbracoObjectTypes objectType) =>
        UmbracoFriendlyNames.GetOrAdd(objectType, t =>
        {
            FieldInfo? field = GetEnumField(t.ToString());
            FriendlyNameAttribute? attribute = field?.GetCustomAttribute<FriendlyNameAttribute>(false);

            return attribute?.ToString() ?? string.Empty;
        });

    /// <summary>
    ///     Gets the CLR type corresponding to this Umbraco object type.
    /// </summary>
    public static Type? GetClrType(this UmbracoObjectTypes objectType) =>
        UmbracoTypes.GetOrAdd(objectType, t =>
        {
            FieldInfo? field = GetEnumField(t.ToString());
            UmbracoObjectTypeAttribute? attribute = field?.GetCustomAttribute<UmbracoObjectTypeAttribute>(false);

            return attribute?.ModelType;
        });

    #endregion
}
