// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
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
        Assert.That(ctor1(), Is.InstanceOf<Class1>());

        var ctor2 = ReflectionUtilities.EmitConstructor<Func<object>>(declaring: typeof(Class1));
        Assert.That(ctor2(), Is.InstanceOf<Class1>());

        var ctor3 = ReflectionUtilities.EmitConstructor<Func<int, Class3>>();
        Assert.That(ctor3(42), Is.InstanceOf<Class3>());

        var ctor4 = ReflectionUtilities.EmitConstructor<Func<int, object>>(declaring: typeof(Class3));
        Assert.That(ctor4(42), Is.InstanceOf<Class3>());
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
        Assert.That(ctor1(), Is.InstanceOf<Class1>());

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
        Assert.That(ctor3(42), Is.InstanceOf<Class1>());

        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitConstructor<Func<string, object>>(ctorInfo));
    }

    [Test]
    public void EmitCtorEmitsPrivateCtor()
    {
        var ctor = ReflectionUtilities.EmitConstructor<Func<string, Class3>>();
        Assert.That(ctor("foo"), Is.InstanceOf<Class3>());
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
        Assert.That(ReflectionUtilities.EmitConstructor<Func<bool, Class3>>(false), Is.Null);

    [Test]
    public void EmitMethodEmitsInstance()
    {
        var class1 = new Class1();

        var method1 = ReflectionUtilities.EmitMethod<Action<Class1>>("Method1");
        method1(class1);

        var method2 = ReflectionUtilities.EmitMethod<Action<Class1, int>>("Method2");
        method2(class1, 42);

        var method3 = ReflectionUtilities.EmitMethod<Func<Class1, int>>("Method3");
        Assert.That(method3(class1), Is.EqualTo(42));

        var method4 = ReflectionUtilities.EmitMethod<Func<Class1, string, int>>("Method4");
        Assert.That(method4(class1, "42"), Is.EqualTo(42));
    }

    [Test]
    public void EmitMethodEmitsStatic()
    {
        var method1 = ReflectionUtilities.EmitMethod<Class1, Action>("SMethod1");
        method1();

        var method2 = ReflectionUtilities.EmitMethod<Class1, Action<int>>("SMethod2");
        method2(42);

        var method3 = ReflectionUtilities.EmitMethod<Class1, Func<int>>("SMethod3");
        Assert.That(method3(), Is.EqualTo(42));

        var method4 = ReflectionUtilities.EmitMethod<Class1, Func<string, int>>("SMethod4");
        Assert.That(method4("42"), Is.EqualTo(42));
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
        Assert.That(method3(class1), Is.EqualTo(42));

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
        Assert.That(method4(class1, "42"), Is.EqualTo(42));

        methodInfo = typeof(Class1).GetMethod("SMethod1", BindingFlags.Static | BindingFlags.Public);
        var smethod1 = ReflectionUtilities.EmitMethod<Action>(methodInfo);
        smethod1();

        methodInfo = typeof(Class1).GetMethod("SMethod2", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(int) }, null);
        var smethod2 = ReflectionUtilities.EmitMethod<Action<int>>(methodInfo);
        smethod2(42);

        methodInfo = typeof(Class1).GetMethod("SMethod3", BindingFlags.Static | BindingFlags.Public);
        var smethod3 = ReflectionUtilities.EmitMethod<Func<int>>(methodInfo);
        Assert.That(smethod3(), Is.EqualTo(42));

        methodInfo = typeof(Class1).GetMethod(
            "SMethod4",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[] { typeof(string) },
            null);
        var smethod4 = ReflectionUtilities.EmitMethod<Func<string, int>>(methodInfo);
        Assert.That(smethod4("42"), Is.EqualTo(42));

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
        Assert.That(ReflectionUtilities.EmitMethod<Action<Class1>>("ZZZ", false), Is.Null);
        Assert.That(ReflectionUtilities.EmitMethod<Action<Class1, int, int>>("Method1", false), Is.Null);
    }

    [Test]
    public void EmitPropertyEmits()
    {
        var class1 = new Class1();

        var getter1 = ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value1");
        Assert.That(getter1(class1), Is.EqualTo(42));

        var getter2 = ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value3");
        Assert.That(getter2(class1), Is.EqualTo(42));

        var setter1 = ReflectionUtilities.EmitPropertySetter<Class1, int>("Value2");
        setter1(class1, 42);

        var setter2 = ReflectionUtilities.EmitPropertySetter<Class1, int>("Value3");
        setter2(class1, 42);

        (var getter3, var setter3) = ReflectionUtilities.EmitPropertyGetterAndSetter<Class1, int>("Value3");
        Assert.That(getter3(class1), Is.EqualTo(42));
        setter3(class1, 42);
    }

    [Test]
    public void EmitPropertyEmitsFromInfo()
    {
        var class1 = new Class1();

        var propertyInfo = typeof(Class1).GetProperty("Value1");
        var getter1 = ReflectionUtilities.EmitPropertyGetter<Class1, int>(propertyInfo);
        Assert.That(getter1(class1), Is.EqualTo(42));

        propertyInfo = typeof(Class1).GetProperty("Value3");
        var getter2 = ReflectionUtilities.EmitPropertyGetter<Class1, int>(propertyInfo);
        Assert.That(getter2(class1), Is.EqualTo(42));

        propertyInfo = typeof(Class1).GetProperty("Value2");
        var setter1 = ReflectionUtilities.EmitPropertySetter<Class1, int>(propertyInfo);
        setter1(class1, 42);

        propertyInfo = typeof(Class1).GetProperty("Value3");
        var setter2 = ReflectionUtilities.EmitPropertySetter<Class1, int>(propertyInfo);
        setter2(class1, 42);

        (var getter3, var setter3) = ReflectionUtilities.EmitPropertyGetterAndSetter<Class1, int>(propertyInfo);
        Assert.That(getter3(class1), Is.EqualTo(42));
        setter3(class1, 42);
    }

    [Test]
    public void EmitPropertyEmitsPrivateProperty()
    {
        var class1 = new Class1();

        var getter1 = ReflectionUtilities.EmitPropertyGetter<Class1, int>("ValueP1");
        Assert.That(getter1(class1), Is.EqualTo(42));
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
        Assert.That(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Zalue1", false), Is.Null);
        Assert.That(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value2", false), Is.Null);
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
        Assert.That(propString4, Is.Not.Null);
        var setterString4 = ReflectionUtilities.EmitPropertySetterUnsafe<Class4, object>(propString4);
        Assert.That(setterString4, Is.Not.Null);
        setterString4(object4, "foo");
        Assert.That(object4.StringValue, Is.Not.Null);
        Assert.That(object4.StringValue, Is.EqualTo("foo"));

        // unsafe is... unsafe
        Assert.Throws<InvalidCastException>(() => setterString4(object4, new Class2()));

        // works with a reference property
        Assert.That(propClassA4, Is.Not.Null);
        var setterClassA4 = ReflectionUtilities.EmitPropertySetterUnsafe<Class4, object>(propClassA4);
        Assert.That(setterClassA4, Is.Not.Null);
        setterClassA4(object4, object2A);
        Assert.That(object4.ClassAValue, Is.Not.Null);
        Assert.That(object4.ClassAValue, Is.EqualTo(object2A));

        // works with a boxed value type
        Assert.That(propInt4, Is.Not.Null);
        var setterInt4 = ReflectionUtilities.EmitPropertySetterUnsafe<Class4, object>(propInt4);
        Assert.That(setterInt4, Is.Not.Null);

        setterInt4(object4, 42);
        Assert.That(object4.IntValue, Is.EqualTo(42));

        // TODO: the code below runs fine with ReSharper test running within VisualStudio
        // but it crashes when running via vstest.console.exe - unless some settings are required?

        // converting works
        setterInt4(object4, 42.0);
        Assert.That(object4.IntValue, Is.EqualTo(42));

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
        Assert.That(object4.ClassValue, Is.Not.Null);
        Assert.That(object4.ClassValue, Is.SameAs(object2));
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
        Assert.That(object4.ClassValue, Is.Not.Null);
        Assert.That(object4.ClassValue, Is.SameAs(object2));
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
        Assert.That(valueClass4A, Is.Not.Null);
        Assert.That(valueClass4A, Is.SameAs(object2A));

        // cannot cast the return type from Class2A to Class3!
        Assert.Throws<ArgumentException>(()
            => ReflectionUtilities.EmitPropertyGetter<Class4, Class3>(propClassA4));

        // can cast and box the return type from int to object
        var getterInt4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propInt4);

        var valueInt4 = getterInt4(object4);
        Assert.That(valueInt4 is int, Is.True);
        Assert.That(valueInt4, Is.EqualTo(159));

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
        Assert.That(valueClass4, Is.Not.Null);
        Assert.That(valueClass4, Is.SameAs(object2));

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
        Assert.That(propString4, Is.Not.Null);
        var getterString4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propString4);
        Assert.That(getterString4, Is.Not.Null);
        var valueString4 = getterString4(object4);
        Assert.That(valueString4, Is.Not.Null);
        Assert.That(valueString4, Is.EqualTo("foo"));

        // works with a reference property
        var propClass4 = type4.GetProperty("ClassValue");
        Assert.That(propClass4, Is.Not.Null);
        var getterClass4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propClass4);
        Assert.That(getterClass4, Is.Not.Null);
        var valueClass4 = getterClass4(object4);
        Assert.That(valueClass4, Is.Not.Null);
        Assert.That(valueClass4, Is.InstanceOf<Class2>());

        // works with a value type property
        var propInt4 = type4.GetProperty("IntValue");
        Assert.That(propInt4, Is.Not.Null);

        // ... if explicitly getting a value type
        var getterInt4T = ReflectionUtilities.EmitPropertyGetter<Class4, int>(propInt4);
        Assert.That(getterInt4T, Is.Not.Null);
        var valueInt4T = getterInt4T(object4);
        Assert.That(valueInt4T, Is.EqualTo(1));

        // ... if using a compiled getter
        var valueInt4D = GetIntValue(object4);
        Assert.That(valueInt4D, Is.Not.Null);
        Assert.That(valueInt4D is int, Is.True);
        Assert.That(valueInt4D, Is.EqualTo(1));

        // ... if getting a non-value type (emit adds a box)
        var getterInt4 = ReflectionUtilities.EmitPropertyGetter<Class4, object>(propInt4);
        Assert.That(getterInt4, Is.Not.Null);
        var valueInt4 = getterInt4(object4);
        Assert.That(valueInt4, Is.Not.Null);
        Assert.That(valueInt4 is int, Is.True);
        Assert.That(valueInt4, Is.EqualTo(1));

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

        Assert.That(values4, Has.Count.EqualTo(4));
        Assert.That(values4["StringValue"], Is.EqualTo("foo"));
        Assert.That(values4["ClassValue"], Is.InstanceOf<Class2>());
        Assert.That(values4["IntValue"], Is.EqualTo(1));

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

        Assert.That(values5, Has.Count.EqualTo(7));
        Assert.That(values5["StringValue"], Is.EqualTo("foo"));
        Assert.That(values5["ClassValue"], Is.InstanceOf<Class2>());
        Assert.That(values5["IntValue"], Is.EqualTo(1));
        Assert.That(values5["StringValue2"], Is.EqualTo("foo"));
        Assert.That(values5["ClassValue2"], Is.InstanceOf<Class2>());
        Assert.That(values5["IntValue2"], Is.EqualTo(1));

        // test object extensions
        Console.WriteLine("Getting object5D values...");
        var values5D = ObjectJsonExtensions.ToObjectDictionary(object5);

        Console.WriteLine("Writing object5D values...");
        foreach ((var name, var value) in values5)
        {
            Console.WriteLine($"{name}: {value}");
        }

        Assert.That(values5, Has.Count.EqualTo(7));
        Assert.That(values5D["StringValue"], Is.EqualTo("foo"));
        Assert.That(values5D["ClassValue"], Is.InstanceOf<Class2>());
        Assert.That(values5D["IntValue"], Is.EqualTo(1));
        Assert.That(values5D["StringValue2"], Is.EqualTo("foo"));
        Assert.That(values5D["ClassValue2"], Is.InstanceOf<Class2>());
        Assert.That(values5D["intValue2"], Is.EqualTo(1)); // JsonProperty changes property name
    }

    [Test]
    public void EmitFieldGetterSetterEmits()
    {
        var getter1 = ReflectionUtilities.EmitFieldGetter<Class1, int>("Field1");
        var getter2 = ReflectionUtilities.EmitFieldGetter<Class1, int>("Field2");
        var c = new Class1();
        Assert.That(getter1(c), Is.EqualTo(33));
        Assert.That(getter2(c), Is.EqualTo(66));

        var setter2 = ReflectionUtilities.EmitFieldSetter<Class1, int>("Field2");
        setter2(c, 99);
        Assert.That(getter2(c), Is.EqualTo(99));

        // works on readonly fields!
        (var getter3, var setter3) = ReflectionUtilities.EmitFieldGetterAndSetter<Class1, int>("Field3");
        Assert.That(getter3(c), Is.EqualTo(22));
        setter3(c, 44);
        Assert.That(getter3(c), Is.EqualTo(44));
    }

    // TODO: missing tests specifying 'returned' on method, property
    [Test]
    public void DeconstructAnonymousType()
    {
        var o = new { a = 1, b = "hello" };

        var getters = new Dictionary<string, Func<object, object>>();
        foreach (var prop in o.GetType().GetProperties())
        {
            getters[prop.Name] = ReflectionUtilities.EmitMethodUnsafe<Func<object, object>>(prop.GetMethod);
        }

        Assert.That(getters, Has.Count.EqualTo(2));
        Assert.That(getters.ContainsKey("a"), Is.True);
        Assert.That(getters.ContainsKey("b"), Is.True);
        Assert.That(getters["a"](o), Is.EqualTo(1));
        Assert.That(getters["b"](o), Is.EqualTo("hello"));
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
#pragma warning disable SA1306 // Field names should begin with lower-case letter
        private readonly int Field2 = 66;
#pragma warning restore SA1306 // Field names should begin with lower-case letter

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
        [JsonPropertyName("intValue2")]
        public int IntValue2 { get; set; }

        public string StringValue2 { get; set; }

        public Class2 ClassValue2 { get; set; }
    }
}
