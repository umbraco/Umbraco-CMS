using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.ModelsBuilder.Building
{
    /// <summary>
    /// Represents a model.
    /// </summary>
    public class TypeModel
    {
        /// <summary>
        /// Gets the unique identifier of the corresponding content type.
        /// </summary>
        public int Id;

        /// <summary>
        /// Gets the alias of the model.
        /// </summary>
        public string Alias;

        /// <summary>
        /// Gets the name of the content type.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets the description of the content type.
        /// </summary>
        public string Description;

        /// <summary>
        /// Gets the clr name of the model.
        /// </summary>
        /// <remarks>This is the complete name eg "Foo.Bar.MyContent".</remarks>
        public string ClrName;

        /// <summary>
        /// Gets the unique identifier of the parent.
        /// </summary>
        /// <remarks>The parent can either be a base content type, or a content types container. If the content
        /// type does not have a base content type, then returns <c>-1</c>.</remarks>
        public int ParentId;

        /// <summary>
        /// Gets the base model.
        /// </summary>
        /// <remarks>
        ///     <para>If the content type does not have a base content type, then returns <c>null</c>.</para>
        ///     <para>The current model inherits from its base model.</para>
        /// </remarks>
        public TypeModel BaseType; // the parent type in Umbraco (type inherits its properties)

        /// <summary>
        /// Gets the list of properties that are defined by this model.
        /// </summary>
        /// <remarks>These are only those property that are defined locally by this model,
        /// and the list does not contain properties inherited from base models or from mixins.</remarks>
        public readonly List<PropertyModel> Properties = new List<PropertyModel>();

        /// <summary>
        /// Gets the mixin models.
        /// </summary>
        /// <remarks>The current model implements mixins.</remarks>
        public readonly List<TypeModel> MixinTypes = new List<TypeModel>();

        /// <summary>
        /// Gets the list of interfaces that this model needs to declare it implements.
        /// </summary>
        /// <remarks>Some of these interfaces may actually be implemented by a base model
        /// that this model inherits from.</remarks>
        public readonly List<TypeModel> DeclaringInterfaces = new List<TypeModel>();

        /// <summary>
        /// Gets the list of interfaces that this model needs to actually implement.
        /// </summary>
        public readonly List<TypeModel> ImplementingInterfaces = new List<TypeModel>();

        /// <summary>
        /// Gets the list of existing static mixin method candidates.
        /// </summary>
        public readonly List<string> StaticMixinMethods = new List<string>();

        /// <summary>
        /// Gets a value indicating whether this model has a base class.
        /// </summary>
        /// <remarks>Can be either because the content type has a base content type declared in Umbraco,
        /// or because the existing user's code declares a base class for this model.</remarks>
        public bool HasBase;

        /// <summary>
        /// Gets a value indicating whether this model has been renamed.
        /// </summary>
        public bool IsRenamed;

        /// <summary>
        /// Gets a value indicating whether this model has [ImplementContentType] already.
        /// </summary>
        public bool HasImplement;

        /// <summary>
        /// Gets a value indicating whether this model is used as a mixin by another model.
        /// </summary>
        public bool IsMixin;

        /// <summary>
        /// Gets a value indicating whether this model is the base model of another model.
        /// </summary>
        public bool IsParent;

        /// <summary>
        /// Gets a value indicating whether this model should be excluded from generation.
        /// </summary>
        public bool IsContentIgnored;

        /// <summary>
        /// Gets a value indicating whether the ctor is already defined in a partial.
        /// </summary>
        public bool HasCtor;

        /// <summary>
        /// Gets a value indicating whether the type is an element.
        /// </summary>
        public bool IsElement => ItemType == ItemTypes.Element;

        /// <summary>
        /// Represents the different model item types.
        /// </summary>
        public enum ItemTypes
        {
            /// <summary>
            /// Element.
            /// </summary>
            Element,

            /// <summary>
            /// Content.
            /// </summary>
            Content,

            /// <summary>
            /// Media.
            /// </summary>
            Media,

            /// <summary>
            /// Member.
            /// </summary>
            Member
        }

        private ItemTypes _itemType;

        /// <summary>
        /// Gets or sets the model item type.
        /// </summary>
        public ItemTypes ItemType
        {
            get { return _itemType; }
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
        /// Recursively collects all types inherited, or implemented as interfaces, by a specified type.
        /// </summary>
        /// <param name="types">The collection.</param>
        /// <param name="type">The type.</param>
        /// <remarks>Includes the specified type.</remarks>
        internal static void CollectImplems(ICollection<TypeModel> types, TypeModel type)
        {
            if (!type.IsContentIgnored && types.Contains(type) == false)
                types.Add(type);
            if (type.BaseType != null && !type.BaseType.IsContentIgnored)
                CollectImplems(types, type.BaseType);
            foreach (var mixin in type.MixinTypes.Where(x => !x.IsContentIgnored))
                CollectImplems(types, mixin);
        }

        /// <summary>
        /// Enumerates the base models starting from the current model up.
        /// </summary>
        /// <param name="andSelf">Indicates whether the enumeration should start with the current model
        /// or from its base model.</param>
        /// <returns>The base models.</returns>
        public IEnumerable<TypeModel> EnumerateBaseTypes(bool andSelf = false)
        {
            var typeModel = andSelf ? this : BaseType;
            while (typeModel != null)
            {
                yield return typeModel;
                typeModel = typeModel.BaseType;
            }
        }
    }
}
