using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Core.Exceptions;

namespace Umbraco.Tests.Testing
{
    public abstract class TestOptionAttributeBase : Attribute
    {
        public static readonly List<Assembly> ScanAssemblies = new List<Assembly>();

        public static TOptions GetTestOptions<TOptions>(MethodInfo method)
            where TOptions : TestOptionAttributeBase, new()
        {
            var attr = ((TOptions[]) method.GetCustomAttributes(typeof (TOptions), true)).FirstOrDefault();
            var type = method.DeclaringType;
            return Get(type, attr);
        }

        public static TOptions GetFixtureOptions<TOptions>(Type type)
            where TOptions : TestOptionAttributeBase, new()
        {
            return Get<TOptions>(type, null);
        }
        /// <summary>
        /// Find the class name and method name that has the attribute
        /// </summary>
        /// <param name="callerName">Out Caller Method Name</param>
        /// <returns>Caller Class Name</returns>
        public static string GetBenchmarkTestCaller(out string callerName)
        {
            string fullName;
            Type declaringType;
            int skipFrames = 2;
            MethodBase method;
            do
            {
                method = null;
                method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    callerName = method.Name;
                    return method.Name;
                }
                skipFrames++;

                fullName = declaringType.FullName;
                if(!declaringType.Module.Name.Equals("Umbraco.Tests.dll", StringComparison.OrdinalIgnoreCase) && method.Name == "SetUp")
                {
                    //base class setup method called SetUp, we don't want this method, we want the method that calls this one.
                    //It's a bit hacky, need to check instead if the base class is in the Umbraco.Tests.dll
                    method = null;
                    method = new StackFrame(skipFrames, false).GetMethod();
                    declaringType = method.DeclaringType;
                    if (declaringType == null)
                    {
                        callerName = method.Name;
                        return method.Name;
                    }
                    skipFrames++;
                }
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase)
            ||
            declaringType.Module.Name.Equals("Umbraco.Tests.dll", StringComparison.OrdinalIgnoreCase)
            );
            callerName = method.Name;
            return fullName;
        }
        public static TOptions GetTestOptions<TOptions>()
            where TOptions : TestOptionAttributeBase, new()
        {
            var test = TestContext.CurrentContext.Test;
            var typeName = test.ClassName;
            var methodName = test.MethodName;
            if (typeName == "NUnit.Framework.Internal.TestExecutionContext+AdhocContext")
            {
                AppDomain currentDomain = AppDomain.CurrentDomain;

                var isBenchmark = currentDomain.GetAssemblies()
                    .Any(a => a.FullName.Contains("BenchmarkDotNet"));
                if (isBenchmark)
                {
                    typeName = GetBenchmarkTestCaller(out methodName);
                }
            }
            var type = Type.GetType(typeName, false);
            if (type == null)
            {
                type = ScanAssemblies
                    .Select(assembly => assembly.GetType(typeName, false))
                    .FirstOrDefault(x => x != null);
                if (type == null)
                { 
                    throw new PanicException($"Could not resolve the running test fixture from type name {typeName}.\n" +
                                             $"To use base classes from Umbraco.Tests, add your test assembly to TestOptionAttributeBase.ScanAssemblies");
                }
            }
            var methodInfo = type.GetMethod(methodName); // what about overloads?
            var options = GetTestOptions<TOptions>(methodInfo);
            return options;
        }

        private static TOptions Get<TOptions>(Type type, TOptions attr)
            where TOptions : TestOptionAttributeBase, new()
        {
            while (type != null && type != typeof(object))
            {
                var attr2 = ((TOptions[]) type.GetCustomAttributes(typeof (TOptions), true)).FirstOrDefault();
                if (attr2 != null)
                    attr = attr == null ? attr2 : attr2.Merge(attr);
                type = type.BaseType;
            }
            return attr ?? new TOptions();
        }

        private TOptions Merge<TOptions>(TOptions other)
            where TOptions : TestOptionAttributeBase
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (!(Merge((TestOptionAttributeBase) other) is TOptions merged))
                throw new PanicException("Could not merge test options");
            return merged;
        }

        protected virtual TestOptionAttributeBase Merge(TestOptionAttributeBase other)
        {
            return this;
        }
    }
}
