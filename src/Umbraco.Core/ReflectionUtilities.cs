using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public static class ReflectionUtilities
    {
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
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullOrEmptyException(nameof(propertyName));

            var property = typeof (TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || property.GetMethod == null)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find getter for {typeof(TDeclaring)}.{propertyName}.");
            }

            return EmitMethod<Func<TDeclaring, TValue>>(property.GetMethod);
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
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullOrEmptyException(nameof(propertyName));

            var property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || property.SetMethod == null)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find setter for {typeof(TDeclaring)}.{propertyName}.");
            }

            return EmitMethod<Action<TDeclaring, TValue>>(property.SetMethod);
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
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullOrEmptyException(nameof(propertyName));

            var property = typeof(TDeclaring).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null || property.GetMethod == null || property.SetMethod == null)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find getter and/or setter for {typeof(TDeclaring)}.{propertyName}.");
            }

            return (
                EmitMethod<Func<TDeclaring, TValue>>(property.GetMethod),
                EmitMethod<Action<TDeclaring, TValue>>(property.SetMethod));
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
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.GetMethod == null)
                throw new ArgumentException("Property has no getter.", nameof(propertyInfo));

            return EmitMethod<Func<TDeclaring, TValue>> (propertyInfo.GetMethod);
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
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

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
            if (propertyInfo == null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
                throw new ArgumentException("Property has no getter and/or no setter.", nameof(propertyInfo));

            return (
                EmitMethod<Func<TDeclaring, TValue>>(propertyInfo.GetMethod),
                EmitMethod<Action<TDeclaring, TValue>>(propertyInfo.SetMethod));
        }

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
            // validate lambda type
            ValidateCtorLambda<TLambda>();

            // get instance and arguments types
            var genericArgs = typeof(TLambda).GetGenericArguments();
            var args = new Type[genericArgs.Length - 1];
            for (var i = 0; i < args.Length; i++)
                args[i] = genericArgs[i];
            var returned = genericArgs[args.Length];

            if (declaring == null)
                declaring = returned;
            else if (!returned.IsAssignableFrom(declaring))
                throw new ArgumentException($"Type {returned} is not assignable from type {declaring}.", nameof(declaring));

            // get the constructor infos
            var ctor = declaring.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, args, null);

            if (ctor == null)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find constructor {declaring}.ctor({string.Join(", ", (IEnumerable<Type>) args)}).");
            }

            // emit
            return EmitCtor<TLambda>(declaring, args, returned, ctor);
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
            if (ctor == null)
                throw new ArgumentNullException(nameof(ctor));

            // get type and args
            var declaring = ctor.DeclaringType;
            var args = ctor.GetParameters().Select(x => x.ParameterType).ToArray();

            // validate lambda type
            ValidateCtorLambda<TLambda>();

            // validate arguments
            var genericArgs = typeof(TLambda).GetGenericArguments();
            if (genericArgs.Length != args.Length + 1)
                ThrowInvalidLambda<TLambda>("ctor", declaring, args);
            for (var i = 0; i < args.Length; i++)
                if (args[i] != genericArgs[i])
                    ThrowInvalidLambda<TLambda>("ctor", declaring, args);
            if (!genericArgs[args.Length].IsAssignableFrom(declaring))
                ThrowInvalidLambda<TLambda>("ctor", declaring, args);

            // emit
            return EmitCtor<TLambda>(declaring, args, declaring, ctor);
        }

        private static void ValidateCtorLambda<TLambda>()
        {
            var typeLambda = typeof(TLambda);
            var genericDefinition = typeLambda.IsGenericType ? typeLambda.GetGenericTypeDefinition() : null;
            if (genericDefinition == null || genericDefinition.FullName == null || !genericDefinition.FullName.StartsWith("System.Func`"))
                throw new ArgumentException($"Lambda {typeLambda} is not a Func.", nameof(TLambda));
        }

        private static TLambda EmitCtor<TLambda>(Type declaring, Type[] args, Type returned, ConstructorInfo ctor)
        {
            var dm = new DynamicMethod(string.Empty, returned, args, declaring.Module, true);
            var ilgen = dm.GetILGenerator();
            EmitLdargs(ilgen, args.Length);
            ilgen.Emit(OpCodes.Newobj, ctor); // ok to just return, it's only objects
            ilgen.Emit(OpCodes.Ret);
            return (TLambda) (object) dm.CreateDelegate(typeof(TLambda));
        }

        private static void ValidateMethodLambda<TLambda>(out bool isFunction)
        {
            isFunction = false;

            var typeLambda = typeof(TLambda);
            var genericDefinition = typeLambda.IsGenericType ? typeLambda.GetGenericTypeDefinition() : null;

            if (typeLambda.FullName == "System.Action")
                return;

            if (genericDefinition == null
                || genericDefinition.FullName == null
                || !genericDefinition.FullName.StartsWith("System.Func`") && !genericDefinition.FullName.StartsWith("System.Action`"))
                throw new ArgumentException($"Lambda {typeLambda} is not a Func nor an Action.", nameof(TLambda));

            isFunction = genericDefinition.FullName.StartsWith("System.Func`");
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
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            // get type and args
            var type = method.DeclaringType;
            var returned = method.ReturnType;
            var args = method.GetParameters().Select(x => x.ParameterType).ToArray();

            // validate lambda type
            ValidateMethodLambda<TLambda>(out var isFunction);

            var genericArgs = typeof(TLambda).GetGenericArguments();
            var isStatic = method.IsStatic;
            var ax = 0;
            var gx = 0;

            // must match the expected number of args
            var expectedCount = (isStatic ? 0 : 1) + args.Length + (isFunction ? 1 : 0);
            if (expectedCount != genericArgs.Length)
                ThrowInvalidLambda<TLambda>(method.Name, returned, args);

            // if not static then the first generic arg must be the declaring type
            if (!isStatic && genericArgs[gx++] != type)
                ThrowInvalidLambda<TLambda>(method.Name, returned, args);

            // all other generic args must match parameters
            // except the last one, if it's a function, 'cos then its the returned type
            while (gx < genericArgs.Length - (isFunction ? 1 : 0))
                if (genericArgs[gx++] != args[ax++])
                    ThrowInvalidLambda<TLambda>(method.Name, returned, args);

            // if it's a function then the last one must match the returned type
            if (isFunction && genericArgs[gx] != returned)
                ThrowInvalidLambda<TLambda>(method.Name, returned, args);

            // emit
            return EmitMethod<TLambda>(returned, args, method);
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
            ValidateMethodLambda<TLambda>(out var isFunction);

            // get instance and arguments types
            var genericArgs = typeof(TLambda).GetGenericArguments();
            var gx = 0;
            var declaring = genericArgs[gx++];
            var args = new Type[genericArgs.Length - 1 - (isFunction ? 1 : 0)];
            for (var i = 0; i < args.Length; i++)
                args[i] = genericArgs[gx++];
            var returned = isFunction ? genericArgs[gx] : typeof (void);

            // get the method infos
            var method = declaring.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, args, null);

            if (method == null || isFunction && method.ReturnType != returned)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find method {declaring}.{methodName}({string.Join(", ", (IEnumerable<Type>) args)}).");
            }

            // emit
            return EmitMethod<TLambda>(returned, args, method);
        }

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
            return EmitMethod<TLambda>(typeof (TDeclaring), methodName, mustExist);
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
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentNullOrEmptyException(nameof(methodName));

            // validate lambda type
            ValidateMethodLambda<TLambda>(out var isFunction);

            // get instance and arguments types
            var genericArgs = typeof(TLambda).GetGenericArguments();
            var args = new Type[genericArgs.Length - (isFunction ? 1 : 0)];
            for (var i = 0; i < args.Length; i++)
                args[i] = genericArgs[i];
            var returned = isFunction ? genericArgs[args.Length] : typeof (void);

            // get the method infos
            var method = declaring.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static,
                null, args, null);

            if (method == null || isFunction && method.ReturnType != returned)
            {
                if (!mustExist) return default;
                throw new InvalidOperationException($"Could not find static method {declaring}.{methodName}({string.Join(", ", (IEnumerable<Type>) args)}).");
            }

            // emit
            return EmitMethod<TLambda>(returned, args, method);
        }

        private static TLambda EmitMethod<TLambda>(Type returned, Type[] args, MethodInfo method)
        {
            var args2 = args;
            if (!method.IsStatic)
            {
                args2 = new Type[args.Length + 1];
                args2[0] = method.DeclaringType;
                Array.Copy(args, 0, args2, 1, args.Length);
            }
            var module = method.DeclaringType?.Module;
            if (module == null)
                throw new ArgumentException("Failed to get method's declaring type module.", nameof(method));
            var dm = new DynamicMethod(string.Empty, returned, args2, module, true);
            var ilgen = dm.GetILGenerator();
            EmitLdargs(ilgen, args2.Length);
            ilgen.Emit(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method);
            ilgen.Emit(OpCodes.Ret);
            return (TLambda) (object) dm.CreateDelegate(typeof(TLambda));
        }

        private static void EmitLdargs(ILGenerator ilgen, int count)
        {
            if (count < 5)
            {
                if (count > 0)
                    ilgen.Emit(OpCodes.Ldarg_0);
                if (count > 1)
                    ilgen.Emit(OpCodes.Ldarg_1);
                if (count > 2)
                    ilgen.Emit(OpCodes.Ldarg_2);
                if (count > 3)
                    ilgen.Emit(OpCodes.Ldarg_3);
            }
            else
            {
                for (var i = 0; i < count; i++)
                    ilgen.Emit(OpCodes.Ldarg, i);
            }
        }

        private static void ThrowInvalidLambda<TLambda>(string methodName, Type returned, Type[] args)
        {
            throw new ArgumentException($"Lambda {typeof(TLambda)} does not match {methodName}({string.Join(", ", (IEnumerable<Type>) args)}):{returned}.", nameof(TLambda));
        }

        // the code below should NOT be used
        //
        // keeping all this around as a reference for now - building expressions into dynamic assemblies,
        // the resulting methods are fast (as fast as IL-generated methods) whereas the standard compiled
        // expressions are slowish - alas, the compiled methods do not have access to eg private members
        // and anything that would violate access control - we're not using it anymore - still used in a
        // benchmark

        internal static Func<TArg0, TInstance> GetCtor<TInstance, TArg0>()
        {
            var type = typeof (TInstance);
            var type0 = typeof (TArg0);

            // get the constructor infos
            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, new[] { type0 }, null);

            if (ctor == null)
                throw new InvalidOperationException($"Could not find constructor {type}.ctor({type0}).");

            var exprArgs = ctor.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            // ReSharper disable once CoVariantArrayConversion
            var exprNew = Expression.New(ctor, exprArgs);
            var expr = Expression.Lambda<Func<TArg0, TInstance>>(exprNew, exprArgs);
            return CompileToDelegate(expr);
        }

        internal static Func<TArg0, TArg1, TInstance> GetCtor<TInstance, TArg0, TArg1>()
        {
            var type = typeof (TInstance);
            var type0 = typeof (TArg0);
            var type1 = typeof (TArg1);

            // get the constructor infos
            var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, new[] { type0, type1 }, null);

            if (ctor == null)
                throw new InvalidOperationException($"Could not find constructor {type}.ctor({type0}, {type1}).");

            var exprArgs = ctor.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
            // ReSharper disable once CoVariantArrayConversion
            var exprNew = Expression.New(ctor, exprArgs);
            var expr = Expression.Lambda<Func<TArg0, TArg1, TInstance>>(exprNew, exprArgs);
            return CompileToDelegate(expr);
        }

        internal static TMethod GetMethod<TMethod>(MethodInfo method)
        {
            var type = method.DeclaringType;

            GetMethodParms<TMethod>(out var parameterTypes, out var returnType);
            return GetStaticMethod<TMethod>(method, method.Name, type, parameterTypes, returnType);
        }

        internal static TMethod GetMethod<TInstance, TMethod>(MethodInfo method)
        {
            var type = method.DeclaringType;

            GetMethodParms<TInstance, TMethod>(out var parameterTypes, out var returnType);
            return GetMethod<TMethod>(method, method.Name, type, parameterTypes, returnType);
        }

        internal static TMethod GetMethod<TInstance, TMethod>(string methodName)
        {
            var type = typeof (TInstance);

            GetMethodParms<TInstance, TMethod>(out var parameterTypes, out var returnType);

            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, parameterTypes, null);

            return GetMethod<TMethod>(method, methodName, type, parameterTypes, returnType);
        }

        private static void GetMethodParms<TMethod>(out Type[] parameterTypes, out Type returnType)
        {
            var typeM = typeof (TMethod);
            var typeList = new List<Type>();
            returnType = typeof (void);

            if (!typeof (MulticastDelegate).IsAssignableFrom(typeM) || typeM == typeof (MulticastDelegate))
                throw new InvalidOperationException("Invalid TMethod, must be a Func or Action.");

            var typeName = typeM.FullName;
            if (typeName == null)
                throw new InvalidOperationException($"Could not get {typeM} type name.");
            if (typeName.StartsWith("System.Func`"))
            {
                var i = 0;
                while (i < typeM.GenericTypeArguments.Length - 1)
                    typeList.Add(typeM.GenericTypeArguments[i++]);
                returnType = typeM.GenericTypeArguments[i];
            }
            else if (typeName.StartsWith("System.Action`"))
            {
                var i = 0;
                while (i < typeM.GenericTypeArguments.Length)
                    typeList.Add(typeM.GenericTypeArguments[i++]);
            }
            else if (typeName == "System.Action")
            {
                // no args
            }
            else
                throw new InvalidOperationException(typeName);

            parameterTypes = typeList.ToArray();
        }

        private static void GetMethodParms<TInstance, TMethod>(out Type[] parameterTypes, out Type returnType)
        {
            var type = typeof (TInstance);

            var typeM = typeof (TMethod);
            var typeList = new List<Type>();
            returnType = typeof (void);

            if (!typeof (MulticastDelegate).IsAssignableFrom(typeM) || typeM == typeof (MulticastDelegate))
                throw new InvalidOperationException("Invalid TMethod, must be a Func or Action.");

            var typeName = typeM.FullName;
            if (typeName == null)
                throw new InvalidOperationException($"Could not get {typeM} type name.");
            if (!typeM.IsGenericType)
                throw new InvalidOperationException($"Type {typeName} is not generic.");
            if (typeM.GenericTypeArguments[0] != type)
                throw new InvalidOperationException($"Invalid type {typeName}, the first generic argument must be {type.FullName}.");
            if (typeName.StartsWith("System.Func`"))
            {
                var i = 1;
                while (i < typeM.GenericTypeArguments.Length - 1)
                    typeList.Add(typeM.GenericTypeArguments[i++]);
                returnType = typeM.GenericTypeArguments[i];
            }
            else if (typeName.StartsWith("System.Action`"))
            {
                var i = 1;
                while (i < typeM.GenericTypeArguments.Length)
                    typeList.Add(typeM.GenericTypeArguments[i++]);
            }
            else
                throw new InvalidOperationException(typeName);

            parameterTypes = typeList.ToArray();
        }

        private static TMethod GetStaticMethod<TMethod>(MethodInfo method, string methodName, Type type, Type[] parameterTypes, Type returnType)
        {
            if (method == null || method.ReturnType != returnType)
                throw new InvalidOperationException($"Could not find static method {type}.{methodName}({string.Join(",", parameterTypes.Select(x => x.ToString()))}) : {returnType}");

            var e = new List<ParameterExpression>();
            foreach (var p in method.GetParameters())
                e.Add(Expression.Parameter(p.ParameterType, p.Name));
            var exprCallArgs = e.ToArray();
            var exprLambdaArgs = exprCallArgs;

            // ReSharper disable once CoVariantArrayConversion
            var exprCall = Expression.Call(method, exprCallArgs);
            var expr = Expression.Lambda<TMethod>(exprCall, exprLambdaArgs);
            return CompileToDelegate(expr);
        }

        private static TMethod GetMethod<TMethod>(MethodInfo method, string methodName, Type type, Type[] parameterTypes, Type returnType)
        {
            if (method == null || method.ReturnType != returnType)
                throw new InvalidOperationException($"Could not find method {type}.{methodName}({string.Join(",", parameterTypes.Select(x => x.ToString()))}) : {returnType}");

            var e = new List<ParameterExpression>();
            foreach (var p in method.GetParameters())
                e.Add(Expression.Parameter(p.ParameterType, p.Name));
            var exprCallArgs = e.ToArray();

            var exprThis = Expression.Parameter(type, "this");
            e.Insert(0, exprThis);
            var exprLambdaArgs = e.ToArray();

            // ReSharper disable once CoVariantArrayConversion
            var exprCall = Expression.Call(exprThis, method, exprCallArgs);
            var expr = Expression.Lambda<TMethod>(exprCall, exprLambdaArgs);
            return expr.Compile();
        }

        internal const AssemblyBuilderAccess DefaultAssemblyBuilderAccess = AssemblyBuilderAccess.Run;
        internal const AssemblyBuilderAccess NoAssembly = 0;

        internal static TLambda Compile<TLambda>(Expression<TLambda> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            return access == NoAssembly
                ? expr.Compile()
                : CompileToDelegate(expr, access);
        }

        internal static Action CompileToDelegate(Expression<Action> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static); // CompileToMethod requires a static method

            expr.CompileToMethod(builder);

            return (Action) Delegate.CreateDelegate(typeof (Action), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static Action<T1> CompileToDelegate<T1>(Expression<Action<T1>> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                typeof (void), new[] { typeof (T1) });

            expr.CompileToMethod(builder);

            return (Action<T1>) Delegate.CreateDelegate(typeof (Action<T1>), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static Action<T1, T2> CompileToDelegate<T1, T2>(Expression<Action<T1, T2>> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                typeof (void), new[] { typeof (T1), typeof (T2) });

            expr.CompileToMethod(builder);

            return (Action<T1, T2>) Delegate.CreateDelegate(typeof (Action<T1, T2>), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static Action<T1, T2, T3> CompileToDelegate<T1, T2, T3>(Expression<Action<T1, T2, T3>> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                typeof (void), new[] { typeof (T1), typeof (T2), typeof (T3) });

            expr.CompileToMethod(builder);

            return (Action<T1, T2, T3>) Delegate.CreateDelegate(typeof (Action<T1, T2, T3>), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static Func<TResult> CompileToDelegate<TResult>(Expression<Func<TResult>> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof (TResult), Array.Empty<Type>()); // CompileToMethod requires a static method

            expr.CompileToMethod(builder);

            return (Func<TResult>) Delegate.CreateDelegate(typeof (Func<TResult>), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static Func<T1, TResult> CompileToDelegate<T1, TResult>(Expression<Func<T1, TResult>> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                typeof (TResult), new[] { typeof (T1) });

            expr.CompileToMethod(builder);

            return (Func<T1, TResult>) Delegate.CreateDelegate(typeof (Func<T1, TResult>), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static Func<T1, T2, TResult> CompileToDelegate<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                typeof (TResult), new[] { typeof (T1), typeof (T2) });

            expr.CompileToMethod(builder);

            return (Func<T1, T2, TResult>) Delegate.CreateDelegate(typeof (Func<T1, T2, TResult>), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static Func<T1, T2, T3, TResult> CompileToDelegate<T1, T2, T3, TResult>(Expression<Func<T1, T2, T3, TResult>> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                typeof (TResult), new[] { typeof (T1), typeof (T2), typeof (T3) });

            expr.CompileToMethod(builder);

            return (Func<T1, T2, T3, TResult>) Delegate.CreateDelegate(typeof (Func<T1, T2, T3, TResult>), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static TMethod CompileToDelegate<TMethod>(Expression<TMethod> expr, AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var typeBuilder = CreateTypeBuilder(access);

            GetMethodParms<TMethod>(out var parameterTypes, out var returnType);

            var builder = typeBuilder.DefineMethod("Method",
                MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                returnType, parameterTypes);

            expr.CompileToMethod(builder);

            return (TMethod) (object) Delegate.CreateDelegate(typeof (TMethod), typeBuilder.CreateType().GetMethod("Method"));
        }

        internal static TMethod[] CompileToDelegates<TMethod>(params Expression<TMethod>[] exprs)
            => CompileToDelegates(AssemblyBuilderAccess.RunAndCollect, exprs);

        internal static TMethod[] CompileToDelegates<TMethod>(AssemblyBuilderAccess access, params Expression<TMethod>[] exprs)
        {
            var typeBuilder = CreateTypeBuilder(access);

            GetMethodParms<TMethod>(out var parameterTypes, out var returnType);

            var i = 0;
            foreach (var expr in exprs)
            {
                var builder = typeBuilder.DefineMethod($"Method_{i++}",
                    MethodAttributes.Public | MethodAttributes.Static, // CompileToMethod requires a static method
                    returnType, parameterTypes);

                expr.CompileToMethod(builder);
            }

            var type = typeBuilder.CreateType();

            var methods = new TMethod[exprs.Length];
            for (i = 0; i < exprs.Length; i++)
                methods[i] = (TMethod) (object) Delegate.CreateDelegate(typeof (TMethod), type.GetMethod($"Method_{i++}"));
            return methods;
        }

        private static TypeBuilder CreateTypeBuilder(AssemblyBuilderAccess access = DefaultAssemblyBuilderAccess)
        {
            var assemblyName = new AssemblyName("Umbraco.Core.DynamicAssemblies." + Guid.NewGuid().ToString("N"));
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, access);

            var module = (access & AssemblyBuilderAccess.Save) > 0
                ? assembly.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll")
                : assembly.DefineDynamicModule(assemblyName.Name); // has to be transient

            return module.DefineType("Class", TypeAttributes.Public | TypeAttributes.Abstract);
        }
    }
}
