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

/// <summary>
/// Contains unit tests for the <see cref="ReflectionUtilities"/> class in the Umbraco Core.
/// </summary>
[TestFixture]
public class ReflectionUtilitiesTests
{
    /// <summary>
    /// Tests that the EmitConstructor method correctly emits constructors for various delegate types.
    /// </summary>
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

    /// <summary>
    /// Tests that the EmitConstructor method correctly emits constructors from ConstructorInfo.
    /// It verifies creation of instances with parameterless and parameterized constructors,
    /// and checks that invalid constructor signatures throw exceptions.
    /// </summary>
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

    /// <summary>
    /// Tests that EmitConstructor correctly emits a private constructor.
    /// </summary>
    [Test]
    public void EmitCtorEmitsPrivateCtor()
    {
        var ctor = ReflectionUtilities.EmitConstructor<Func<string, Class3>>();
        Assert.IsInstanceOf<Class3>(ctor("foo"));
    }

    /// <summary>
    /// Tests that EmitConstructor throws an InvalidOperationException if the constructor is not found.
    /// </summary>
    [Test]
    public void EmitCtorThrowsIfNotFound() =>
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitConstructor<Func<bool, Class3>>());

    /// <summary>
    /// Tests that EmitConstructor throws an ArgumentException when given an invalid constructor.
    /// </summary>
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

    /// <summary>
    /// Tests that EmitConstructor returns null when expected.
    /// </summary>
    [Test]
    public void EmitCtorReturnsNull() =>
        Assert.IsNull(ReflectionUtilities.EmitConstructor<Func<bool, Class3>>(false));

    /// <summary>
    /// Tests that the EmitMethod function correctly emits methods that operate on instance methods with various signatures.
    /// </summary>
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

    /// <summary>
    /// Tests that EmitMethod correctly emits static methods and allows invocation.
    /// </summary>
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

    /// <summary>
    /// Tests that EmitMethod correctly emits a static method.
    /// </summary>
    [Test]
    public void EmitMethodEmitsStaticStatic()
    {
        var method = ReflectionUtilities.EmitMethod<Action>(typeof(StaticClass1), "Method");
        method();
    }

    /// <summary>
    /// Tests that ReflectionUtilities.EmitMethod correctly emits delegates from MethodInfo instances.
    /// </summary>
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

    /// <summary>
    /// Tests that EmitMethod correctly emits delegates for private methods.
    /// </summary>
    [Test]
    public void EmitMethodEmitsPrivateMethod()
    {
        var class1 = new Class1();

        var method1 = ReflectionUtilities.EmitMethod<Action<Class1>>("MethodP1");
        method1(class1);

        var method2 = ReflectionUtilities.EmitMethod<Class1, Action>("SMethodP1");
        method2();
    }

    /// <summary>
    /// Tests that EmitMethod throws an InvalidOperationException when the method is not found.
    /// </summary>
    [Test]
    public void EmitMethodThrowsIfNotFound()
    {
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitMethod<Action<Class1>>("ZZZ"));
        Assert.Throws<InvalidOperationException>(() =>
            ReflectionUtilities.EmitMethod<Action<Class1, int, int>>("Method1"));
    }

    /// <summary>
    /// Tests that EmitMethod throws an ArgumentException if the provided method is invalid.
    /// </summary>
    [Test]
    public void EmitMethodThrowsIfInvalid()
    {
        var methodInfo = typeof(Class1).GetMethod("Method1", BindingFlags.Instance | BindingFlags.Public);
        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitMethod<Action<Class1, int, int>>(methodInfo));
    }

    /// <summary>
    /// Tests that EmitMethod returns null when the specified method does not exist.
    /// </summary>
    [Test]
    public void EmitMethodReturnsNull()
    {
        Assert.IsNull(ReflectionUtilities.EmitMethod<Action<Class1>>("ZZZ", false));
        Assert.IsNull(ReflectionUtilities.EmitMethod<Action<Class1, int, int>>("Method1", false));
    }

    /// <summary>
    /// Tests that the EmitProperty methods correctly emit getters and setters for properties.
    /// </summary>
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

    /// <summary>
    /// Verifies that property getter and setter delegates can be correctly emitted from <see cref="PropertyInfo"/> instances
    /// using the <see cref="ReflectionUtilities"/> methods. Ensures that the emitted delegates work for different properties
    /// and both get and set operations.
    /// </summary>
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

    /// <summary>
    /// Tests that EmitPropertyGetter correctly emits a getter for a private property.
    /// </summary>
    [Test]
    public void EmitPropertyEmitsPrivateProperty()
    {
        var class1 = new Class1();

        var getter1 = ReflectionUtilities.EmitPropertyGetter<Class1, int>("ValueP1");
        Assert.AreEqual(42, getter1(class1));
    }

    /// <summary>
    /// Tests that EmitPropertyGetter and EmitPropertySetter throw the expected exceptions when the property is not found or invalid.
    /// </summary>
    [Test]
    public void EmitPropertyThrowsIfNotFound()
    {
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitPropertyGetter<Class1, int>("Zalue1"));
        Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value2"));

        var propertyInfo = typeof(Class1).GetProperty("Value1");
        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitPropertySetter<Class1, int>(propertyInfo));
    }

    /// <summary>
    /// Tests that EmitPropertyGetter throws an ArgumentException when given an invalid property name.
    /// </summary>
    [Test]
    public void EmitPropertyThrowsIfInvalid() =>
        Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitPropertyGetter<Class1, string>("Value1"));

    /// <summary>
    /// Tests that EmitPropertyGetter returns null when the property does not exist.
    /// </summary>
    [Test]
    public void EmitPropertyReturnsNull()
    {
        Assert.IsNull(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Zalue1", false));
        Assert.IsNull(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value2", false));
    }

    /// <summary>
    /// Tests that property setters emitted via ReflectionUtilities can cast unsafe values correctly.
    /// This includes casting from object to the property type, handling reference types, boxed value types,
    /// and verifying that invalid casts throw the appropriate exceptions.
    /// </summary>
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

        // TODO: the code below runs fine with ReSharper test running within VisualStudio
        // but it crashes when running via vstest.console.exe - unless some settings are required?

        // converting works
        setterInt4(object4, 42.0);
        Assert.AreEqual(42, object4.IntValue);

        // unsafe is... unsafe
        Assert.Throws<FormatException>(() => setterInt4(object4, "foo"));
    }

    /// <summary>
    /// Verifies that a property setter generated via reflection can cast an input object to the property's declaring type
    /// and correctly assign the value, even when the property is defined on a base class and the value is of a derived type.
    /// Specifically tests that an object of a derived type can be set on a property declared in a base type using the emitted setter.
    /// </summary>
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

    /// <summary>
    /// Tests that the property setter can cast an unsafe object to the expected type.
    /// </summary>
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

    /// <summary>
    /// Verifies that property getters generated via <see cref="ReflectionUtilities.EmitPropertyGetter{T,TResult}"/> can correctly cast property values to the specified result type.
    /// This includes casting reference types to base types, boxing value types to <see cref="object"/>, and ensuring that an <see cref="ArgumentException"/> is thrown when an invalid cast is attempted.
    /// </summary>
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

    /// <summary>
    /// Verifies that the property getter emitted by <see cref="ReflectionUtilities.EmitPropertyGetter{T,TValue}"/> can correctly cast the declaring object type when retrieving a property value.
    /// Ensures that casting from <see cref="Class5"/> to <see cref="Class2"/> works as expected, and that attempting to cast from <see cref="Class3"/> to <see cref="Class2"/> throws an <see cref="ArgumentException"/>.
    /// </summary>
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

    /// <summary>
    /// Tests that property getters emitted by <see cref="ReflectionUtilities"/> correctly cast the returned value to <see cref="object"/>.
    /// Verifies correct behavior for different property types, including string, reference types, and value types, ensuring boxing occurs as needed.
    /// Also tests property getters across class hierarchies and validates integration with object extension methods such as <c>ToObjectDictionary</c>.
    /// </summary>
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

    /// <summary>
    /// Tests that EmitFieldGetter and EmitFieldSetter correctly emit delegates
    /// for getting and setting field values, including readonly fields.
    /// </summary>
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

    // TODO: missing tests specifying 'returned' on method, property
    /// <summary>
    /// Verifies that property getters can be dynamically created and invoked for an anonymous type
    /// using reflection utilities, ensuring correct deconstruction and value retrieval for each property.
    /// </summary>
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
    /// <summary>Gets the integer value from the specified Class4 object.</summary>
    /// <param name="object4">The Class4 object from which to get the integer value.</param>
    /// <returns>The integer value boxed as an object.</returns>
    public object GetIntValue(Class4 object4) => object4.IntValue;

    // unbox.any    [mscorlib]System.Int32
    /// <summary>
    /// Sets the <c>IntValue</c> property of the specified <see cref="Class4"/> instance to the provided value, casting it to <see cref="int"/>.
    /// </summary>
    /// <param name="object4">The <see cref="Class4"/> instance whose <c>IntValue</c> property will be set.</param>
    /// <param name="i">The value to assign to <c>IntValue</c>; will be cast to <see cref="int"/>.</param>
    public void SetIntValue(Class4 object4, object i) => object4.IntValue = (int)i;

    // castclass    [mscorlib]System.String
    /// <summary>
    /// Sets the StringValue property of the given Class4 instance to the specified string.
    /// </summary>
    /// <param name="object4">The Class4 instance whose StringValue property will be set.</param>
    /// <param name="s">The object to be cast to string and assigned.</param>
    public void SetStringValue(Class4 object4, object s) => object4.StringValue = (string)s;

    // conv.i4
    /// <summary>
    /// Sets the <c>IntValue</c> property of the given <see cref="Class4"/> object to the specified integer value.
    /// </summary>
    /// <param name="object4">The <see cref="Class4"/> instance whose <c>IntValue</c> property will be set.</param>
    /// <param name="d">The value to set, expected to be a double that will be cast to integer.</param>
    public void SetIntValue(Class4 object4, double d) => object4.IntValue = (int)d;

    // unbox.any    [mscorlib]System.Double
    // conv.i4
    /// <summary>Sets the IntValue property of the specified Class4 instance by casting the provided object to double and then to int.</summary>
    /// <param name="object4">The Class4 instance whose IntValue property will be set.</param>
    /// <param name="d">The object to be cast to double and then to int.</param>
    public void SetIntValue2(Class4 object4, object d) => object4.IntValue = (int)(double)d;

    /// <summary>
    /// Sets the IntValue property of the given Class4 object to the integer value represented by v.
    /// </summary>
    /// <param name="object4">The Class4 instance whose IntValue property will be set.</param>
    /// <param name="v">The value to convert to an integer and assign to IntValue.</param>
    /// <returns>The integer value of the field.</returns>
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

    /// <summary>
    /// Sets the IntValue property of the given Class4 object to the integer value represented by the object v.
    /// </summary>
    /// <param name="object4">The Class4 instance whose IntValue property will be set.</param>
    /// <param name="v">The value to convert to an integer and assign to IntValue.</param>
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
    /// <summary>
    /// Gets the integer field value from the specified Class1 object.
    /// </summary>
    /// <param name="object1">The Class1 object from which to get the field value.</param>
    /// <returns>The integer value of the field.</returns>
    public int GetIntField(Class1 object1) => object1.Field1;

    // set field
    /// <summary>Sets the integer field of the specified object.</summary>
    /// <param name="object1">The object whose field is to be set.</param>
    /// <param name="i">The integer value to set.</param>
    public void SetIntField(Class1 object1, int i) => object1.Field1 = i;

    /// <summary>
    /// A static class used for testing reflection utilities.
    /// </summary>
    public static class StaticClass1
    {
    /// <summary>
    /// Represents a static method in <see cref="StaticClass1"/> used for unit testing reflection utilities.
    /// </summary>
        public static void Method()
        {
        }
    }

    /// <summary>
    /// A helper class used for unit testing within <see cref="ReflectionUtilitiesTests"/>.
    /// </summary>
    public class Class1
    {
#pragma warning disable IDE1006 // Field names should begin with lower-case letter
        public readonly int Field3 = 22;
        public int Field1 = 33;
#pragma warning restore IDE1006 // Field names should begin with lower-case letter

    /// <summary>
    /// Initializes a new instance of the <see cref="Class1"/> class.
    /// </summary>
        public Class1()
        {
        }

    /// <summary>
    /// Initializes a new instance of the <see cref="Class1"/> class with the specified integer value.
    /// </summary>
    /// <param name="i">The integer value to initialize the class with.</param>
        public Class1(int i)
        {
        }

    /// <summary>
    /// Gets the value 1. Always returns 42.
    /// </summary>
        public int Value1 => 42;

    /// <summary>
    /// Sets the Value2 property. This is a set-only property.
    /// </summary>
        public int Value2
        {
            set { }
        }

        /// <summary>
        /// Gets or sets the <see cref="Value3"/> property. This property always returns the integer value 42 when accessed.
        /// </summary>
        public int Value3
        {
            get => 42;
            set { }
        }

        private int ValueP1 => 42;

    /// <summary>
    /// Represents a method with no implementation.
    /// </summary>
        public void Method1()
        {
        }

    /// <summary>
    /// A sample method that takes an integer parameter.
    /// </summary>
    /// <param name="i">The integer parameter.</param>
        public void Method2(int i)
        {
        }

    /// <summary>Returns the integer 42.</summary>
    /// <returns>An integer value of 42.</returns>
        public int Method3() => 42;

    /// <summary>
    /// Parses the specified string to an integer.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The integer value parsed from the string.</returns>
        public int Method4(string s) => int.Parse(s);

    /// <summary>Returns a string value.</summary>
    /// <returns>A string.</returns>
        public string Method5() => "foo";

    /// <summary>
    /// Represents a static method in the <see cref="Class1"/> class. This method currently has no implementation.
    /// </summary>
        public static void SMethod1()
        {
        }

    /// <summary>
    /// Static method that takes an integer parameter.
    /// </summary>
    /// <param name="i">The integer parameter.</param>
        public static void SMethod2(int i)
        {
        }

    /// <summary>
    /// Returns the integer value 42.
    /// </summary>
    /// <returns>The integer 42.</returns>
        public static int SMethod3() => 42;

    /// <summary>
    /// Parses the given string to an integer.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The integer parsed from the string.</returns>
        public static int SMethod4(string s) => int.Parse(s);

        private void MethodP1()
        {
        }

        private static void SMethodP1()
        {
        }
    }

    /// <summary>
    /// A helper class used within <see cref="ReflectionUtilitiesTests"/> to test reflection-related utilities.
    /// This class is intended for use in unit tests only.
    /// </summary>
    public class Class2
    {
    }

    /// <summary>
    /// Test fixture class used in <see cref="ReflectionUtilitiesTests"/>.
    /// </summary>
    public class Class2A : Class2
    {
    }

    /// <summary>
    /// A test class used within <see cref="ReflectionUtilitiesTests"/> to validate reflection-based utilities.
    /// This class serves as a sample type for unit testing reflection scenarios.
    /// </summary>
    public class Class3
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="Class3"/> class with the specified integer parameter.
    /// </summary>
    /// <param name="i">The integer value to initialize the class with.</param>
        public Class3(int i)
        {
        }

        private Class3(string s)
        {
        }
    }

    /// <summary>
    /// Test fixture class used in <see cref="ReflectionUtilitiesTests"/>.
    /// </summary>
    public class Class4
    {
    /// <summary>
    /// Gets or sets the integer value.
    /// </summary>
        public int IntValue { get; set; }

    /// <summary>
    /// Gets or sets the string value.
    /// </summary>
        public string StringValue { get; set; }

    /// <summary>
    /// Gets or sets the value of the <see cref="Class2"/> property associated with this instance of <see cref="Class4"/>.
    /// </summary>
        public Class2 ClassValue { get; set; }

    /// <summary>
    /// Gets or sets the value of type <see cref="Class2A"/> for this property.
    /// </summary>
        public Class2A ClassAValue { get; set; }
    }

    /// <summary>
    /// A test class used within <see cref="ReflectionUtilitiesTests"/> to verify reflection-related functionality.
    /// </summary>
    public class Class5 : Class4
    {
        /// <summary>
        /// Gets or sets the secondary integer value for the <see cref="Class5"/> instance.
        /// </summary>
        [JsonPropertyName("intValue2")]
        public int IntValue2 { get; set; }

    /// <summary>
    /// Gets or sets the value of the StringValue2 property.
    /// This property holds a string value for testing purposes in Class5.
    /// </summary>
        public string StringValue2 { get; set; }

    /// <summary>
    /// Gets or sets the value of type <see cref="Class2"/> for this property.
    /// </summary>
        public Class2 ClassValue2 { get; set; }
    }
}
