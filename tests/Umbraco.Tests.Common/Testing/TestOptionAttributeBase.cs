// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Umbraco.Cms.Core.Exceptions;

namespace Umbraco.Cms.Tests.Common.Testing;

public abstract class TestOptionAttributeBase : Attribute
{
    [Obsolete("This is not used anymore - Test classes are found using nunit helpers")]
    public static readonly List<Assembly> ScanAssemblies = new();

    public static TOptions GetTestOptions<TOptions>(MethodInfo method)
        where TOptions : TestOptionAttributeBase, new()
    {
        var attr = ((TOptions[])method.GetCustomAttributes(typeof(TOptions), true)).FirstOrDefault();
        var type = method.DeclaringType;
        return Get(type, attr);
    }

    public static TOptions GetFixtureOptions<TOptions>(Type type)
        where TOptions : TestOptionAttributeBase, new() => Get<TOptions>(type, null);

    public static TOptions GetTestOptions<TOptions>()
        where TOptions : TestOptionAttributeBase, new()
    {
        var test = TestContext.CurrentContext.Test;
        var methodName = test.MethodName;
        var type = TestExecutionContext.CurrentContext.TestObject.GetType();
        var methodInfo = type.GetMethod(methodName); // what about overloads?
        var options = GetTestOptions<TOptions>(methodInfo);
        return options;
    }

    private static TOptions Get<TOptions>(Type type, TOptions attr)
        where TOptions : TestOptionAttributeBase, new()
    {
        while (type != null && type != typeof(object))
        {
            var attr2 = ((TOptions[])type.GetCustomAttributes(typeof(TOptions), true)).FirstOrDefault();
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
