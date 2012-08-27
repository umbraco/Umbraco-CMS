using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Umbraco.Core
{
    internal class DynamicWrapper
    {
        public class DynamicWrapperBase
        {
            internal protected object RealObject;
        }

        private static readonly ModuleBuilder _moduleBuilder;

        static DynamicWrapper()
        {
            var name = Assembly.GetExecutingAssembly().GetName();
            var assembly = Thread.GetDomain().DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            _moduleBuilder = assembly.DefineDynamicModule("DynamicWrapperModule", false);
        }

        private static readonly WrapperDictionary _wrapperDictionary = new WrapperDictionary();

        public static Type GetWrapper(Type interfaceType, Type realObjectType)
        {
            Type wrapperType = _wrapperDictionary.GetType(interfaceType, realObjectType);

            if (wrapperType == null)
            {
                wrapperType = GenerateWrapperType(interfaceType, realObjectType);
                _wrapperDictionary.SetType(interfaceType, realObjectType, wrapperType);
            }

            return wrapperType;
        }

        private static Type GenerateWrapperType(Type InterfaceType, Type RealObjectType)
        {
            var wrapperName = string.Format("{0}_{1}_Wrapper", InterfaceType.Name, RealObjectType.Name);

            TypeBuilder wrapperBuilder = _moduleBuilder.DefineType(
                wrapperName,
                TypeAttributes.NotPublic | TypeAttributes.Sealed,
                typeof(DynamicWrapperBase),
                new[] { InterfaceType });

            var wrapperMethod = new WrapperMethodBuilder(RealObjectType, wrapperBuilder);

            foreach (MethodInfo method in InterfaceType.AllMethods())
            {
                wrapperMethod.Generate(method);
            }

            return wrapperBuilder.CreateType();
        }

        public static T CreateWrapper<T>(object realObject) where T : class 
        {
            var dynamicType = GetWrapper(typeof(T), realObject.GetType());
            var dynamicWrapper = (DynamicWrapperBase)Activator.CreateInstance(dynamicType);

            dynamicWrapper.RealObject = realObject;

            return dynamicWrapper as T;
        }
    }

    internal class WrapperMethodBuilder
    {
        private readonly Type _realObjectType;
        private readonly TypeBuilder _wrapperBuilder;

        public WrapperMethodBuilder(Type realObjectType, TypeBuilder proxyBuilder)
        {
            _realObjectType = realObjectType;
            _wrapperBuilder = proxyBuilder;
        }

        public void Generate(MethodInfo newMethod)
        {
            if (newMethod.IsGenericMethod)
                newMethod = newMethod.GetGenericMethodDefinition();

            FieldInfo srcField = typeof(DynamicWrapper.DynamicWrapperBase).GetField("RealObject", BindingFlags.Instance | BindingFlags.NonPublic);

            var parameters = newMethod.GetParameters();
            var parameterTypes = parameters.Select(parameter => parameter.ParameterType).ToArray();

            var methodBuilder = _wrapperBuilder.DefineMethod(
                newMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                newMethod.ReturnType,
                parameterTypes);

            if (newMethod.IsGenericMethod)
            {
                methodBuilder.DefineGenericParameters(
                    newMethod.GetGenericArguments().Select(arg => arg.Name).ToArray());
            }

            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            LoadRealObject(ilGenerator, srcField);
            PushParameters(parameters, ilGenerator);
            ExecuteMethod(newMethod, parameterTypes, ilGenerator);
            Return(ilGenerator);
        }

        private static void Return(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ret);
        }

        private void ExecuteMethod(MethodBase newMethod, Type[] parameterTypes, ILGenerator ilGenerator)
        {
            MethodInfo srcMethod = GetMethod(newMethod, parameterTypes);

            if (srcMethod == null) throw new MissingMethodException("Unable to find method " + newMethod.Name + " in " + _realObjectType.FullName);

            ilGenerator.Emit(OpCodes.Call, srcMethod);
        }

        private MethodInfo GetMethod(MethodBase realMethod, Type[] parameterTypes)
        {
            if (realMethod.IsGenericMethod)
                return _realObjectType.GetGenericMethod(realMethod.Name, parameterTypes);

            return _realObjectType.GetMethod(realMethod.Name, parameterTypes);
        }

        private static void PushParameters(ICollection<ParameterInfo> parameters, ILGenerator ilGenerator)
        {
            for (int i = 1; i < parameters.Count + 1; i++)
                ilGenerator.Emit(OpCodes.Ldarg, i);
        }

        private static void LoadRealObject(ILGenerator ilGenerator, FieldInfo srcField)
        {
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, srcField);
        }
    }

    internal class WrapperDictionary
    {
        private readonly Dictionary<string, Type> _wrapperTypes = new Dictionary<string, Type>();

        private static string GenerateKey(Type interfaceType, Type realObjectType)
        {
            return interfaceType.Name + "->" + realObjectType.Name;
        }

        public Type GetType(Type interfaceType, Type realObjectType)
        {
            string key = GenerateKey(interfaceType, realObjectType);

            if (_wrapperTypes.ContainsKey(key))
                return _wrapperTypes[key];

            return null;
        }

        public void SetType(Type interfaceType, Type realObjectType, Type wrapperType)
        {
            string key = GenerateKey(interfaceType, realObjectType);

            if (_wrapperTypes.ContainsKey(key))
                _wrapperTypes[key] = wrapperType;
            else
                _wrapperTypes.Add(key, wrapperType);
        }

    }
}