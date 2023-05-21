using System.Reflection;
using System.Reflection.Emit;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides utilities to simplify reflection.
/// </summary>
/// <remarks>
///     <para>
///         Readings:
///         * CIL instructions: https://en.wikipedia.org/wiki/List_of_CIL_instructions
///         * ECMA 335: https://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf
///         * MSIL programming: http://www.blackbeltcoder.com/Articles/net/msil-programming-part-1
///     </para>
///     <para>
///         Supports emitting constructors, instance and static methods, instance property getters and
///         setters. Does not support static properties yet.
///     </para>
/// </remarks>
public static class ReflectionUtilities
{
    #region Fields

    /// <summary>
    ///     Emits a field getter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The field type.</typeparam>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>
    ///     A field getter function.
    /// </returns>
    /// <exception cref="ArgumentNullException">fieldName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="fieldName" />
    ///     or
    ///     Value type <typeparamref name="TValue" /> does not match field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" /> type.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Could not find field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" />.
    /// </exception>
    public static Func<TDeclaring, TValue> EmitFieldGetter<TDeclaring, TValue>(string fieldName)
    {
        FieldInfo field = GetField<TDeclaring, TValue>(fieldName);
        return EmitFieldGetter<TDeclaring, TValue>(field);
    }

    /// <summary>
    ///     Emits a field setter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The field type.</typeparam>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>
    ///     A field setter action.
    /// </returns>
    /// <exception cref="ArgumentNullException">fieldName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="fieldName" />
    ///     or
    ///     Value type <typeparamref name="TValue" /> does not match field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" /> type.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Could not find field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" />.
    /// </exception>
    public static Action<TDeclaring, TValue> EmitFieldSetter<TDeclaring, TValue>(string fieldName)
    {
        FieldInfo field = GetField<TDeclaring, TValue>(fieldName);
        return EmitFieldSetter<TDeclaring, TValue>(field);
    }

    /// <summary>
    ///     Emits a field getter and setter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The field type.</typeparam>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>
    ///     A field getter and setter functions.
    /// </returns>
    /// <exception cref="ArgumentNullException">fieldName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="fieldName" />
    ///     or
    ///     Value type <typeparamref name="TValue" /> does not match field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" /> type.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Could not find field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" />.
    /// </exception>
    public static (Func<TDeclaring, TValue>, Action<TDeclaring, TValue>) EmitFieldGetterAndSetter<TDeclaring, TValue>(
        string fieldName)
    {
        FieldInfo field = GetField<TDeclaring, TValue>(fieldName);
        return (EmitFieldGetter<TDeclaring, TValue>(field), EmitFieldSetter<TDeclaring, TValue>(field));
    }

