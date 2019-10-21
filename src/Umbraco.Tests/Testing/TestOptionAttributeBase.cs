﻿using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Core.Exceptions;

namespace Umbraco.Tests.Testing
{
    public abstract class TestOptionAttributeBase : Attribute
    {
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

        public static TOptions GetTestOptions<TOptions>()
            where TOptions : TestOptionAttributeBase, new()
        {
            var test = TestContext.CurrentContext.Test;
            var typeName = test.ClassName;
            var methodName = test.MethodName;
            var type = Type.GetType(typeName, true);
            if (type == null)
                throw new PanicException($"Could not resolve the type from type name {typeName}"); // makes no sense
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
