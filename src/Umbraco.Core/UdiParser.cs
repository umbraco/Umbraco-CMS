using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Core;

public sealed class UdiParser
{
    private static readonly ConcurrentDictionary<string, Udi> RootUdis = new();

    static UdiParser() =>

        // initialize with known (built-in) Udi types
        // we will add scanned types later on
        UdiTypes = new ConcurrentDictionary<string, UdiType>(GetKnownUdiTypes());

    internal static ConcurrentDictionary<string, UdiType> UdiTypes { get; private set; }

    /// <summary>
    ///     Internal API for tests to resets all udi types back to only the known udi types.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void ResetUdiTypes() => UdiTypes = new ConcurrentDictionary<string, UdiType>(GetKnownUdiTypes());

    /// <summary>
    ///     Converts the string representation of an entity identifier into the equivalent Udi instance.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>An Udi instance that contains the value that was parsed.</returns>
    public static Udi Parse(string s)
    {
        ParseInternal(s, false, false, out Udi? udi);
        return udi!;
    }

    /// <summary>
    ///     Converts the string representation of an entity identifier into the equivalent Udi instance.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <param name="knownTypes">A value indicating whether to only deal with known types.</param>
    /// <returns>An Udi instance that contains the value that was parsed.</returns>
    /// <remarks>
    ///     <para>
    ///         If <paramref name="knownTypes" /> is <c>true</c>, and the string could not be parsed because
    ///         the entity type was not known, the method succeeds but sets <c>udi</c>to an
    ///         <see cref="UnknownTypeUdi" /> value.
    ///     </para>
    ///     <para>
    ///         If <paramref name="knownTypes" /> is <c>true</c>, assemblies are not scanned for types,
    ///         and therefore only builtin types may be known. Unless scanning already took place.
    ///     </para>
    /// </remarks>
    public static Udi Parse(string s, bool knownTypes)
    {
        ParseInternal(s, false, knownTypes, out Udi? udi);
        return udi!;
    }

    /// <summary>
    ///     Converts the string representation of an entity identifier into the equivalent Udi instance.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <param name="udi">An Udi instance that contains the value that was parsed.</param>
    /// <returns>A boolean value indicating whether the string could be parsed.</returns>
    public static bool TryParse(string s, [MaybeNullWhen(false)] out Udi udi) => ParseInternal(s, true, false, out udi);

    /// <summary>
    ///     Converts the string representation of an entity identifier into the equivalent Udi instance.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <param name="udi">An Udi instance that contains the value that was parsed.</param>
    /// <returns>A boolean value indicating whether the string could be parsed.</returns>
    public static bool TryParse<T>(string? s, [MaybeNullWhen(false)] out T udi)
        where T : Udi?
    {
        var result = ParseInternal(s, true, false, out Udi? parsed);
        if (result && parsed is T)
        {
            udi = (T)parsed;
            return true;
        }

        udi = null;
        return false;
    }

    /// <summary>
    ///     Converts the string representation of an entity identifier into the equivalent Udi instance.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <param name="knownTypes">A value indicating whether to only deal with known types.</param>
    /// <param name="udi">An Udi instance that contains the value that was parsed.</param>
    /// <returns>A boolean value indicating whether the string could be parsed.</returns>
    /// <remarks>
    ///     <para>
    ///         If <paramref name="knownTypes" /> is <c>true</c>, and the string could not be parsed because
    ///         the entity type was not known, the method returns <c>false</c> but still sets <c>udi</c>
    ///         to an <see cref="UnknownTypeUdi" /> value.
    ///     </para>
    ///     <para>
    ///         If <paramref name="knownTypes" /> is <c>true</c>, assemblies are not scanned for types,
    ///         and therefore only builtin types may be known. Unless scanning already took place.
    ///     </para>
    /// </remarks>
    public static bool TryParse(string? s, bool knownTypes, [MaybeNullWhen(false)] out Udi udi) =>
        ParseInternal(s, true, knownTypes, out udi);

    /// <summary>
    ///     Registers a custom entity type.
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="udiType"></param>
    public static void RegisterUdiType(string entityType, UdiType udiType) => UdiTypes.TryAdd(entityType, udiType);