    /// <summary>
    ///     Gets the field.
    /// </summary>
    /// <typeparam name="TDeclaring">The type of the declaring.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="fieldName">Name of the field.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">fieldName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="fieldName" />
    ///     or
    ///     Value type <typeparamref name="TValue" /> does not match field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" /> type.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Could not find field <typeparamref name="TDeclaring" />.
    ///     <paramref name="fieldName" />.
    /// </exception>
    private static FieldInfo GetField<TDeclaring, TValue>(string fieldName)
    {
        if (fieldName == null)
        {
            throw new ArgumentNullException(nameof(fieldName));
        }

        if (string.IsNullOrWhiteSpace(fieldName))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(fieldName));
        }

        // get the field
        FieldInfo? field = typeof(TDeclaring).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new InvalidOperationException($"Could not find field {typeof(TDeclaring)}.{fieldName}.");
        }

        // validate field type
        if (field.FieldType != typeof(TValue))
        {
            throw new ArgumentException(
                $"Value type {typeof(TValue)} does not match field {typeof(TDeclaring)}.{fieldName} type {field.FieldType}.");
        }

        return field;
    }

    private static Func<TDeclaring, TValue> EmitFieldGetter<TDeclaring, TValue>(FieldInfo field)
    {
        // emit
        (DynamicMethod dm, ILGenerator ilgen) =
            CreateIlGenerator(field.DeclaringType?.Module, new[] { typeof(TDeclaring) }, typeof(TValue));
        ilgen.Emit(OpCodes.Ldarg_0);
        ilgen.Emit(OpCodes.Ldfld, field);
        ilgen.Return();

        return (Func<TDeclaring, TValue>)dm.CreateDelegate(typeof(Func<TDeclaring, TValue>));
    }

    private static Action<TDeclaring, TValue> EmitFieldSetter<TDeclaring, TValue>(FieldInfo field)
    {
        // emit
        (DynamicMethod dm, ILGenerator ilgen) = CreateIlGenerator(field.DeclaringType?.Module, new[] { typeof(TDeclaring), typeof(TValue) }, typeof(void));
        ilgen.Emit(OpCodes.Ldarg_0);
        ilgen.Emit(OpCodes.Ldarg_1);
        ilgen.Emit(OpCodes.Stfld, field);
        ilgen.Return();

        return (Action<TDeclaring, TValue>)dm.CreateDelegate(typeof(Action<TDeclaring, TValue>));
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Emits a property getter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="mustExist">A value indicating whether the property and its getter must exist.</param>
    /// <returns>
    ///     A property getter function. If <paramref name="mustExist" /> is <c>false</c>, returns null when the property or its
    ///     getter does not exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">propertyName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="propertyName" />
    ///     or
    ///     Value type <typeparamref name="TValue" /> does not match property <typeparamref name="TDeclaring" />.
    ///     <paramref name="propertyName" /> type.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Could not find property getter for <typeparamref name="TDeclaring" />.
    ///     <paramref name="propertyName" />.
    /// </exception>
    public static Func<TDeclaring, TValue>? EmitPropertyGetter<TDeclaring, TValue>(string propertyName, bool mustExist = true)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyName));
        }

        PropertyInfo? property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property?.GetMethod != null)
        {
            return EmitMethod<Func<TDeclaring, TValue>>(property.GetMethod);
        }

        if (!mustExist)
        {
            return default;
        }

        throw new InvalidOperationException($"Could not find getter for {typeof(TDeclaring)}.{propertyName}.");
    }

    /// <summary>
    ///     Emits a property setter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="mustExist">A value indicating whether the property and its setter must exist.</param>
    /// <returns>
    ///     A property setter function. If <paramref name="mustExist" /> is <c>false</c>, returns null when the property or its
    ///     setter does not exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">propertyName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="propertyName" />
    ///     or
    ///     Value type <typeparamref name="TValue" /> does not match property <typeparamref name="TDeclaring" />.
    ///     <paramref name="propertyName" /> type.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Could not find property setter for <typeparamref name="TDeclaring" />.
    ///     <paramref name="propertyName" />.
    /// </exception>
    public static Action<TDeclaring, TValue>? EmitPropertySetter<TDeclaring, TValue>(string propertyName, bool mustExist = true)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyName));
        }

        PropertyInfo? property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property?.SetMethod != null)
        {
            return EmitMethod<Action<TDeclaring, TValue>>(property.SetMethod);
        }

        if (!mustExist)
        {
            return default;
        }

        throw new InvalidOperationException($"Could not find setter for {typeof(TDeclaring)}.{propertyName}.");
    }

    /// <summary>
    ///     Emits a property getter and setter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="mustExist">A value indicating whether the property and its getter and setter must exist.</param>
    /// <returns>
    ///     A property getter and setter functions. If <paramref name="mustExist" /> is <c>false</c>, returns null when the
    ///     property or its getter or setter does not exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">propertyName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="propertyName" />
    ///     or
    ///     Value type <typeparamref name="TValue" /> does not match property <typeparamref name="TDeclaring" />.
    ///     <paramref name="propertyName" /> type.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Could not find property getter and setter for
    ///     <typeparamref name="TDeclaring" />.<paramref name="propertyName" />.
    /// </exception>
    public static (Func<TDeclaring, TValue>, Action<TDeclaring, TValue>)
        EmitPropertyGetterAndSetter<TDeclaring, TValue>(string propertyName, bool mustExist = true)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(propertyName));
        }

        PropertyInfo? property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property?.GetMethod != null && property.SetMethod != null)
        {
            return (
                EmitMethod<Func<TDeclaring, TValue>>(property.GetMethod),
                EmitMethod<Action<TDeclaring, TValue>>(property.SetMethod));
        }

        if (!mustExist)
        {
            return default;
        }

        throw new InvalidOperationException(
            $"Could not find getter and/or setter for {typeof(TDeclaring)}.{propertyName}.");
    }

    /// <summary>
    ///     Emits a property getter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="propertyInfo">The property info.</param>
    /// <returns>A property getter function.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo" /> is null.</exception>
    /// <exception cref="ArgumentException">Occurs when the property has no getter.</exception>
    /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue" /> does not match the type of the property.</exception>
    public static Func<TDeclaring, TValue> EmitPropertyGetter<TDeclaring, TValue>(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
        {
            throw new ArgumentNullException(nameof(propertyInfo));
        }

        if (propertyInfo.GetMethod == null)
        {
            throw new ArgumentException("Property has no getter.", nameof(propertyInfo));
        }

        return EmitMethod<Func<TDeclaring, TValue>>(propertyInfo.GetMethod);
    }

    /// <summary>
    ///     Emits a property setter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="propertyInfo">The property info.</param>
    /// <returns>A property setter function.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo" /> is null.</exception>
    /// <exception cref="ArgumentException">Occurs when the property has no setter.</exception>
    /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue" /> does not match the type of the property.</exception>
    public static Action<TDeclaring, TValue> EmitPropertySetter<TDeclaring, TValue>(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
        {
            throw new ArgumentNullException(nameof(propertyInfo));
        }

        if (propertyInfo.SetMethod == null)
        {
            throw new ArgumentException("Property has no setter.", nameof(propertyInfo));
        }

        return EmitMethod<Action<TDeclaring, TValue>>(propertyInfo.SetMethod);
    }

    /// <summary>
    ///     Emits a property getter and setter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="propertyInfo">The property info.</param>
    /// <returns>A property getter and setter functions.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo" /> is null.</exception>
    /// <exception cref="ArgumentException">Occurs when the property has no getter or no setter.</exception>
    /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue" /> does not match the type of the property.</exception>
    public static (Func<TDeclaring, TValue>, Action<TDeclaring, TValue>)
        EmitPropertyGetterAndSetter<TDeclaring, TValue>(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
        {
            throw new ArgumentNullException(nameof(propertyInfo));
        }

        if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
        {
            throw new ArgumentException("Property has no getter and/or no setter.", nameof(propertyInfo));
        }

        return (
            EmitMethod<Func<TDeclaring, TValue>>(propertyInfo.GetMethod),
            EmitMethod<Action<TDeclaring, TValue>>(propertyInfo.SetMethod));
    }

    /// <summary>
    ///     Emits a property setter.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TValue">The property type.</typeparam>
    /// <param name="propertyInfo">The property info.</param>
    /// <returns>A property setter function.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo" /> is null.</exception>
    /// <exception cref="ArgumentException">Occurs when the property has no setter.</exception>
    /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue" /> does not match the type of the property.</exception>
    public static Action<TDeclaring, TValue> EmitPropertySetterUnsafe<TDeclaring, TValue>(PropertyInfo propertyInfo)
    {
        if (propertyInfo == null)
        {
            throw new ArgumentNullException(nameof(propertyInfo));
        }

        if (propertyInfo.SetMethod == null)
        {
            throw new ArgumentException("Property has no setter.", nameof(propertyInfo));
        }

        return EmitMethodUnsafe<Action<TDeclaring, TValue>>(propertyInfo.SetMethod);
    }

    #endregion

    #region Constructors

    /// <summary>
    ///     Emits a constructor.
    /// </summary>
    /// <typeparam name="TLambda">A lambda representing the constructor.</typeparam>
    /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
    /// <param name="declaring">The optional type of the class to construct.</param>
    /// <returns>
    ///     A constructor function. If <paramref name="mustExist" /> is <c>false</c>, returns null when the constructor
    ///     does not exist.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         When <paramref name="declaring" /> is not specified, it is the type returned by
    ///         <typeparamref name="TLambda" />.
    ///     </para>
    ///     <para>The constructor arguments are determined by <typeparamref name="TLambda" /> generic arguments.</para>
    ///     <para>
    ///         The type returned by <typeparamref name="TLambda" /> does not need to be exactly <paramref name="declaring" />,
    ///         when e.g. that type is not known at compile time, but it has to be a parent type (eg an interface, or
    ///         <c>object</c>).
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Occurs when the constructor does not exist and
    ///     <paramref name="mustExist" /> is <c>true</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Occurs when <typeparamref name="TLambda" /> is not a Func or when <paramref name="declaring" />
    ///     is specified and does not match the function's returned type.
    /// </exception>
    public static TLambda? EmitConstructor<TLambda>(bool mustExist = true, Type? declaring = null)
    {
        (_, Type[] lambdaParameters, Type lambdaReturned) = AnalyzeLambda<TLambda>(true, true);

        // determine returned / declaring type
        if (declaring == null)
        {
            declaring = lambdaReturned;
        }
        else if (!lambdaReturned.IsAssignableFrom(declaring))
        {
            throw new ArgumentException($"Type {lambdaReturned} is not assignable from type {declaring}.", nameof(declaring));
        }

        // get the constructor infos
        ConstructorInfo? ctor = declaring.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, lambdaParameters, null);
        if (ctor == null)
        {
            if (!mustExist)
            {
                return default;
            }

            throw new InvalidOperationException(
                $"Could not find constructor {declaring}.ctor({string.Join(", ", (IEnumerable<Type>)lambdaParameters)}).");
        }

        // emit
        return EmitConstructorSafe<TLambda>(lambdaParameters, lambdaReturned, ctor);
    }

    /// <summary>
    ///     Emits a constructor.
    /// </summary>
    /// <typeparam name="TLambda">A lambda representing the constructor.</typeparam>
    /// <param name="ctor">The constructor info.</param>
    /// <returns>A constructor function.</returns>
    /// <exception cref="ArgumentException">
    ///     Occurs when <typeparamref name="TLambda" /> is not a Func or when its generic
    ///     arguments do not match those of <paramref name="ctor" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="ctor" /> is null.</exception>
    public static TLambda EmitConstructor<TLambda>(ConstructorInfo ctor)
    {
        if (ctor == null)
        {
            throw new ArgumentNullException(nameof(ctor));
        }

        (_, Type[] lambdaParameters, Type lambdaReturned) = AnalyzeLambda<TLambda>(true, true);

        return EmitConstructorSafe<TLambda>(lambdaParameters, lambdaReturned, ctor);
    }

    private static TLambda EmitConstructorSafe<TLambda>(Type[] lambdaParameters, Type returned, ConstructorInfo ctor)
    {
        // get type and args
        Type? ctorDeclaring = ctor.DeclaringType;
        Type[] ctorParameters = ctor.GetParameters().Select(x => x.ParameterType).ToArray();

        // validate arguments
        if (lambdaParameters.Length != ctorParameters.Length)
        {
            ThrowInvalidLambda<TLambda>("ctor", ctorDeclaring, ctorParameters);
        }

        for (var i = 0; i < lambdaParameters.Length; i++)
        {
            // note: relax the constraint with IsAssignableFrom?
            if (lambdaParameters[i] != ctorParameters[i])
            {
                ThrowInvalidLambda<TLambda>("ctor", ctorDeclaring, ctorParameters);
            }
        }

        if (!returned.IsAssignableFrom(ctorDeclaring))
        {
            ThrowInvalidLambda<TLambda>("ctor", ctorDeclaring, ctorParameters);
        }

        // emit
        return EmitConstructor<TLambda>(ctorDeclaring, ctorParameters, ctor);
    }

    /// <summary>
    ///     Emits a constructor.
    /// </summary>
    /// <typeparam name="TLambda">A lambda representing the constructor.</typeparam>
    /// <param name="ctor">The constructor info.</param>
    /// <returns>A constructor function.</returns>
    /// <remarks>
    ///     <para>
    ///         The constructor is emitted in an unsafe way, using the lambda arguments without verifying
    ///         them at all. This assumes that the calling code is taking care of all verifications, in order
    ///         to avoid cast errors.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     Occurs when <typeparamref name="TLambda" /> is not a Func or when its generic
    ///     arguments do not match those of <paramref name="ctor" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="ctor" /> is null.</exception>
    public static TLambda EmitConstructorUnsafe<TLambda>(ConstructorInfo ctor)
    {
        if (ctor == null)
        {
            throw new ArgumentNullException(nameof(ctor));
        }

        (_, Type[] lambdaParameters, Type lambdaReturned) = AnalyzeLambda<TLambda>(true, true);

        // emit - unsafe - use lambda's args and assume they are correct
        return EmitConstructor<TLambda>(lambdaReturned, lambdaParameters, ctor);
    }

    private static TLambda EmitConstructor<TLambda>(Type? declaring, Type[] lambdaParameters, ConstructorInfo ctor)
    {
        // gets the method argument types
        Type[] ctorParameters = GetParameters(ctor);

        // emit
        (DynamicMethod dm, ILGenerator ilgen) =
            CreateIlGenerator(ctor.DeclaringType?.Module, lambdaParameters, declaring);
        EmitLdargs(ilgen, lambdaParameters, ctorParameters);
        ilgen.Emit(OpCodes.Newobj, ctor); // ok to just return, it's only objects
        ilgen.Return();

        return (TLambda)(object)dm.CreateDelegate(typeof(TLambda));
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Emits a static method.
    /// </summary>
    /// <typeparam name="TDeclaring">The declaring type.</typeparam>
    /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
    /// <returns>
    ///     The method. If <paramref name="mustExist" /> is <c>false</c>, returns null when the method does not exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">methodName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="methodName" />
    ///     or
    ///     Occurs when <typeparamref name="TLambda" /> does not match the method signature..
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Occurs when no proper method with name <paramref name="methodName" /> could
    ///     be found.
    /// </exception>
    /// <remarks>
    ///     The method arguments are determined by <typeparamref name="TLambda" /> generic arguments.
    /// </remarks>
    public static TLambda? EmitMethod<TDeclaring, TLambda>(string methodName, bool mustExist = true) =>
        EmitMethod<TLambda>(typeof(TDeclaring), methodName, mustExist);

    /// <summary>
    ///     Emits a static method.
    /// </summary>
    /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
    /// <param name="declaring">The declaring type.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
    /// <returns>
    ///     The method. If <paramref name="mustExist" /> is <c>false</c>, returns null when the method does not exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">methodName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="methodName" />
    ///     or
    ///     Occurs when <typeparamref name="TLambda" /> does not match the method signature..
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Occurs when no proper method with name <paramref name="methodName" /> could
    ///     be found.
    /// </exception>
    /// <remarks>
    ///     The method arguments are determined by <typeparamref name="TLambda" /> generic arguments.
    /// </remarks>
    public static TLambda? EmitMethod<TLambda>(Type declaring, string methodName, bool mustExist = true)
    {
        if (methodName == null)
        {
            throw new ArgumentNullException(nameof(methodName));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(methodName));
        }

        (Type? lambdaDeclaring, Type[] lambdaParameters, Type lambdaReturned) =
            AnalyzeLambda<TLambda>(true, out var isFunction);

        // get the method infos
        MethodInfo? method = declaring.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, lambdaParameters, null);
        if (method == null || (isFunction && !lambdaReturned.IsAssignableFrom(method.ReturnType)))
        {
            if (!mustExist)
            {
                return default;
            }

            throw new InvalidOperationException(
                $"Could not find static method {declaring}.{methodName}({string.Join(", ", (IEnumerable<Type>)lambdaParameters)}).");
        }

        // emit
        return EmitMethod<TLambda>(lambdaDeclaring, lambdaReturned, lambdaParameters, method);
    }

    /// <summary>
    ///     Emits a method.
    /// </summary>
    /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
    /// <param name="method">The method info.</param>
    /// <returns>The method.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="method" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Occurs when Occurs when <typeparamref name="TLambda" /> does not match the method
    ///     signature.
    /// </exception>
    public static TLambda EmitMethod<TLambda>(MethodInfo method)
    {
        if (method == null)
        {
            throw new ArgumentNullException(nameof(method));
        }

        // get type and args
        Type? methodDeclaring = method.DeclaringType;
        Type methodReturned = method.ReturnType;
        Type[] methodParameters = method.GetParameters().Select(x => x.ParameterType).ToArray();

        var isStatic = method.IsStatic;
        (Type? lambdaDeclaring, Type[] lambdaParameters, Type lambdaReturned) =
            AnalyzeLambda<TLambda>(isStatic, out var isFunction);

        // if not static, then the first lambda arg must be the method declaring type
        if (!isStatic && (methodDeclaring == null || !methodDeclaring.IsAssignableFrom(lambdaDeclaring)))
        {
            ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);
        }

        if (methodParameters.Length != lambdaParameters.Length)
        {
            ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);
        }

        for (var i = 0; i < methodParameters.Length; i++)
        {
            if (!methodParameters[i].IsAssignableFrom(lambdaParameters[i]))
            {
                ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);
            }
        }

        // if it's a function then the last lambda arg must match the method returned type
        if (isFunction && !lambdaReturned.IsAssignableFrom(methodReturned))
        {
            ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);
        }

        // emit
        return EmitMethod<TLambda>(lambdaDeclaring, lambdaReturned, lambdaParameters, method);
    }

    /// <summary>
    ///     Emits a method.
    /// </summary>
    /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
    /// <param name="method">The method info.</param>
    /// <returns>The method.</returns>
    /// <exception cref="ArgumentNullException">Occurs when <paramref name="method" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Occurs when Occurs when <typeparamref name="TLambda" /> does not match the method
    ///     signature.
    /// </exception>
    public static TLambda EmitMethodUnsafe<TLambda>(MethodInfo method)
    {
        if (method == null)
        {
            throw new ArgumentNullException(nameof(method));
        }

        var isStatic = method.IsStatic;
        (Type? lambdaDeclaring, Type[] lambdaParameters, Type lambdaReturned) = AnalyzeLambda<TLambda>(isStatic, out _);

        // emit - unsafe - use lambda's args and assume they are correct
        return EmitMethod<TLambda>(lambdaDeclaring, lambdaReturned, lambdaParameters, method);
    }

    /// <summary>
    ///     Emits an instance method.
    /// </summary>
    /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
    /// <returns>
    ///     The method. If <paramref name="mustExist" /> is <c>false</c>, returns null when the method does not exist.
    /// </returns>
    /// <exception cref="ArgumentNullException">methodName</exception>
    /// <exception cref="ArgumentException">
    ///     Value can't be empty or consist only of white-space characters. - <paramref name="methodName" />
    ///     or
    ///     Occurs when <typeparamref name="TLambda" /> does not match the method signature..
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Occurs when no proper method with name <paramref name="methodName" /> could
    ///     be found.
    /// </exception>
    /// <remarks>
    ///     The method arguments are determined by <typeparamref name="TLambda" /> generic arguments.
    /// </remarks>
    public static TLambda? EmitMethod<TLambda>(string methodName, bool mustExist = true)
    {
        if (methodName == null)
        {
            throw new ArgumentNullException(nameof(methodName));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(methodName));
        }

        // validate lambda type
        (Type? lambdaDeclaring, Type[] lambdaParameters, Type lambdaReturned) =
            AnalyzeLambda<TLambda>(false, out var isFunction);

        // get the method infos
        MethodInfo? method = lambdaDeclaring?.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, lambdaParameters, null);
        if (method == null || (isFunction && method.ReturnType != lambdaReturned))
        {
            if (!mustExist)
            {
                return default;
            }

            throw new InvalidOperationException(
                $"Could not find method {lambdaDeclaring}.{methodName}({string.Join(", ", (IEnumerable<Type>)lambdaParameters)}).");
        }

        // emit
        return EmitMethod<TLambda>(lambdaDeclaring, lambdaReturned, lambdaParameters, method);
    }

    // lambdaReturned = the lambda returned type (can be void)
    // lambdaArgTypes = the lambda argument types
    private static TLambda EmitMethod<TLambda>(Type? lambdaDeclaring, Type lambdaReturned, Type[] lambdaParameters, MethodInfo method)
    {
        // non-static methods need the declaring type as first arg
        Type[] parameters = lambdaParameters;
        if (!method.IsStatic)
        {
            parameters = new Type[lambdaParameters.Length + 1];
            parameters[0] = lambdaDeclaring ?? method.DeclaringType!;
            Array.Copy(lambdaParameters, 0, parameters, 1, lambdaParameters.Length);
        }

        // gets the method argument types
        Type[] methodArgTypes = GetParameters(method, !method.IsStatic);

        // emit IL
        (DynamicMethod dm, ILGenerator ilgen) =
            CreateIlGenerator(method.DeclaringType?.Module, parameters, lambdaReturned);
        EmitLdargs(ilgen, parameters, methodArgTypes);
        ilgen.CallMethod(method);
        EmitOutputAdapter(ilgen, lambdaReturned, method.ReturnType);
        ilgen.Return();

        // create
        return (TLambda)(object)dm.CreateDelegate(typeof(TLambda));
    }

    #endregion

    #region Utilities

    // when !isStatic, the first generic argument of the lambda is the declaring type
    //  hence, when !isStatic, the lambda cannot be a simple Action, as it requires at least one generic argument
    // when isFunction, the last generic argument of the lambda is the returned type
    // everything in between is parameters
    private static (Type? Declaring, Type[] Parameters, Type Returned) AnalyzeLambda<TLambda>(bool isStatic, bool isFunction)
    {
        Type typeLambda = typeof(TLambda);

        (Type? declaring, Type[] parameters, Type returned) = AnalyzeLambda<TLambda>(isStatic, out var maybeFunction);

        if (isFunction)
        {
            if (!maybeFunction)
            {
                throw new ArgumentException($"Lambda {typeLambda} is an Action, a Func was expected.", nameof(TLambda));
            }
        }
        else
        {
            if (maybeFunction)
            {
                throw new ArgumentException($"Lambda {typeLambda} is a Func, an Action was expected.", nameof(TLambda));
            }
        }

        return (declaring, parameters, returned);
    }

    // when !isStatic, the first generic argument of the lambda is the declaring type
    //  hence, when !isStatic, the lambda cannot be a simple Action, as it requires at least one generic argument
    // when isFunction, the last generic argument of the lambda is the returned type
    // everything in between is parameters
    private static (Type? Declaring, Type[] Parameters, Type Returned) AnalyzeLambda<TLambda>(bool isStatic, out bool isFunction)
    {
        isFunction = false;

        Type typeLambda = typeof(TLambda);

        var isAction = typeLambda.FullName == "System.Action";
        if (isAction)
        {
            if (!isStatic)
            {
                throw new ArgumentException(
                    $"Lambda {typeLambda} is an Action and can be used for static methods exclusively.",
                    nameof(TLambda));
            }

            return (null, Array.Empty<Type>(), typeof(void));
        }

        Type? genericDefinition = typeLambda.IsGenericType ? typeLambda.GetGenericTypeDefinition() : null;
        var name = genericDefinition?.FullName;

        if (name == null)
        {
            throw new ArgumentException($"Lambda {typeLambda} is not a Func nor an Action.", nameof(TLambda));
        }

        var isActionOf = name.StartsWith("System.Action`");
        isFunction = name.StartsWith("System.Func`");

        if (!isActionOf && !isFunction)
        {
            throw new ArgumentException($"Lambda {typeLambda} is not a Func nor an Action.", nameof(TLambda));
        }

        Type[] genericArgs = typeLambda.GetGenericArguments();
        if (genericArgs.Length == 0)
        {
            throw new Exception("Panic: Func<> or Action<> has zero generic arguments.");
        }

        var i = 0;
        Type declaring = isStatic ? typeof(void) : genericArgs[i++];

        var parameterCount = genericArgs.Length - (isStatic ? 0 : 1) - (isFunction ? 1 : 0);
        if (parameterCount < 0)
        {
            throw new ArgumentException(
                $"Lambda {typeLambda} is a Func and requires at least two arguments (declaring type and returned type).",
                nameof(TLambda));
        }

        var parameters = new Type[parameterCount];
        for (var j = 0; j < parameterCount; j++)
        {
            parameters[j] = genericArgs[i++];
        }

        Type returned = isFunction ? genericArgs[i] : typeof(void);

        return (declaring, parameters, returned);
    }

    private static (DynamicMethod, ILGenerator) CreateIlGenerator(Module? module, Type[] arguments, Type? returned)
    {
        if (module == null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        var dm = new DynamicMethod(string.Empty, returned, arguments, module, true);
        return (dm, dm.GetILGenerator());
    }

    private static Type[] GetParameters(ConstructorInfo ctor)
    {
        ParameterInfo[] parameters = ctor.GetParameters();
        var types = new Type[parameters.Length];
        var i = 0;
        foreach (ParameterInfo parameter in parameters)
        {
            types[i++] = parameter.ParameterType;
        }

        return types;
    }

    private static Type[] GetParameters(MethodInfo method, bool withDeclaring)
    {
        ParameterInfo[] parameters = method.GetParameters();
        var types = new Type[parameters.Length + (withDeclaring ? 1 : 0)];
        var i = 0;
        if (withDeclaring)
        {
            types[i++] = method.DeclaringType!;
        }

        foreach (ParameterInfo parameter in parameters)
        {
            types[i++] = parameter.ParameterType;
        }

        return types;
    }

    // emits args
    private static void EmitLdargs(ILGenerator ilgen, Type[] lambdaArgTypes, Type[] methodArgTypes)
    {
        OpCode[] ldargOpCodes = new[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

        if (lambdaArgTypes.Length != methodArgTypes.Length)
        {
            throw new Exception("Panic: inconsistent number of args.");
        }

        for (var i = 0; i < lambdaArgTypes.Length; i++)
        {
            if (lambdaArgTypes.Length < 5)
            {
                ilgen.Emit(ldargOpCodes[i]);
            }
            else
            {
                ilgen.Emit(OpCodes.Ldarg, i);
            }

            // var local = false;
            EmitInputAdapter(ilgen, lambdaArgTypes[i], methodArgTypes[i] /*, ref local*/);
        }
    }

    // emits adapter opcodes after OpCodes.Ldarg
    //  inputType is the lambda input type
    //  methodParamType is the actual type expected by the actual method
    // adding code to do inputType -> methodParamType
    //  valueType  -> valueType  : not supported ('cos, why?)
    //  valueType  -> !valueType : not supported ('cos, why?)
    //  !valueType -> valueType  : unbox and convert
    //  !valueType -> !valueType : cast (could throw)
    private static void EmitInputAdapter(ILGenerator ilgen, Type inputType, Type methodParamType /*, ref bool local*/)
    {
        if (inputType == methodParamType)
        {
            return;
        }

        if (methodParamType.IsValueType)
        {
            if (inputType.IsValueType)
            {
                // both input and parameter are value types
                // not supported, use proper input
                // (otherwise, would require converting)
                throw new NotSupportedException("ValueTypes conversion.");
            }

            // parameter is value type, but input is reference type
            // unbox the input to the parameter value type
            // this is more or less equivalent to the ToT method below
            Label unbox = ilgen.DefineLabel();

            // if (!local)
            // {
            //    ilgen.DeclareLocal(typeof(object)); // declare local var for st/ld loc_0
            //    local = true;
            // }

            // stack: value

            // following code can be replaced with .Dump (and then we don't need the local variable anymore)
            // ilgen.Emit(OpCodes.Stloc_0); // pop value into loc.0
            //// stack:
            // ilgen.Emit(OpCodes.Ldloc_0); // push loc.0
            // ilgen.Emit(OpCodes.Ldloc_0); // push loc.0
            ilgen.Emit(OpCodes.Dup); // duplicate top of stack

            // stack: value ; value
            ilgen.Emit(OpCodes.Isinst, methodParamType); // test, pops value, and pushes either a null ref, or an instance of the type

            // stack: inst|null ; value
            ilgen.Emit(OpCodes.Ldnull); // push null

            // stack: null ; inst|null ; value
            ilgen.Emit(OpCodes.Cgt_Un); // compare what isInst returned to null - pops 2 values, and pushes 1 if greater else 0

            // stack: 0|1 ; value
            ilgen.Emit(OpCodes.Brtrue_S, unbox); // pops value, branches to unbox if true, ie nonzero

            // stack: value
            ilgen.Convert(methodParamType); // convert

            // stack: value|converted
            ilgen.MarkLabel(unbox);
            ilgen.Emit(OpCodes.Unbox_Any, methodParamType);
        }
        else
        {
            // parameter is reference type, but input is value type
            // not supported, input should always be less constrained
            // (otherwise, would require boxing and converting)
            if (inputType.IsValueType)
            {
                throw new NotSupportedException("ValueType boxing.");
            }

            // both input and parameter are reference types
            // cast the input to the parameter type
            ilgen.Emit(OpCodes.Castclass, methodParamType);
        }
    }

    // private static T ToT<T>(object o)
    // {
    //     return o is T t ? t : (T) System.Convert.ChangeType(o, typeof(T));
    // }
    private static MethodInfo? _convertMethod;
    private static MethodInfo? _getTypeFromHandle;

    private static void Convert(this ILGenerator ilgen, Type type)
    {
        if (_getTypeFromHandle == null)
        {
            _getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(RuntimeTypeHandle) }, null);
        }

        if (_convertMethod == null)
        {
            _convertMethod = typeof(Convert).GetMethod("ChangeType", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object), typeof(Type) }, null);
        }

        ilgen.Emit(OpCodes.Ldtoken, type);
        ilgen.CallMethod(_getTypeFromHandle);
        ilgen.CallMethod(_convertMethod);
    }

    // emits adapter code before OpCodes.Ret
    //  outputType is the lambda output type
    //  methodReturnedType is the actual type returned by the actual method
    // adding code to do methodReturnedType -> outputType
    //  valueType  -> valueType  : not supported ('cos, why?)
    //  valueType  -> !valueType : box
    //  !valueType -> valueType  : not supported ('cos, why?)
    //  !valueType -> !valueType : implicit cast (could throw)
    private static void EmitOutputAdapter(ILGenerator ilgen, Type outputType, Type methodReturnedType)
    {
        if (outputType == methodReturnedType)
        {
            return;
        }

        // note: the only important thing to support here, is returning a specific type
        // as an object, when emitting the method as a Func<..., object> - anything else
        // is pointless really - so we box value types, and ensure that non value types
        // can be assigned
        if (methodReturnedType.IsValueType)
        {
            if (outputType.IsValueType)
            {
                // both returned and output are value types
                // not supported, use proper output
                // (otherwise, would require converting)
                throw new NotSupportedException("ValueTypes conversion.");
            }

            // returned is value type, but output is reference type
            // box the returned value
            ilgen.Emit(OpCodes.Box, methodReturnedType);
        }
        else
        {
            // returned is reference type, but output is value type
            // not supported, output should always be less constrained
            // (otherwise, would require boxing and converting)
            if (outputType.IsValueType)
            {
                throw new NotSupportedException("ValueType boxing.");
            }

            // both output and returned are reference types
            // as long as returned can be assigned to output, good
            if (!outputType.IsAssignableFrom(methodReturnedType))
            {
                throw new NotSupportedException("Invalid cast.");
            }
        }
    }

    private static void ThrowInvalidLambda<TLambda>(string methodName, Type? returned, Type[] args) =>
        throw new ArgumentException(
            $"Lambda {typeof(TLambda)} does not match {methodName}({string.Join(", ", (IEnumerable<Type>)args)}):{returned}.",
            nameof(TLambda));

    private static void CallMethod(this ILGenerator ilgen, MethodInfo? method)
    {
        if (method is not null)
        {
            var virt = !method.IsStatic && (method.IsVirtual || !method.IsFinal);
            ilgen.Emit(virt ? OpCodes.Callvirt : OpCodes.Call, method);
        }
    }

    private static void Return(this ILGenerator ilgen) => ilgen.Emit(OpCodes.Ret);

    #endregion
}
