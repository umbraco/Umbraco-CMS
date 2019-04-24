using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Umbraco.Tests.Testing
{
    public abstract class TestOptionAttributeBase : Attribute
    {
        public static readonly List<string> ScanAssemblies = new List<string>();

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
            var type = Type.GetType(typeName, false);
            if (type == null)
            {
                type = ScanAssemblies
                    .Select(x => Type.GetType(typeName + ", " + x, false))
                    .FirstOrDefault(x => x != null);

                //                throw new Exception("panic"); // makes no sense
                // REVIEW: But happens anyway since we can't discover tests in assemblies referencing this for some reason. However, might be able to figure something out. (AKA. Blew up, quickfix)
                if (type == null) // TODO: Panic or return default?
                    return new TOptions();
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
                throw new Exception("panic");
            return merged;
        }

        protected virtual TestOptionAttributeBase Merge(TestOptionAttributeBase other)
        {
            return this;
        }
    }
}
