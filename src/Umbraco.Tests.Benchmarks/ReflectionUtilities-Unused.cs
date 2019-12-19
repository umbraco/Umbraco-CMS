using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Umbraco.Tests.Benchmarks
{
    /// <summary>
    /// Provides utilities to simplify reflection.
    /// </summary>
    public static class ReflectionUtilitiesForTest
    {
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
