using System.Globalization;
using System.Reflection;
using Umbraco.Cms.Core.Exceptions;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <inheritdoc />
/// <summary>
///     Represents the CLR type of a model.
/// </summary>
/// <example>
///     ModelType.For("alias")
///     typeof (IEnumerable{}).MakeGenericType(ModelType.For("alias"))
///     Model.For("alias").MakeArrayType()
/// </example>
public class ModelType : Type
{
    private ModelType(string? contentTypeAlias)
    {
        if (contentTypeAlias == null)
        {
            throw new ArgumentNullException(nameof(contentTypeAlias));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(contentTypeAlias));
        }

        ContentTypeAlias = contentTypeAlias;
        Name = "{" + ContentTypeAlias + "}";
    }

    /// <summary>
    ///     Gets the content type alias.
    /// </summary>
    public string ContentTypeAlias { get; }

    /// <inheritdoc />
    public override Type UnderlyingSystemType => this;

    /// <inheritdoc />
    public override Type? BaseType => null;

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override Guid GUID { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public override Module Module => GetType().Module; // hackish but FullName requires something

    /// <inheritdoc />
    public override Assembly Assembly => GetType().Assembly; // hackish but FullName requires something

    /// <inheritdoc />
    public override string FullName => Name;

    /// <inheritdoc />
    public override string Namespace => string.Empty;

    /// <inheritdoc />
    public override string AssemblyQualifiedName => Name;

    /// <summary>
    ///     Gets the model type for a published element type.
    /// </summary>
    /// <param name="alias">The published element type alias.</param>
    /// <returns>The model type for the published element type.</returns>
    public static ModelType For(string? alias)
        => new(alias);

    /// <inheritdoc />
    public override string ToString()
        => Name;

    /// <summary>
    ///     Gets the actual CLR type by replacing model types, if any.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="modelTypes">The model types map.</param>
    /// <returns>The actual CLR type.</returns>
    public static Type Map(Type type, Dictionary<string, Type>? modelTypes)
        => Map(type, modelTypes, false);

    public static Type Map(Type type, Dictionary<string, Type>? modelTypes, bool dictionaryIsInvariant)
    {
        // it may be that senders forgot to send an invariant dictionary (garbage-in)
        if (modelTypes is not null && !dictionaryIsInvariant)
        {
            modelTypes = new Dictionary<string, Type>(modelTypes, StringComparer.InvariantCultureIgnoreCase);
        }

        if (type is ModelType modelType)
        {
            if (modelTypes?.TryGetValue(modelType.ContentTypeAlias, out Type? actualType) ?? false)
            {
                return actualType;
            }

            throw new InvalidOperationException(
                $"Don't know how to map ModelType with content type alias \"{modelType.ContentTypeAlias}\".");
        }

        if (type is ModelTypeArrayType arrayType)
        {
            if (modelTypes?.TryGetValue(arrayType.ContentTypeAlias, out Type? actualType) ?? false)
            {
                return actualType.MakeArrayType();
            }

            throw new InvalidOperationException(
                $"Don't know how to map ModelType with content type alias \"{arrayType.ContentTypeAlias}\".");
        }

        if (type.IsGenericType == false)
        {
            return type;
        }

        Type def = type.GetGenericTypeDefinition();
        if (def == null)
        {
            throw new PanicException($"The type {type} has not generic type definition");
        }

        Type[] args = type.GetGenericArguments().Select(x => Map(x, modelTypes, true)).ToArray();
        return def.MakeGenericType(args);
    }

    /// <summary>
    ///     Gets the actual CLR type name by replacing model types, if any.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="map">The model types map.</param>
    /// <returns>The actual CLR type name.</returns>
    public static string MapToName(Type type, Dictionary<string, string> map)
        => MapToName(type, map, false);

    /// <summary>
    ///     Gets a value indicating whether two <see cref="Type" /> instances are equal.
    /// </summary>
    /// <param name="t1">The first instance.</param>
    /// <param name="t2">The second instance.</param>
    /// <returns>A value indicating whether the two instances are equal.</returns>
    /// <remarks>Knows how to compare <see cref="ModelType" /> instances.</remarks>
    public static bool Equals(Type t1, Type t2)
    {
        if (t1 == t2)
        {
            return true;
        }

        if (t1 is ModelType m1 && t2 is ModelType m2)
        {
            return m1.ContentTypeAlias == m2.ContentTypeAlias;
        }

        if (t1 is ModelTypeArrayType a1 && t2 is ModelTypeArrayType a2)
        {
            return a1.ContentTypeAlias == a2.ContentTypeAlias;
        }

        if (t1.IsGenericType == false || t2.IsGenericType == false)
        {
            return false;
        }

        Type[] args1 = t1.GetGenericArguments();
        Type[] args2 = t2.GetGenericArguments();
        if (args1.Length != args2.Length)
        {
            return false;
        }

        for (var i = 0; i < args1.Length; i++)
        {
            // ReSharper disable once CheckForReferenceEqualityInstead.2
            if (Equals(args1[i], args2[i]) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        => Array.Empty<ConstructorInfo>();

    /// <inheritdoc />
    public override Type[] GetInterfaces()
        => Array.Empty<Type>();

    private static string MapToName(Type type, Dictionary<string, string> map, bool dictionaryIsInvariant)
    {
        // it may be that senders forgot to send an invariant dictionary (garbage-in)
        if (!dictionaryIsInvariant)
        {
            map = new Dictionary<string, string>(map, StringComparer.InvariantCultureIgnoreCase);
        }

        if (type is ModelType modelType)
        {
            if (map.TryGetValue(modelType.ContentTypeAlias, out var actualTypeName))
            {
                return actualTypeName;
            }

            throw new InvalidOperationException(
                $"Don't know how to map ModelType with content type alias \"{modelType.ContentTypeAlias}\".");
        }

        if (type is ModelTypeArrayType arrayType)
        {
            if (map.TryGetValue(arrayType.ContentTypeAlias, out var actualTypeName))
            {
                return actualTypeName + "[]";
            }

            throw new InvalidOperationException(
                $"Don't know how to map ModelType with content type alias \"{arrayType.ContentTypeAlias}\".");
        }

        if (type.IsGenericType == false)
        {
            return type.FullName!;
        }

        Type def = type.GetGenericTypeDefinition();
        if (def == null)
        {
            throw new PanicException($"The type {type} has not generic type definition");
        }

        var args = type.GetGenericArguments().Select(x => MapToName(x, map, true)).ToArray();
        var defFullName = def.FullName?[..def.FullName.IndexOf('`')];
        return defFullName + "<" + string.Join(", ", args) + ">";
    }

    /// <inheritdoc />
    protected override TypeAttributes GetAttributeFlagsImpl()
        => TypeAttributes.Class;

    /// <inheritdoc />
    protected override ConstructorInfo? GetConstructorImpl(
        BindingFlags bindingAttr,
        Binder? binder,
        CallingConventions callConvention,
        Type[] types,
        ParameterModifier[]? modifiers)
        => null;

    /// <inheritdoc />
    public override Type? GetInterface(string name, bool ignoreCase)
        => null;

    /// <inheritdoc />
    public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        => Array.Empty<EventInfo>();

    /// <inheritdoc />
    public override EventInfo? GetEvent(string name, BindingFlags bindingAttr)
        => null;

    /// <inheritdoc />
    public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        => Array.Empty<Type>();

    /// <inheritdoc />
    public override Type? GetNestedType(string name, BindingFlags bindingAttr)
        => null;

    /// <inheritdoc />
    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        => Array.Empty<PropertyInfo>();

    /// <inheritdoc />
    public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        => Array.Empty<MethodInfo>();

    /// <inheritdoc />
    public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        => Array.Empty<FieldInfo>();

    /// <inheritdoc />
    protected override PropertyInfo? GetPropertyImpl(
        string name,
        BindingFlags bindingAttr,
        Binder? binder,
        Type? returnType,
        Type[]? types,
        ParameterModifier[]? modifiers)
        => null;

    /// <inheritdoc />
    protected override MethodInfo? GetMethodImpl(
        string name,
        BindingFlags bindingAttr,
        Binder? binder,
        CallingConventions callConvention,
        Type[]? types,
        ParameterModifier[]? modifiers)
        => null;

    /// <inheritdoc />
    public override FieldInfo? GetField(string name, BindingFlags bindingAttr)
        => null;

    /// <inheritdoc />
    public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        => Array.Empty<MemberInfo>();

    /// <inheritdoc />
    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        => Array.Empty<object>();

    /// <inheritdoc />
    public override object[] GetCustomAttributes(bool inherit)
        => Array.Empty<object>();

    /// <inheritdoc />
    public override bool IsDefined(Type attributeType, bool inherit)
        => false;

    /// <inheritdoc />
    public override Type? GetElementType()
        => null;

    /// <inheritdoc />
    public override object InvokeMember(
        string name,
        BindingFlags invokeAttr,
        Binder? binder,
        object? target,
        object?[]? args,
        ParameterModifier[]? modifiers,
        CultureInfo? culture,
        string[]? namedParameters)
        => throw new NotSupportedException();

    /// <inheritdoc />
    protected override bool HasElementTypeImpl()
        => false;

    /// <inheritdoc />
    protected override bool IsArrayImpl()
        => false;

    /// <inheritdoc />
    protected override bool IsByRefImpl()
        => false;

    /// <inheritdoc />
    protected override bool IsPointerImpl()
        => false;

    /// <inheritdoc />
    protected override bool IsPrimitiveImpl()
        => false;

    /// <inheritdoc />
    protected override bool IsCOMObjectImpl()
        => false;

    /// <inheritdoc />
    public override Type MakeArrayType()
        => new ModelTypeArrayType(this);
}

/// <inheritdoc />
internal class ModelTypeArrayType : Type
{
    private readonly Type _elementType;

    public ModelTypeArrayType(ModelType type)
    {
        _elementType = type;
        ContentTypeAlias = type.ContentTypeAlias;
        Name = "{" + type.ContentTypeAlias + "}[*]";
    }

    public string ContentTypeAlias { get; }

    public override Type UnderlyingSystemType => this;

    public override Type? BaseType => null;

    public override string Name { get; }

    public override Guid GUID { get; } = Guid.NewGuid();

    public override Module Module => GetType().Module; // hackish but FullName requires something

    public override Assembly Assembly => GetType().Assembly; // hackish but FullName requires something

    public override string FullName => Name;

    public override string Namespace => string.Empty;

    public override string AssemblyQualifiedName => Name;

    public override string ToString()
        => Name;

    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        => Array.Empty<ConstructorInfo>();

    public override Type[] GetInterfaces()
        => Array.Empty<Type>();

    protected override TypeAttributes GetAttributeFlagsImpl()
        => TypeAttributes.Class;

    protected override ConstructorInfo? GetConstructorImpl(
        BindingFlags bindingAttr,
        Binder? binder,
        CallingConventions callConvention,
        Type[] types,
        ParameterModifier[]? modifiers)
        => null;

    public override Type? GetInterface(string name, bool ignoreCase)
        => null;

    public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        => Array.Empty<EventInfo>();

    public override EventInfo? GetEvent(string name, BindingFlags bindingAttr)
        => null;

    public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        => Array.Empty<Type>();

    public override Type? GetNestedType(string name, BindingFlags bindingAttr)
        => null;

    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        => Array.Empty<PropertyInfo>();

    public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        => Array.Empty<MethodInfo>();

    public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        => Array.Empty<FieldInfo>();

    protected override PropertyInfo? GetPropertyImpl(
        string name,
        BindingFlags bindingAttr,
        Binder? binder,
        Type? returnType,
        Type[]? types,
        ParameterModifier[]? modifiers)
        => null;

    protected override MethodInfo? GetMethodImpl(
        string name,
        BindingFlags bindingAttr,
        Binder? binder,
        CallingConventions callConvention,
        Type[]? types,
        ParameterModifier[]? modifiers)
        => null;

    public override FieldInfo? GetField(string name, BindingFlags bindingAttr)
        => null;

    public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        => Array.Empty<MemberInfo>();

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        => Array.Empty<object>();

    public override object[] GetCustomAttributes(bool inherit)
        => Array.Empty<object>();

    public override bool IsDefined(Type attributeType, bool inherit)
        => false;

    public override Type GetElementType()
        => _elementType;

    public override object InvokeMember(
        string name,
        BindingFlags invokeAttr,
        Binder? binder,
        object? target,
        object?[]? args,
        ParameterModifier[]? modifiers,
        CultureInfo? culture,
        string[]? namedParameters) =>
        throw new NotSupportedException();

    protected override bool HasElementTypeImpl()
        => true;

    protected override bool IsArrayImpl()
        => true;

    protected override bool IsByRefImpl()
        => false;

    protected override bool IsPointerImpl()
        => false;

    protected override bool IsPrimitiveImpl()
        => false;

    protected override bool IsCOMObjectImpl()
        => false;

    public override int GetArrayRank()
        => 1;
}
