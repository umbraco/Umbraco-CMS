using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides utilities to simplify reflection.
    /// </summary>
    /// <remarks>
    /// <para>Readings:
    /// * CIL instructions: https://en.wikipedia.org/wiki/List_of_CIL_instructions
    /// * ECMA 335: https://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf
    /// * MSIL programming: http://www.blackbeltcoder.com/Articles/net/msil-programming-part-1
    /// </para>
    /// <para>Supports emitting constructors, instance and static methods, instance property getters and
    /// setters. Does not support static properties yet.</para>
    /// </remarks>
    public static partial class ReflectionUtilities
    {
        #region Properties

        /// <summary>
        /// Emits a property getter.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="mustExist">A value indicating whether the property and its getter must exist.</param>
        /// <returns>A property getter function. If <paramref name="mustExist"/> is <c>false</c>, returns null when the property or its getter does not exist.</returns>
        /// <exception cref="ArgumentNullOrEmptyException">Occurs when <paramref name="propertyName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Occurs when the property or its getter does not exist.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue"/> does not match the type of the property.</exception>
        public static Func<TDeclaring, TValue> EmitPropertyGetter<TDeclaring, TValue>(string propertyName, bool mustExist = true)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullOrEmptyException(nameof(propertyName));

            var property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property?.GetMethod != null)
                return EmitMethod<Func<TDeclaring, TValue>>(property.GetMethod);

            if (!mustExist)
                return default;

            throw new InvalidOperationException($"Could not find getter for {typeof(TDeclaring)}.{propertyName}.");
        }

        /// <summary>
        /// Emits a property setter.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="mustExist">A value indicating whether the property and its setter must exist.</param>
        /// <returns>A property setter function. If <paramref name="mustExist"/> is <c>false</c>, returns null when the property or its setter does not exist.</returns>
        /// <exception cref="ArgumentNullOrEmptyException">Occurs when <paramref name="propertyName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Occurs when the property or its setter does not exist.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue"/> does not match the type of the property.</exception>
        public static Action<TDeclaring, TValue> EmitPropertySetter<TDeclaring, TValue>(string propertyName, bool mustExist = true)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullOrEmptyException(nameof(propertyName));

            var property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property?.SetMethod != null)
                return EmitMethod<Action<TDeclaring, TValue>>(property.SetMethod);

            if (!mustExist)
                return default;

            throw new InvalidOperationException($"Could not find setter for {typeof(TDeclaring)}.{propertyName}.");
        }

        /// <summary>
        /// Emits a property getter and setter.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="mustExist">A value indicating whether the property and its getter and setter must exist.</param>
        /// <returns>A property getter and setter functions. If <paramref name="mustExist"/> is <c>false</c>, returns null when the property or its getter or setter does not exist.</returns>
        /// <exception cref="ArgumentNullOrEmptyException">Occurs when <paramref name="propertyName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Occurs when the property or its getter or setter does not exist.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue"/> does not match the type of the property.</exception>
        public static (Func<TDeclaring, TValue>, Action<TDeclaring, TValue>) EmitPropertyGetterAndSetter<TDeclaring, TValue>(string propertyName, bool mustExist = true)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullOrEmptyException(nameof(propertyName));

            var property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property?.GetMethod != null && property.SetMethod != null)
                return (
                    EmitMethod<Func<TDeclaring, TValue>>(property.GetMethod),
                    EmitMethod<Action<TDeclaring, TValue>>(property.SetMethod));

            if (!mustExist)
                return default;

            throw new InvalidOperationException($"Could not find getter and/or setter for {typeof(TDeclaring)}.{propertyName}.");
        }

        /// <summary>
        /// Emits a property getter.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>A property getter function.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Occurs when the property has no getter.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue"/> does not match the type of the property.</exception>
        public static Func<TDeclaring, TValue> EmitPropertyGetter<TDeclaring, TValue>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.GetMethod == null)
                throw new ArgumentException("Property has no getter.", nameof(propertyInfo));

            return EmitMethod<Func<TDeclaring, TValue>>(propertyInfo.GetMethod);
        }

        /// <summary>
        /// Emits a property setter.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>A property setter function.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Occurs when the property has no setter.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue"/> does not match the type of the property.</exception>
        public static Action<TDeclaring, TValue> EmitPropertySetter<TDeclaring, TValue>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.SetMethod == null)
                throw new ArgumentException("Property has no setter.", nameof(propertyInfo));

            return EmitMethod<Action<TDeclaring, TValue>>(propertyInfo.SetMethod);
        }

        /// <summary>
        /// Emits a property getter and setter.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>A property getter and setter functions.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Occurs when the property has no getter or no setter.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue"/> does not match the type of the property.</exception>
        public static (Func<TDeclaring, TValue>, Action<TDeclaring, TValue>) EmitPropertyGetterAndSetter<TDeclaring, TValue>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
                throw new ArgumentException("Property has no getter and/or no setter.", nameof(propertyInfo));

            return (
                EmitMethod<Func<TDeclaring, TValue>>(propertyInfo.GetMethod),
                EmitMethod<Action<TDeclaring, TValue>>(propertyInfo.SetMethod));
        }

        /// <summary>
        /// Emits a property setter.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TValue">The property type.</typeparam>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>A property setter function.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="propertyInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Occurs when the property has no setter.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TValue"/> does not match the type of the property.</exception>
        public static Action<TDeclaring, TValue> EmitPropertySetterUnsafe<TDeclaring, TValue>(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.SetMethod == null)
                throw new ArgumentException("Property has no setter.", nameof(propertyInfo));

            return EmitMethodUnsafe<Action<TDeclaring, TValue>>(propertyInfo.SetMethod);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Emits a constructor.
        /// </summary>
        /// <typeparam name="TLambda">A lambda representing the constructor.</typeparam>
        /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
        /// <param name="declaring">The optional type of the class to construct.</param>
        /// <returns>A constructor function. If <paramref name="mustExist"/> is <c>false</c>, returns null when the constructor does not exist.</returns>
        /// <remarks>
        /// <para>When <paramref name="declaring"/> is not specified, it is the type returned by <typeparamref name="TLambda"/>.</para>
        /// <para>The constructor arguments are determined by <typeparamref name="TLambda"/> generic arguments.</para>
        /// <para>The type returned by <typeparamref name="TLambda"/> does not need to be exactly <paramref name="declaring"/>,
        /// when e.g. that type is not known at compile time, but it has to be a parent type (eg an interface, or <c>object</c>).</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Occurs when the constructor does not exist and <paramref name="mustExist"/> is <c>true</c>.</exception>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TLambda"/> is not a Func or when <paramref name="declaring"/>
        /// is specified and does not match the function's returned type.</exception>
        public static TLambda EmitCtor<TLambda>(bool mustExist = true, Type declaring = null)
        {
            (_, var lambdaParameters, var lambdaReturned) = AnalyzeLambda<TLambda>(true, true);

            // determine returned / declaring type
            if (declaring == null) declaring = lambdaReturned;
            else if (!lambdaReturned.IsAssignableFrom(declaring))
                throw new ArgumentException($"Type {lambdaReturned} is not assignable from type {declaring}.", nameof(declaring));

            // get the constructor infos
            var ctor = declaring.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, lambdaParameters, null);
            if (ctor == null)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find constructor {declaring}.ctor({string.Join(", ", (IEnumerable<Type>) lambdaParameters)}).");
            }

            // emit
            return EmitCtorSafe<TLambda>(lambdaParameters, lambdaReturned, ctor);
        }

        /// <summary>
        /// Emits a constructor.
        /// </summary>
        /// <typeparam name="TLambda">A lambda representing the constructor.</typeparam>
        /// <param name="ctor">The constructor info.</param>
        /// <returns>A constructor function.</returns>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TLambda"/> is not a Func or when its generic
        /// arguments do not match those of <paramref name="ctor"/>.</exception>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="ctor"/> is null.</exception>
        public static TLambda EmitCtor<TLambda>(ConstructorInfo ctor)
        {
            if (ctor == null) throw new ArgumentNullException(nameof(ctor));

            (_, var lambdaParameters, var lambdaReturned) = AnalyzeLambda<TLambda>(true, true);

            return EmitCtorSafe<TLambda>(lambdaParameters, lambdaReturned, ctor);
        }

        private static TLambda EmitCtorSafe<TLambda>(Type[] lambdaParameters, Type returned, ConstructorInfo ctor)
        {
            // get type and args
            var ctorDeclaring = ctor.DeclaringType;
            var ctorParameters = ctor.GetParameters().Select(x => x.ParameterType).ToArray();

            // validate arguments
            if (lambdaParameters.Length != ctorParameters.Length)
                ThrowInvalidLambda<TLambda>("ctor", ctorDeclaring, ctorParameters);
            for (var i = 0; i < lambdaParameters.Length; i++)
                if (lambdaParameters[i] != ctorParameters[i]) // note: relax the constraint with IsAssignableFrom?
                    ThrowInvalidLambda<TLambda>("ctor", ctorDeclaring, ctorParameters);
            if (!returned.IsAssignableFrom(ctorDeclaring))
                ThrowInvalidLambda<TLambda>("ctor", ctorDeclaring, ctorParameters);

            // emit
            return EmitCtor<TLambda>(ctorDeclaring, ctorParameters, ctor);
        }

        /// <summary>
        /// Emits a constructor.
        /// </summary>
        /// <typeparam name="TLambda">A lambda representing the constructor.</typeparam>
        /// <param name="ctor">The constructor info.</param>
        /// <returns>A constructor function.</returns>
        /// <remarks>
        /// <para>The constructor is emitted in an unsafe way, using the lambda arguments without verifying
        /// them at all. This assumes that the calling code is taking care of all verifications, in order
        /// to avoid cast errors.</para>
        /// </remarks>
        /// <exception cref="ArgumentException">Occurs when <typeparamref name="TLambda"/> is not a Func or when its generic
        /// arguments do not match those of <paramref name="ctor"/>.</exception>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="ctor"/> is null.</exception>
        public static TLambda EmitCtorUnsafe<TLambda>(ConstructorInfo ctor)
        {
            if (ctor == null) throw new ArgumentNullException(nameof(ctor));

            (_, var lambdaParameters, var lambdaReturned) = AnalyzeLambda<TLambda>(true, true);

            // emit - unsafe - use lambda's args and assume they are correct
            return EmitCtor<TLambda>(lambdaReturned, lambdaParameters, ctor);
        }

        private static TLambda EmitCtor<TLambda>(Type declaring, Type[] lambdaParameters, ConstructorInfo ctor)
        {
            // gets the method argument types
            var ctorParameters = GetParameters(ctor);

            // emit
            (var dm, var ilgen) = CreateIlGenerator(ctor.DeclaringType?.Module, lambdaParameters, declaring);
            EmitLdargs(ilgen, lambdaParameters, ctorParameters);
            ilgen.Emit(OpCodes.Newobj, ctor); // ok to just return, it's only objects
            ilgen.Emit(OpCodes.Ret);

            return (TLambda) (object) dm.CreateDelegate(typeof(TLambda));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Emits a static method.
        /// </summary>
        /// <typeparam name="TDeclaring">The declaring type.</typeparam>
        /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
        /// <returns>The method. If <paramref name="mustExist"/> is <c>false</c>, returns null when the method does not exist.</returns>
        /// <remarks>
        /// <para>The method arguments are determined by <typeparamref name="TLambda"/> generic arguments.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullOrEmptyException">Occurs when <paramref name="methodName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Occurs when no proper method with name <paramref name="methodName"/> could be found.</exception>
        /// <exception cref="ArgumentException">Occurs when Occurs when <typeparamref name="TLambda"/> does not match the method signature.</exception>
        public static TLambda EmitMethod<TDeclaring, TLambda>(string methodName, bool mustExist = true)
        {
            return EmitMethod<TLambda>(typeof(TDeclaring), methodName, mustExist);
        }

        /// <summary>
        /// Emits a static method.
        /// </summary>
        /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
        /// <param name="declaring">The declaring type.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
        /// <returns>The method. If <paramref name="mustExist"/> is <c>false</c>, returns null when the method does not exist.</returns>
        /// <remarks>
        /// <para>The method arguments are determined by <typeparamref name="TLambda"/> generic arguments.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullOrEmptyException">Occurs when <paramref name="methodName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Occurs when no proper method with name <paramref name="methodName"/> could be found.</exception>
        /// <exception cref="ArgumentException">Occurs when Occurs when <typeparamref name="TLambda"/> does not match the method signature.</exception>
        public static TLambda EmitMethod<TLambda>(Type declaring, string methodName, bool mustExist = true)
        {
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentNullOrEmptyException(nameof(methodName));

            (_, var lambdaParameters, var lambdaReturned) = AnalyzeLambda<TLambda>(true, out var isFunction);

            // get the method infos
            var method = declaring.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, lambdaParameters, null);
            if (method == null || isFunction && !lambdaReturned.IsAssignableFrom(method.ReturnType))
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find static method {declaring}.{methodName}({string.Join(", ", (IEnumerable<Type>) lambdaParameters)}).");
            }

            // emit
            return EmitMethod<TLambda>(lambdaReturned, lambdaParameters, method);
        }

        /// <summary>
        /// Emits a method.
        /// </summary>
        /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
        /// <param name="method">The method info.</param>
        /// <returns>The method.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="method"/> is null.</exception>
        /// <exception cref="ArgumentException">Occurs when Occurs when <typeparamref name="TLambda"/> does not match the method signature.</exception>
        public static TLambda EmitMethod<TLambda>(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            // get type and args
            var methodDeclaring = method.DeclaringType;
            var methodReturned = method.ReturnType;
            var methodParameters = method.GetParameters().Select(x => x.ParameterType).ToArray();

            var isStatic = method.IsStatic;
            (var lambdaDeclaring, var lambdaParameters, var lambdaReturned) = AnalyzeLambda<TLambda>(isStatic, out var isFunction);

            // if not static, then the first lambda arg must be the method declaring type
            if (!isStatic && (methodDeclaring == null || !methodDeclaring.IsAssignableFrom(lambdaDeclaring)))
                ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);

            if (methodParameters.Length != lambdaParameters.Length)
                ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);

            for (var i = 0; i < methodParameters.Length; i++)
                if (!methodParameters[i].IsAssignableFrom(lambdaParameters[i]))
                    ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);

            // if it's a function then the last lambda arg must match the method returned type
            if (isFunction && !lambdaReturned.IsAssignableFrom(methodReturned))
                ThrowInvalidLambda<TLambda>(method.Name, methodReturned, methodParameters);

            // emit
            return EmitMethod<TLambda>(lambdaReturned, lambdaParameters, method);
        }

        /// <summary>
        /// Emits a method.
        /// </summary>
        /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
        /// <param name="method">The method info.</param>
        /// <returns>The method.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="method"/> is null.</exception>
        /// <exception cref="ArgumentException">Occurs when Occurs when <typeparamref name="TLambda"/> does not match the method signature.</exception>
        public static TLambda EmitMethodUnsafe<TLambda>(MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            var isStatic = method.IsStatic;
            (_, var lambdaParameters, var lambdaReturned) = AnalyzeLambda<TLambda>(isStatic, out _);

            // emit - unsafe - use lambda's args and assume they are correct
            return EmitMethod<TLambda>(lambdaReturned, lambdaParameters, method);
        }

        /// <summary>
        /// Emits an instance method.
        /// </summary>
        /// <typeparam name="TLambda">A lambda representing the method.</typeparam>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="mustExist">A value indicating whether the constructor must exist.</param>
        /// <returns>The method. If <paramref name="mustExist"/> is <c>false</c>, returns null when the method does not exist.</returns>
        /// <remarks>
        /// <para>The method arguments are determined by <typeparamref name="TLambda"/> generic arguments.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullOrEmptyException">Occurs when <paramref name="methodName"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Occurs when no proper method with name <paramref name="methodName"/> could be found.</exception>
        /// <exception cref="ArgumentException">Occurs when Occurs when <typeparamref name="TLambda"/> does not match the method signature.</exception>
        public static TLambda EmitMethod<TLambda>(string methodName, bool mustExist = true)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullOrEmptyException(nameof(methodName));

            // validate lambda type
            (var declaring, var lambdaParameters, var lambdaReturned) = AnalyzeLambda<TLambda>(false, out var isFunction);

            // get the method infos
            var method = declaring.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, lambdaParameters, null);
            if (method == null || isFunction && method.ReturnType != lambdaReturned)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find method {declaring}.{methodName}({string.Join(", ", (IEnumerable<Type>) lambdaParameters)}).");
            }

            // emit
            return EmitMethod<TLambda>(lambdaReturned, lambdaParameters, method);
        }

        // lambdaReturned = the lambda returned type (can be void)
        // lambdaArgTypes = the lambda argument types
        private static TLambda EmitMethod<TLambda>(Type lambdaReturned, Type[] lambdaParameters, MethodInfo method)
        {
            // non-static methods need the declaring type as first arg
            var parameters = lambdaParameters;
            if (!method.IsStatic)
            {
                parameters = new Type[lambdaParameters.Length + 1];
                parameters[0] = method.DeclaringType;
                Array.Copy(lambdaParameters, 0, parameters, 1, lambdaParameters.Length);
            }

            // gets the method argument types
            var methodArgTypes = GetParameters(method, withDeclaring: !method.IsStatic);

            // emit IL
            (var dm, var ilgen) = CreateIlGenerator(method.DeclaringType?.Module, parameters, lambdaReturned);
            EmitLdargs(ilgen, parameters, methodArgTypes);
            ilgen.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method);
            EmitOutputAdapter(ilgen, lambdaReturned, method.ReturnType);
            ilgen.Emit(OpCodes.Ret);

            // create
            return (TLambda) (object) dm.CreateDelegate(typeof(TLambda));
        }

        #endregion

        #region Utilities

        // when !isStatic, the first generic argument of the lambda is the declaring type
        //  hence, when !isStatic, the lambda cannot be a simple Action, as it requires at least one generic argument
        // when isFunction, the last generic argument of the lambda is the returned type
        // everything in between is parameters
        private static (Type Declaring, Type[] Parameters, Type Returned) AnalyzeLambda<TLambda>(bool isStatic, bool isFunction)
        {
            var typeLambda = typeof(TLambda);

            (var declaring, var parameters, var returned) = AnalyzeLambda<TLambda>(isStatic, out var maybeFunction);

            if (isFunction)
            {
                if (!maybeFunction)
                    throw new ArgumentException($"Lambda {typeLambda} is an Action, a Func was expected.", nameof(TLambda));
            }
            else
            {
                if (maybeFunction)
                    throw new ArgumentException($"Lambda {typeLambda} is a Func, an Action was expected.", nameof(TLambda));
            }

            return (declaring, parameters, returned);
        }

        // when !isStatic, the first generic argument of the lambda is the declaring type
        //  hence, when !isStatic, the lambda cannot be a simple Action, as it requires at least one generic argument
        // when isFunction, the last generic argument of the lambda is the returned type
        // everything in between is parameters
        private static (Type Declaring, Type[] Parameters, Type Returned) AnalyzeLambda<TLambda>(bool isStatic, out bool isFunction)
        {
            isFunction = false;

            var typeLambda = typeof(TLambda);

            var isAction = typeLambda.FullName == "System.Action";
            if (isAction)
            {
                if (!isStatic)
                    throw new ArgumentException($"Lambda {typeLambda} is an Action and can be used for static methods exclusively.", nameof(TLambda));

                return (null, Array.Empty<Type>(), typeof(void));
            }

            var genericDefinition = typeLambda.IsGenericType ? typeLambda.GetGenericTypeDefinition() : null;
            var name = genericDefinition?.FullName;

            if (name == null)
                throw new ArgumentException($"Lambda {typeLambda} is not a Func nor an Action.", nameof(TLambda));

            var isActionOf = name.StartsWith("System.Action`");
            isFunction = name.StartsWith("System.Func`");

            if (!isActionOf && !isFunction)
                throw new ArgumentException($"Lambda {typeLambda} is not a Func nor an Action.", nameof(TLambda));

            var genericArgs = typeLambda.GetGenericArguments();
            if (genericArgs.Length == 0)
                throw new Exception("Panic: Func<> or Action<> has zero generic arguments.");

            var i = 0;
            var declaring = isStatic ? typeof(void) : genericArgs[i++];

            var parameterCount = genericArgs.Length - (isStatic ? 0 : 1) - (isFunction ? 1 : 0);
            if (parameterCount < 0)
                throw new ArgumentException($"Lambda {typeLambda} is a Func and requires at least two arguments (declaring type and returned type).", nameof(TLambda));

            var parameters = new Type[parameterCount];
            for (var j = 0; j < parameterCount; j++)
                parameters[j] = genericArgs[i++];

            var returned = isFunction ? genericArgs[i] : typeof(void);

            return (declaring, parameters, returned);
        }

        private static (DynamicMethod, ILGenerator) CreateIlGenerator(Module module, Type[] arguments, Type returned)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            var dm = new DynamicMethod(string.Empty, returned, arguments, module, true);
            return (dm, dm.GetILGenerator());
        }

        private static Type[] GetParameters(ConstructorInfo ctor)
        {
            var parameters = ctor.GetParameters();
            var types = new Type[parameters.Length];
            var i = 0;
            foreach (var parameter in parameters)
                types[i++] = parameter.ParameterType;
            return types;
        }

        private static Type[] GetParameters(MethodInfo method, bool withDeclaring)
        {
            var parameters = method.GetParameters();
            var types = new Type[parameters.Length + (withDeclaring ? 1 : 0)];
            var i = 0;
            if (withDeclaring)
                types[i++] = method.DeclaringType;
            foreach (var parameter in parameters)
                types[i++] = parameter.ParameterType;
            return types;
        }

        // emits args
        private static void EmitLdargs(ILGenerator ilgen, Type[] lambdaArgTypes, Type[] methodArgTypes)
        {
            var ldargOpCodes = new[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

            if (lambdaArgTypes.Length != methodArgTypes.Length)
                throw new Exception("Panic: inconsistent number of args.");

            for (var i = 0; i < lambdaArgTypes.Length; i++)
            {
                if (lambdaArgTypes.Length < 5)
                    ilgen.Emit(ldargOpCodes[i]);
                else
                    ilgen.Emit(OpCodes.Ldarg, i);
            }
        }

        // emits adapter opcodes before OpCodes.Ret
        private static void EmitOutputAdapter(ILGenerator ilgen, Type outputType, Type methodReturnedType)
        {
            if (outputType == methodReturnedType) return;

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
                    throw new NotSupportedException("ValueType boxing.");

                // both output and returned are reference types
                // as long as returned can be assigned to output, good
                if (!outputType.IsAssignableFrom(methodReturnedType))
                    throw new NotSupportedException("Invalid cast.");
            }
        }

        private static void ThrowInvalidLambda<TLambda>(string methodName, Type returned, Type[] args)
        {
            throw new ArgumentException($"Lambda {typeof(TLambda)} does not match {methodName}({string.Join(", ", (IEnumerable<Type>) args)}):{returned}.", nameof(TLambda));
        }

        #endregion
    }
}
