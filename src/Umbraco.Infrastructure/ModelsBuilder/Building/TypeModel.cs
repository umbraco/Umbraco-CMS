using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Building;

/// <summary>
///     Represents a model.
/// </summary>
public class TypeModel
{
    /// <summary>
    ///     Gets the list of interfaces that this model needs to declare it implements.
    /// </summary>
    /// <remarks>
    ///     Some of these interfaces may actually be implemented by a base model
    ///     that this model inherits from.
    /// </remarks>
    public readonly List<TypeModel> DeclaringInterfaces = new();

    /// <summary>
    ///     Represents the different model item types.
    /// </summary>
    public enum ItemTypes
    {
        /// <summary>
        ///     Element.
        /// </summary>
        Element,

        /// <summary>
        ///     Content.
        /// </summary>
        Content,

        /// <summary>
        ///     Media.
        /// </summary>
        Media,

        /// <summary>
        ///     Member.
        /// </summary>
        Member,
    }

    /// <summary>
    ///     Gets the list of interfaces that this model needs to actually implement.
    /// </summary>
    public readonly List<TypeModel> ImplementingInterfaces = new();

    /// <summary>
    ///     Gets the mixin models.
    /// </summary>
    /// <remarks>The current model implements mixins.</remarks>
    public readonly List<TypeModel> MixinTypes = new();

    /// <summary>
    ///     Gets the list of properties that are defined by this model.
    /// </summary>
    /// <remarks>
    ///     These are only those property that are defined locally by this model,
    ///     and the list does not contain properties inherited from base models or from mixins.
    /// </remarks>
    public readonly List<PropertyModel> Properties = new();

    /// <summary>
    ///     Gets the alias of the model.
    /// </summary>
    public string Alias = string.Empty;

    private ItemTypes _itemType;

    /// <summary>
    ///     Gets the base model.
    /// </summary>
    /// <remarks>
    ///     <para>If the content type does not have a base content type, then returns <c>null</c>.</para>
    ///     <para>The current model inherits from its base model.</para>
    /// </remarks>
    public TypeModel? BaseType; // the parent type in Umbraco (type inherits its properties)

    /// <summary>
    ///     Gets the clr name of the model.
    /// </summary>
    /// <remarks>This is the complete name eg "Foo.Bar.MyContent".</remarks>
    public string ClrName = string.Empty;

    /// <summary>
    ///     Gets the description of the content type.
    /// </summary>
    public string? Description;

    ///// <summary>
    ///// Gets the list of existing static mixin method candidates.
    ///// </summary>
    // public readonly List<string> StaticMixinMethods = new List<string>(); //TODO: Do we need this? it isn't used

    /// <summary>
    ///     Gets a value indicating whether this model has a base class.
    /// </summary>
    /// <remarks>
    ///     Can be either because the content type has a base content type declared in Umbraco,
    ///     or because the existing user's code declares a base class for this model.
    /// </remarks>
    public bool HasBase;

    /// <summary>
    ///     Gets the unique identifier of the corresponding content type.
    /// </summary>
    public int Id;

    /// <summary>
    ///     Gets a value indicating whether this model is used as a mixin by another model.
    /// </summary>
    public bool IsMixin;

    /// <summary>
    ///     Gets a value indicating whether this model is the base model of another model.
    /// </summary>
    public bool IsParent;

    /// <summary>
    ///     Gets the name of the content type.
    /// </summary>
    public string? Name;

    /// <summary>
    ///     Gets the unique identifier of the parent.
    /// </summary>
    /// <remarks>
    ///     The parent can either be a base content type, or a content types container. If the content
    ///     type does not have a base content type, then returns <c>-1</c>.
    /// </remarks>
    public int ParentId;

    /// <summary>
    ///     Gets a value indicating whether the type is an element.
    /// </summary>
    public bool IsElement => ItemType == ItemTypes.Element;

    /// <summary>
    ///     Gets or sets the model item type.
    /// </summary>
    public ItemTypes ItemType
    {
        get => _itemType;
        set
        {
            switch (value)
            {
                case ItemTypes.Element:
                case ItemTypes.Content:
                case ItemTypes.Media:
                case ItemTypes.Member:
                    _itemType = value;
                    break;
                default:
                    throw new ArgumentException("value");
            }
        }
    }

    /// <summary>
    ///     Enumerates the base models starting from the current model up.
    /// </summary>
    /// <param name="andSelf">
    ///     Indicates whether the enumeration should start with the current model
    ///     or from its base model.
    /// </param>
    /// <returns>The base models.</returns>
    public IEnumerable<TypeModel> EnumerateBaseTypes(bool andSelf = false)
    {
        TypeModel? typeModel = andSelf ? this : BaseType;
        while (typeModel != null)
        {
            yield return typeModel;
            typeModel = typeModel.BaseType;
        }
    }

    /// <summary>
    ///     Recursively collects all types inherited, or implemented as interfaces, by a specified type.
    /// </summary>
    /// <param name="types">The collection.</param>
    /// <param name="type">The type.</param>
    /// <remarks>Includes the specified type.</remarks>
    internal static void CollectImplems(ICollection<TypeModel> types, TypeModel type)
    {
        if (types.Contains(type) == false)
        {
            types.Add(type);
        }

        if (type.BaseType != null)
        {
            CollectImplems(types, type.BaseType);
        }

        foreach (TypeModel mixin in type.MixinTypes)
        {
            CollectImplems(types, mixin);
        }
    }

    /// <summary>
    ///     Maps ModelType.
    /// </summary>
    public static void MapModelTypes(IList<TypeModel> typeModels, string ns)
    {
        var hasNs = !string.IsNullOrWhiteSpace(ns);
        var map = typeModels.ToDictionary(x => x.Alias, x => hasNs ? ns + "." + x.ClrName : x.ClrName);
        foreach (TypeModel typeModel in typeModels)
        {
            foreach (PropertyModel propertyModel in typeModel.Properties)
            {
                propertyModel.ClrTypeName = ModelType.MapToName(propertyModel.ModelClrType, map);
            }
        }
    }
}
