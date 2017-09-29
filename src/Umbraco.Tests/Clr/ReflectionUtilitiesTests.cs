using System;
using System.Reflection;
using Lucene.Net.Index;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Clr
{
    [TestFixture]
    public class ReflectionUtilitiesTests
    {
        [Test]
        public void EmitCtorEmits()
        {
            var ctor1 = ReflectionUtilities.EmitCtor<Func<Class1>>();
            Assert.IsInstanceOf<Class1>(ctor1());

            var ctor2 = ReflectionUtilities.EmitCtor<Func<object>>(declaring: typeof(Class1));
            Assert.IsInstanceOf<Class1>(ctor2());

            var ctor3 = ReflectionUtilities.EmitCtor<Func<int, Class3>>();
            Assert.IsInstanceOf<Class3>(ctor3(42));

            var ctor4 = ReflectionUtilities.EmitCtor<Func<int, object>>(declaring: typeof(Class3));
            Assert.IsInstanceOf<Class3>(ctor4(42));
        }

        [Test]
        public void EmitCtorEmitsFromInfo()
        {
            var ctorInfo = typeof(Class1).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, Array.Empty<Type>(), null);
            var ctor1 = ReflectionUtilities.EmitCtor<Func<Class1>>(ctorInfo);
            Assert.IsInstanceOf<Class1>(ctor1());

            ctorInfo = typeof(Class1).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, new[] { typeof(int) }, null);
            var ctor3 = ReflectionUtilities.EmitCtor<Func<int, object>>(ctorInfo);
            Assert.IsInstanceOf<Class1>(ctor3(42));

            Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitCtor<Func<string, object>>(ctorInfo));
        }

        [Test]
        public void EmitCtorEmitsPrivateCtor()
        {
            var ctor = ReflectionUtilities.EmitCtor<Func<string, Class3>>();
            Assert.IsInstanceOf<Class3>(ctor("foo"));
        }

        [Test]
        public void EmitCtorThrowsIfNotFound()
        {
            Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitCtor<Func<bool, Class3>>());
        }

        [Test]
        public void EmitCtorThrowsIfInvalid()
        {
            var ctorInfo = typeof(Class1).GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, Array.Empty<Type>(), null);
            Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitCtor<Func<Class2>>(ctorInfo));
        }

        [Test]
        public void EmitCtorReturnsNull()
        {
            Assert.IsNull(ReflectionUtilities.EmitCtor<Func<bool, Class3>>(false));
        }

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
            // static types cannot be used as type arguments
            //var method = ReflectionUtilities.EmitMethod<StaticClass1, Action>("Method");
            var method = ReflectionUtilities.EmitMethod<Action>(typeof (StaticClass1), "Method");
            method();
        }

        [Test]
        public void EmitMethodEmitsFromInfo()
        {
            var class1 = new Class1();

            var methodInfo = typeof (Class1).GetMethod("Method1", BindingFlags.Instance | BindingFlags.Public);
            var method1 = ReflectionUtilities.EmitMethod<Action<Class1>>(methodInfo);
            method1(class1);

            methodInfo = typeof(Class1).GetMethod("Method2", BindingFlags.Instance | BindingFlags.Public, null, new [] { typeof(int) }, null);
            var method2 = ReflectionUtilities.EmitMethod<Action<Class1, int>>(methodInfo);
            method2(class1, 42);

            methodInfo = typeof(Class1).GetMethod("Method3", BindingFlags.Instance | BindingFlags.Public);
            var method3 = ReflectionUtilities.EmitMethod<Func<Class1, int>>(methodInfo);
            Assert.AreEqual(42, method3(class1));

            methodInfo = typeof(Class1).GetMethod("Method4", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
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

            methodInfo = typeof(Class1).GetMethod("SMethod4", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
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
            Assert.Throws<InvalidOperationException>(() => ReflectionUtilities.EmitMethod<Action<Class1, int, int>>("Method1"));
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

            // this is not supported yet
            //var getter4 = ReflectionUtilities.EmitPropertyGetter<Class1, object>("Value1", returned: typeof(int));
            //Assert.AreEqual(42, getter1(class1));
        }

        [Test]
        public void EmitPropertyEmitsFromInfo()
        {
            var class1 = new Class1();

            var propertyInfo = typeof (Class1).GetProperty("Value1");
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
        public void EmitPropertyThrowsIfInvalid()
        {
            Assert.Throws<ArgumentException>(() => ReflectionUtilities.EmitPropertyGetter<Class1, string>("Value1"));
        }

        [Test]
        public void EmitPropertyReturnsNull()
        {
            Assert.IsNull(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Zalue1", false));
            Assert.IsNull(ReflectionUtilities.EmitPropertyGetter<Class1, int>("Value2", false));
        }

        // fixme - missing tests specifying 'returned' on method, property

        public static class StaticClass1
        {
            public static void Method() { }
        }

        public class Class1
        {
            public Class1() { }
            public Class1(int i) { }

            public void Method1() { }
            public void Method2(int i) { }
            public int Method3() => 42;
            public int Method4(string s) => int.Parse(s);

            public string Method5() => "foo";

            public static void SMethod1() { }
            public static void SMethod2(int i) { }
            public static int SMethod3() => 42;
            public static int SMethod4(string s) => int.Parse(s);

            private void MethodP1() { }
            private static void SMethodP1() { }

            public int Value1 => 42;
            public int Value2 { set { } }
            public int Value3 { get { return 42; } set { } }
            private int ValueP1 => 42;
        }

        public class Class2 { }

        public class Class3
        {
            public Class3(int i) { }

            private Class3(string s) { }
        }
    }
}