    internal static Udi GetRootUdi(string entityType) =>
        RootUdis.GetOrAdd(entityType, x =>
        {
            if (UdiTypes.TryGetValue(x, out UdiType udiType) == false)
            {
                throw new ArgumentException(string.Format("Unknown entity type \"{0}\".", entityType));
            }

            return udiType == UdiType.StringUdi
                ? new StringUdi(entityType, string.Empty)
                : new GuidUdi(entityType, Guid.Empty);
        });

    private static bool ParseInternal(string? s, bool tryParse, bool knownTypes, [MaybeNullWhen(false)] out Udi udi)
    {
        udi = null;
        if (Uri.IsWellFormedUriString(s, UriKind.Absolute) == false
            || Uri.TryCreate(s, UriKind.Absolute, out Uri? uri) == false)
        {
            if (tryParse)
            {
                return false;
            }

            throw new FormatException(string.Format("String \"{0}\" is not a valid udi.", s));
        }

        var entityType = uri.Host;
        if (UdiTypes.TryGetValue(entityType, out UdiType udiType) == false)
        {
            if (knownTypes)
            {
                // not knowing the type is not an error
                // just return the unknown type udi
                udi = UnknownTypeUdi.Instance;
                return false;
            }

            if (tryParse)
            {
                return false;
            }

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

            if (Guid.TryParse(path, out Guid guid) == false)
            {
                if (tryParse)
                {
                    return false;
                }

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

        if (tryParse)
        {
            return false;
        }

        throw new InvalidOperationException(string.Format("Invalid udi type \"{0}\".", udiType));
    }

    public static Dictionary<string, UdiType> GetKnownUdiTypes() =>
        new()
        {
            { Constants.UdiEntityType.Unknown, UdiType.Unknown },
            { Constants.UdiEntityType.AnyGuid, UdiType.GuidUdi },
            { Constants.UdiEntityType.Element, UdiType.GuidUdi },
            { Constants.UdiEntityType.Document, UdiType.GuidUdi },
            { Constants.UdiEntityType.DocumentBlueprint, UdiType.GuidUdi },
            { Constants.UdiEntityType.Media, UdiType.GuidUdi },
            { Constants.UdiEntityType.Member, UdiType.GuidUdi },
            { Constants.UdiEntityType.DictionaryItem, UdiType.GuidUdi },
            { Constants.UdiEntityType.Macro, UdiType.GuidUdi },
            { Constants.UdiEntityType.Template, UdiType.GuidUdi },
            { Constants.UdiEntityType.DocumentType, UdiType.GuidUdi },
            { Constants.UdiEntityType.DocumentTypeContainer, UdiType.GuidUdi },
            { Constants.UdiEntityType.DocumentTypeBluePrints, UdiType.GuidUdi },
            { Constants.UdiEntityType.MediaType, UdiType.GuidUdi },
            { Constants.UdiEntityType.MediaTypeContainer, UdiType.GuidUdi },
            { Constants.UdiEntityType.DataType, UdiType.GuidUdi },
            { Constants.UdiEntityType.DataTypeContainer, UdiType.GuidUdi },
            { Constants.UdiEntityType.MemberType, UdiType.GuidUdi },
            { Constants.UdiEntityType.MemberGroup, UdiType.GuidUdi },
            { Constants.UdiEntityType.RelationType, UdiType.GuidUdi },
            { Constants.UdiEntityType.FormsForm, UdiType.GuidUdi },
            { Constants.UdiEntityType.FormsPreValue, UdiType.GuidUdi },
            { Constants.UdiEntityType.FormsDataSource, UdiType.GuidUdi },
            { Constants.UdiEntityType.AnyString, UdiType.StringUdi },
            { Constants.UdiEntityType.Language, UdiType.StringUdi },
            { Constants.UdiEntityType.MacroScript, UdiType.StringUdi },
            { Constants.UdiEntityType.MediaFile, UdiType.StringUdi },
            { Constants.UdiEntityType.TemplateFile, UdiType.StringUdi },
            { Constants.UdiEntityType.Script, UdiType.StringUdi },
            { Constants.UdiEntityType.PartialView, UdiType.StringUdi },
            { Constants.UdiEntityType.PartialViewMacro, UdiType.StringUdi },
            { Constants.UdiEntityType.Stylesheet, UdiType.StringUdi },
        };
}
