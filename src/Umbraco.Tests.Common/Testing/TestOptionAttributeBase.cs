// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            TOptions attr = ((TOptions[])method.GetCustomAttributes(typeof(TOptions), true)).FirstOrDefault();
            Type type = method.DeclaringType;
            return Get(type, attr);
        }

        public static TOptions GetFixtureOptions<TOptions>(Type type)
            where TOptions : TestOptionAttributeBase, new() => Get<TOptions>(type, null);

        public static TOptions GetTestOptions<TOptions>()
            where TOptions : TestOptionAttributeBase, new()
        {
            TestContext.TestAdapter test = TestContext.CurrentContext.Test;
            var typeName = test.ClassName;
            var methodName = test.MethodName;

            // This will only get types from whatever is already loaded in the app domain.
            var type = Type.GetType(typeName, false);
            if (type == null)
            {
                // automatically add the executing and calling assemblies to the list to scan for this type
                var scanAssemblies = ScanAssemblies.Union(new[] { Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly() }).ToList();

                type = scanAssemblies
                    .Select(assembly => assembly.GetType(typeName, false))
                    .FirstOrDefault(x => x != null);
                if (type == null)
                {
                    throw new PanicException($"Could not resolve the running test fixture from type name {typeName}.\n" +
                                             $"To use base classes from Umbraco.Tests, add your test assembly to TestOptionAttributeBase.ScanAssemblies");
                }
            }

            MethodInfo methodInfo = type.GetMethod(methodName); // what about overloads?
            TOptions options = GetTestOptions<TOptions>(methodInfo);
            return options;
        }

        private static TOptions Get<TOptions>(Type type, TOptions attr)
            where TOptions : TestOptionAttributeBase, new()
        {
            while (type != null && type != typeof(object))
            {
                TOptions attr2 = ((TOptions[])type.GetCustomAttributes(typeof(TOptions), true)).FirstOrDefault();
                if (attr2 != null)
                {
                    attr = attr == null ? attr2 : attr2.Merge(attr);
                }

                type = type.BaseType;
            }

            return attr ?? new TOptions();
        }

        private TOptions Merge<TOptions>(TOptions other)
            where TOptions : TestOptionAttributeBase
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (!(Merge((TestOptionAttributeBase)other) is TOptions merged))
            {
                throw new PanicException("Could not merge test options");
            }

            return merged;
        }

        protected virtual TestOptionAttributeBase Merge(TestOptionAttributeBase other) => this;
    }
}
