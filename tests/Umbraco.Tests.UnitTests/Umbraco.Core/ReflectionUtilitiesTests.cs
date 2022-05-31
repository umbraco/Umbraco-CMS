// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class ReflectionUtilitiesTests
{
    [Test]
    public void EmitCtorEmits()
    {
        var ctor1 = ReflectionUtilities.EmitConstructor<Func<Class1>>();
        Assert.IsInstanceOf<Class1>(ctor1());

        var ctor2 = ReflectionUtilities.EmitConstructor<Func<object>>(declaring: typeof(Class1));
        Assert.IsInstanceOf<Class1>(ctor2());

        var ctor3 = ReflectionUtilities.EmitConstructor<Func<int, Class3>>();
        Assert.IsInstanceOf<Class3>(ctor3(42));

        var ctor4 = ReflectionUtilities.EmitConstructor<Func<int, object>>(declaring: typeof(Class3));
        Assert.IsInstanceOf<Class3>(ctor4(42));
    }

    [Test]
    public void EmitCtorEmitsFromInfo()
    {
        var ctorInfo = typeof(Class1).GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            CallingConventions.Any,
            Array.Empty<Type>(),
            null);
        var ctor1 = ReflectionUtilities.EmitConstructor<Func<Class1>>(ctorInfo);
        Assert.IsInstanceOf<Class1>(ctor1());

        ctorInfo = typeof(Class1).GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            CallingConventions.Any,
            new[]
            {
                typeof(int),
            },
            null);
        var ctor3 = ReflectionUtilities.EmitConstructor<Func<int, object>>(ctorInfo);
        Assert.IsInstanceOf<Class1>(ctor3(42));

        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitConstructor<Func<string, object>>(ctorInfo));
    }

    [Test]
    public void EmitCtorEmitsPrivateCtor()
    {
        var ctor = ReflectionUtilities.EmitConstructor<Func<string, Class3>>();
        Assert.IsInstanceOf<Class3>(ctor("foo"));
    }

    [Test]
    public void EmitCtorThrowsIfNotFound() =>
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitConstructor<Func<bool, Class3>>());

    [Test]
    public void EmitCtorThrowsIfInvalid()
    {
        var ctorInfo = typeof(Class1).GetConstructor(
            BindingFlags.Public | BindingFlags.Instance,
            null,
            CallingConventions.Any,
            Array.Empty<Type>(),
            null);
        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitConstructor<Func<Class2>>(ctorInfo));
    }

    [Test]
    public void EmitCtorReturnsNull() =>
        Assert.IsNull(ReflectionUtilities.EmitConstructor<Func<bool, Class3>>(false));

    [Test]
    public void EmitMethodEmitsInstance()
    {
        var class1 = new Class1();

        var method1 = ReflectionUtilities.EmitMethod<Action<Class1>>("Method1");
        method1(class1);

        var method2 = ReflectionUtilities.EmitMethod<Action<Class1, int>>("Method2");
        method2(class1, 42);

        var method3 = ReflectionUtilities.EmitMethod<Func<Class1, int>>("Method3");
        Assert.AreEqual(42, method3(class1));

        var method4 = ReflectionUtilities.EmitMethod<Func<Class1, string, int>>("Method4");
        Assert.AreEqual(42, method4(class1, "42"));
    }

    [Test]
    public void EmitMethodEmitsStatic()
    {
        var method1 = ReflectionUtilities.EmitMethod<Class1, Action>("SMethod1");
        method1();

        var method2 = ReflectionUtilities.EmitMethod<Class1, Action<int>>("SMethod2");
        method2(42);

        var method3 = ReflectionUtilities.EmitMethod<Class1, Func<int>>("SMethod3");
        Assert.AreEqual(42, method3());

        var method4 = ReflectionUtilities.EmitMethod<Class1, Func<string, int>>("SMethod4");
        Assert.AreEqual(42, method4("42"));
    }

    [Test]
    public void EmitMethodEmitsStaticStatic()
    {
        var method = ReflectionUtilities.EmitMethod<Action>(typeof(StaticClass1), "Method");
        method();
    }

    [Test]
    public void EmitMethodEmitsFromInfo()
    {
        var class1 = new Class1();

        var methodInfo = typeof(Class1).GetMethod("Method1", BindingFlags.Instance | BindingFlags.Public);
        var method1 = ReflectionUtilities.EmitMethod<Action<Class1>>(methodInfo);
        method1(class1);

        methodInfo = typeof(Class1).GetMethod(
            "Method2",
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new[]
            {
                typeof(int),
            },
            null);
        var method2 = ReflectionUtilities.EmitMethod<Action<Class1, int>>(methodInfo);
        method2(class1, 42);

        methodInfo = typeof(Class1).GetMethod("Method3", BindingFlags.Instance | BindingFlags.Public);
        var method3 = ReflectionUtilities.EmitMethod<Func<Class1, int>>(methodInfo);
        Assert.AreEqual(42, method3(class1));

        methodInfo = typeof(Class1).GetMethod(
            "Method4",
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new[]
            {
                typeof(string),
            },
            null);
        var method4 = ReflectionUtilities.EmitMethod<Func<Class1, string, int>>(methodInfo);
        Assert.AreEqual(42, method4(class1, "42"));

        methodInfo = typeof(Class1).GetMethod("SMethod1", BindingFlags.Static | BindingFlags.Public);
        var smethod1 = ReflectionUtilities.EmitMethod<Action>(methodInfo);
        smethod1();

        methodInfo = typeof(Class1).GetMethod("SMethod2", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(int) }, null);
        var smethod2 = ReflectionUtilities.EmitMethod<Action<int>>(methodInfo);
        smethod2(42);

        methodInfo = typeof(Class1).GetMethod("SMethod3", BindingFlags.Static | BindingFlags.Public);
        var smethod3 = ReflectionUtilities.EmitMethod<Func<int>>(methodInfo);
        Assert.AreEqual(42, smethod3());

        methodInfo = typeof(Class1).GetMethod(
            "SMethod4",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[] { typeof(string) },
            null);
        var smethod4 = ReflectionUtilities.EmitMethod<Func<string, int>>(methodInfo);
        Assert.AreEqual(42, smethod4("42"));

        methodInfo = typeof(StaticClass1).GetMethod("Method", BindingFlags.Static | BindingFlags.Public);
        var method = ReflectionUtilities.EmitMethod<Action>(methodInfo);
        method();
    }

    [Test]
    public void EmitMethodEmitsPrivateMethod()
    {
        var class1 = new Class1();

        var method1 = ReflectionUtilities.EmitMethod<Action<Class1>>("MethodP1");
        method1(class1);

        var method2 = ReflectionUtilities.EmitMethod<Class1, Action>("SMethodP1");
        method2();
    }

    [Test]
    public void EmitMethodThrowsIfNotFound()
    {
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitMethod<Action<Class1>>("ZZZ"));
        Assert.Throws<InvalidOperationException>(() =>
            ReflectionUtilities.EmitMethod<Action<Class1, int, int>>("Method1"));
    }

    [Test]
    public void EmitMethodThrowsIfInvalid()
    {
        var methodInfo = typeof(Class1).GetMethod("Method1", BindingFlags.Instance | BindingFlags.Public);
        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitMethod<Action<Class1, int, int>>(methodInfo));
    }

    [Test]
    public void EmitMethodReturnsNull()
    {
        Assert.IsNull(ReflectionUtilities.EmitMethod<Action<Class1>>("ZZZ", false));
        Assert.IsNull(ReflectionUtilities.EmitMethod<Action<Class1, int, int>>("Method1", false));
    }

    [Test]
    public void EmitPropertyEmits()
    {
        var class1 = new Class1();

        var getter1 = ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value1");
        Assert.AreEqual(42, getter1(class1));

        var getter2 = ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value3");
        Assert.AreEqual(42, getter2(class1));

        var setter1 = ReflectionUtilities.EmitPropertySetter<Class1, int>("Value2");
        setter1(class1, 42);

        var setter2 = ReflectionUtilities.EmitPropertySetter<Class1, int>("Value3");
        setter2(class1, 42);

        (var getter3, var setter3) = ReflectionUtilities.EmitPropertyGetterAndSetter<Class1, int>("Value3");
        Assert.AreEqual(42, getter3(class1));
        setter3(class1, 42);
    }

    [Test]
    public void EmitPropertyEmitsFromInfo()
    {
        var class1 = new Class1();

        var propertyInfo = typeof(Class1).GetProperty("Value1");
        var getter1 = ReflectionUtilities.EmitPropertyGetter<Class1, int>(propertyInfo);
        Assert.AreEqual(42, getter1(class1));

        propertyInfo = typeof(Class1).GetProperty("Value3");
        var getter2 = ReflectionUtilities.EmitPropertyGetter<Class1, int>(propertyInfo);
        Assert.AreEqual(42, getter2(class1));

        propertyInfo = typeof(Class1).GetProperty("Value2");
        var setter1 = ReflectionUtilities.EmitPropertySetter<Class1, int>(propertyInfo);
        setter1(class1, 42);

        propertyInfo = typeof(Class1).GetProperty("Value3");
        var setter2 = ReflectionUtilities.EmitPropertySetter<Class1, int>(propertyInfo);
        setter2(class1, 42);

        (var getter3, var setter3) = ReflectionUtilities.EmitPropertyGetterAndSetter<Class1, int>(propertyInfo);
        Assert.AreEqual(42, getter3(class1));
        setter3(class1, 42);
    }

    [Test]
    public void EmitPropertyEmitsPrivateProperty()
    {
        var class1 = new Class1();

        var getter1 = ReflectionUtilities.EmitPropertyGetter<Class1, int>("ValueP1");
        Assert.AreEqual(42, getter1(class1));
    }

    [Test]
    public void EmitPropertyThrowsIfNotFound()
    {
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitPropertyGetter<Class1, int>("Zalue1"));
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value2"));

        var propertyInfo = typeof(Class1).GetProperty("Value1");
        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitPropertySetter<Class1, int>(propertyInfo));
    }

    [Test]
    public void EmitPropertyThrowsIfInvalid() =>
        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitPropertyGetter<Class1, string>("Value1"));

    [Test]
    public void EmitPropertyReturnsNull()
    {
        Assert.IsNull(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Zalue1", false));
        Assert.IsNull(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value2", false));
    }

    [Test]
    public void PropertySetterCanCastUnsafeValue()
    {
        // test that we can emit property setters that cast from eg 'object'
        var type4 = typeof(Class4);
        var propInt4 = type4.GetProperty("IntValue");
        var propString4 = type4.GetProperty("StringValue");
        var propClassA4 = type4.GetProperty("ClassAValue");

        var object4 = new Class4();
        var object2A = new Class2A();

        // works with a string property
        Assert.IsNotNull(propString4);
        var setterString4 = ReflectionUtilities.EmitPropertySetterUnsafe<Class4, object>(propString4);
        Assert.IsNotNull(setterString4);
        setterString4(object4, "foo");
        Assert.IsNotNull(object4.StringValue);
        Assert.AreEqual("foo", object4.StringValue);

        // unsafe is... unsafe
        Assert.Throws<InvalidCastException>(() => setterString4(object4, new Class2()));

        // works with a reference property
        Assert.IsNotNull(propClassA4);
        var setterClassA4 = ReflectionUtilities.EmitPropertySetterUnsafe<Class4, object>(propClassA4);
        Assert.IsNotNull(setterClassA4);
        setterClassA4(object4, object2A);
        Assert.IsNotNull(object4.ClassAValue);
        Assert.AreEqual(object2A, object4.ClassAValue);

        // works with a boxed value type
        Assert.IsNotNull(propInt4);
        var setterInt4 = ReflectionUtilities.EmitPropertySetterUnsafe<Class4, object>(propInt4);
        Assert.IsNotNull(setterInt4);

        setterInt4(object4, 42);
        Assert.AreEqual(42, object4.IntValue);

        // FIXME: the code below runs fine with ReSharper test running within VisualStudio
        // but it crashes when running via vstest.console.exe - unless some settings are required?

        // converting works
        setterInt4(object4, 42.0);
        Assert.AreEqual(42, object4.IntValue);

        // unsafe is... unsafe
        Assert.Throws<FormatException>(() => setterInt4(object4, "foo"));
    }

    [Test]
    public void PropertySetterCanCastObject()
    {
        // Class5 inherits from Class4 and ClassValue is defined on Class4
        var type5 = typeof(Class5);
        var propClass4 = type5.GetProperty("ClassValue");

        var object2 = new Class2();

        // can cast the object type from Class5 to Class4
        var setterClass4 = ReflectionUtilities.EmitPropertySetter<Class5, Class2>(propClass4);

        var object4 = new Class5();
        setterClass4(object4, object2);
        Assert.IsNotNull(object4.ClassValue);
        Assert.AreSame(object2, object4.ClassValue);
    }

    [Test]
    public void PropertySetterCanCastUnsafeObject()
    {
        var type5 = typeof(Class5);
        var propClass4 = type5.GetProperty("ClassValue");

        var object2 = new Class2();

        // can cast the object type from object to Class4
        var setterClass4 = ReflectionUtilities.EmitPropertySetterUnsafe<object, Class2>(propClass4);

        var object4 = new Class5();
        setterClass4(object4, object2);
        Assert.IsNotNull(object4.ClassValue);
        Assert.AreSame(object2, object4.ClassValue);
    }

    [Test]
    public void PropertyGetterCanCastValue()
    {
        var type4 = typeof(Class4);
        var propClassA4 = type4.GetProperty("ClassAValue");
        var propInt4 = type4.GetProperty("IntValue");

        var object2A = new Class2A();
        var object4 = new Class4 { ClassAValue = object2A, IntValue = 159 };

        // can cast the return type from Class2A to Class2
        var getterClassA4 = ReflectionUtilities.EmitPropertyGetter<Class4, Class2>(propClassA4);

        var valueClass4A = getterClassA4(object4);
        Assert.IsNotNull(valueClass4A);
        Assert.AreSame(object2A, valueClass4A);

        // cannot cast the return type from Class2A to Class3!
        Assert.Throws<ArgumentException>(()
            => ReflectionUtilities.EmitPropertyGetter<Class4, Class3>(propClassA4));

        // can cast and box the return type from int to object
        var getterInt4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propInt4);

        var valueInt4 = getterInt4(object4);
        Assert.IsTrue(valueInt4 is int);
        Assert.AreEqual(159, valueInt4);

        // cannot cast the return type from int to Class3!
        Assert.Throws<ArgumentException>(()
            => ReflectionUtilities.EmitPropertyGetter<Class4, Class3>(propInt4));
    }

    [Test]
    public void PropertyGetterCanCastObject()
    {
        var type5 = typeof(Class5);
        var propClass4 = type5.GetProperty("ClassValue");

        var object2 = new Class2();
        var object4 = new Class5 { ClassValue = object2 };

        // can cast the object type from Class5 to Class4
        var getterClass4 = ReflectionUtilities.EmitPropertyGetter<Class5, Class2>(propClass4);

        var valueClass4 = getterClass4(object4);
        Assert.IsNotNull(valueClass4);
        Assert.AreSame(object2, valueClass4);

        // cannot cast the object type from Class3 to Class4!
        Assert.Throws<ArgumentException>(()
            => ReflectionUtilities.EmitPropertyGetter<Class3, Class2>(propClass4));
    }

    [Test]
    public void EmitPropertyCastGetterEmits()
    {
        // test that we can emit property getters that cast the returned value to 'object'
        // test simple class
        var type4 = typeof(Class4);

        var object4 = new Class4 { IntValue = 1, StringValue = "foo", ClassValue = new Class2() };

        // works with a string property
        var propString4 = type4.GetProperty("StringValue");
        Assert.IsNotNull(propString4);
        var getterString4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propString4);
        Assert.IsNotNull(getterString4);
        var valueString4 = getterString4(object4);
        Assert.IsNotNull(valueString4);
        Assert.AreEqual("foo", valueString4);

        // works with a reference property
        var propClass4 = type4.GetProperty("ClassValue");
        Assert.IsNotNull(propClass4);
        var getterClass4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propClass4);
        Assert.IsNotNull(getterClass4);
        var valueClass4 = getterClass4(object4);
        Assert.IsNotNull(valueClass4);
        Assert.IsInstanceOf<Class2>(valueClass4);

        // works with a value type property
        var propInt4 = type4.GetProperty("IntValue");
        Assert.IsNotNull(propInt4);

        // ... if explicitly getting a value type
        var getterInt4T = ReflectionUtilities.EmitPropertyGetter<Class4, int>(propInt4);
        Assert.IsNotNull(getterInt4T);
        var valueInt4T = getterInt4T(object4);
        Assert.AreEqual(1, valueInt4T);

        // ... if using a compiled getter
        var valueInt4D = GetIntValue(object4);
        Assert.IsNotNull(valueInt4D);
        Assert.IsTrue(valueInt4D is int);
        Assert.AreEqual(1, valueInt4D);

        // ... if getting a non-value type (emit adds a box)
        var getterInt4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propInt4);
        Assert.IsNotNull(getterInt4);
        var valueInt4 = getterInt4(object4);
        Assert.IsNotNull(valueInt4);
        Assert.IsTrue(valueInt4 is int);
        Assert.AreEqual(1, valueInt4);

        var getters4 = type4
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .ToDictionary(x => x.Name, x => (object)ReflectionUtilities.EmitPropertyGetter<Class4, object>(x));

        Console.WriteLine("Getting object4 values...");
        var values4 = getters4.ToDictionary(kvp => kvp.Key, kvp => ((Func<Class4, object>)kvp.Value)(object4));

        Console.WriteLine("Writing object4 values...");
        foreach ((var name, var value) in values4)
        {
            Console.WriteLine($"{name}: {value}");
        }

        Assert.AreEqual(4, values4.Count);
        Assert.AreEqual("foo", values4["StringValue"]);
        Assert.IsInstanceOf<Class2>(values4["ClassValue"]);
        Assert.AreEqual(1, values4["IntValue"]);

        // test hierarchy
        var type5 = typeof(Class5);

        var getters5 = type5
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .ToDictionary(x => x.Name, x => (object)ReflectionUtilities.EmitPropertyGetter<Class5, object>(x));

        var object5 = new Class5
        {
            IntValue = 1,
            IntValue2 = 1,
            StringValue = "foo",
            StringValue2 = "foo",
            ClassValue = new Class2(),
            ClassValue2 = new Class2(),
        };

        Console.WriteLine("Getting object5 values...");
        var values5 = getters5.ToDictionary(kvp => kvp.Key, kvp => ((Func<Class5, object>)kvp.Value)(object5));

        Console.WriteLine("Writing object5 values...");
        foreach ((var name, var value) in values5)
        {
            Console.WriteLine($"{name}: {value}");
        }

        Assert.AreEqual(7, values5.Count);
        Assert.AreEqual("foo", values5["StringValue"]);
        Assert.IsInstanceOf<Class2>(values5["ClassValue"]);
        Assert.AreEqual(1, values5["IntValue"]);
        Assert.AreEqual("foo", values5["StringValue2"]);
        Assert.IsInstanceOf<Class2>(values5["ClassValue2"]);
        Assert.AreEqual(1, values5["IntValue2"]);

        // test object extensions
        Console.WriteLine("Getting object5D values...");
        var values5D = ObjectJsonExtensions.ToObjectDictionary(object5);

        Console.WriteLine("Writing object5D values...");
        foreach ((var name, var value) in values5)
        {
            Console.WriteLine($"{name}: {value}");
        }

        Assert.AreEqual(7, values5.Count);
        Assert.AreEqual("foo", values5D["StringValue"]);
        Assert.IsInstanceOf<Class2>(values5D["ClassValue"]);
        Assert.AreEqual(1, values5D["IntValue"]);
        Assert.AreEqual("foo", values5D["StringValue2"]);
        Assert.IsInstanceOf<Class2>(values5D["ClassValue2"]);
        Assert.AreEqual(1, values5D["intValue2"]); // JsonProperty changes property name
    }

    [Test]
    public void EmitFieldGetterSetterEmits()
    {
        var getter1 = ReflectionUtilities.EmitFieldGetter<Class1, int>("Field1");
        var getter2 = ReflectionUtilities.EmitFieldGetter<Class1, int>("Field2");
        var c = new Class1();
        Assert.AreEqual(33, getter1(c));
        Assert.AreEqual(66, getter2(c));

        var setter2 = ReflectionUtilities.EmitFieldSetter<Class1, int>("Field2");
        setter2(c, 99);
        Assert.AreEqual(99, getter2(c));

        // works on readonly fields!
        (var getter3, var setter3) = ReflectionUtilities.EmitFieldGetterAndSetter<Class1, int>("Field3");
        Assert.AreEqual(22, getter3(c));
        setter3(c, 44);
        Assert.AreEqual(44, getter3(c));
    }

    // FIXME: missing tests specifying 'returned' on method, property
    [Test]
    public void DeconstructAnonymousType()
    {
        var o = new { a = 1, b = "hello" };

        var getters = new Dictionary<string, Func<object, object>>();
        foreach (var prop in o.GetType().GetProperties())
        {
            getters[prop.Name] = ReflectionUtilities.EmitMethodUnsafe<Func<object, object>>(prop.GetMethod);
        }

        Assert.AreEqual(2, getters.Count);
        Assert.IsTrue(getters.ContainsKey("a"));
        Assert.IsTrue(getters.ContainsKey("b"));
        Assert.AreEqual(1, getters["a"](o));
        Assert.AreEqual("hello", getters["b"](o));
    }

    // these functions can be examined in eg DotPeek to understand IL works

    // box          [mscorlib]System.Int32
    public object GetIntValue(Class4 object4) => object4.IntValue;

    // unbox.any    [mscorlib]System.Int32
    public void SetIntValue(Class4 object4, object i) => object4.IntValue = (int)i;

    // castclass    [mscorlib]System.String
    public void SetStringValue(Class4 object4, object s) => object4.StringValue = (string)s;

    // conv.i4
    public void SetIntValue(Class4 object4, double d) => object4.IntValue = (int)d;

    // unbox.any    [mscorlib]System.Double
    // conv.i4
    public void SetIntValue2(Class4 object4, object d) => object4.IntValue = (int)(double)d;

    public void SetIntValue3(Class4 object4, object v)
    {
        if (v is int i)
        {
            object4.IntValue = i;
        }
        else
        {
            object4.IntValue = Convert.ToInt32(v);
        }
    }

    public void SetIntValue4(Class4 object4, object v)
    {
        if (v is int i)
        {
            object4.IntValue = i;
        }
        else
        {
            object4.IntValue = (int)Convert.ChangeType(v, typeof(int));
        }
    }

    // get field
    public int GetIntField(Class1 object1) => object1.Field1;

    // set field
    public void SetIntField(Class1 object1, int i) => object1.Field1 = i;

    public static class StaticClass1
    {
        public static void Method()
        {
        }
    }

    public class Class1
    {
        public readonly int Field3 = 22;

        public int Field1 = 33;
        private readonly int Field2 = 66;

        public Class1()
        {
        }

        public Class1(int i)
        {
        }

        public int Value1 => 42;

        public int Value2
        {
            set { }
        }

        public int Value3
        {
            get => 42;
            set { }
        }

        private int ValueP1 => 42;

        public void Method1()
        {
        }

        public void Method2(int i)
        {
        }

        public int Method3() => 42;

        public int Method4(string s) => int.Parse(s);

        public string Method5() => "foo";

        public static void SMethod1()
        {
        }

        public static void SMethod2(int i)
        {
        }

        public static int SMethod3() => 42;

        public static int SMethod4(string s) => int.Parse(s);

        private void MethodP1()
        {
        }

        private static void SMethodP1()
        {
        }
    }

    public class Class2
    {
    }

    public class Class2A : Class2
    {
    }

    public class Class3
    {
        public Class3(int i)
        {
        }

        private Class3(string s)
        {
        }
    }

    public class Class4
    {
        public int IntValue { get; set; }

        public string StringValue { get; set; }

        public Class2 ClassValue { get; set; }

        public Class2A ClassAValue { get; set; }
    }

    public class Class5 : Class4
    {
        [JsonProperty("intValue2")]
        public int IntValue2 { get; set; }

        public string StringValue2 { get; set; }

        public Class2 ClassValue2 { get; set; }
    }
}
