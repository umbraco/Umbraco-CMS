using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Dynamics;

namespace Umbraco.Tests.DynamicsAndReflection
{
    [TestFixture]
    public class ExtensionMethodFinderTests
    {
        #region Tests Elements

        public class TestClass { }
        public class TestClass<T> : TestClass { }
        public class TestClassOfInt : TestClass<int> { }
        public class TestClassOfString : TestClass<string> { }

        public void TestMethod1(int value) { }
        public void TestMethod2<T>(T value) { }
        public void TestMethod3<T>(T value1, T value2) { }
        public void TestMethod4<T1, T2>(T1 value1, T2 value2) { }
        public void TestMethod5<T>(List<T> value) { }
        public void TestMethod6(int value) { }
        public void TestMethod6(string value) { }
        public void TestMethod7<T>(IList<T> value) { }
        public void TestMethod8<T>(IDictionary<T, T> value) { }

        public interface ITestDict<T> : IDictionary<T, T> { }

        #endregion

        #region Utilities

        private static readonly IRuntimeCacheProvider NullCache = new NullCacheProvider();

        private static MethodInfo FindExtensionMethod(Type thisType, object[] args, string name, bool argsContainsThis)
        {
            return ExtensionMethodFinder.FindExtensionMethod(NullCache, thisType, args, name, argsContainsThis);
        }

        #endregion

        #region Tests Set #1

        [Test]
        public void Find_Non_Overloaded_Method()
        {
            var class1 = new TestClass();

            var method = FindExtensionMethod(typeof(TestClass), new object[] { 1 }, "SimpleMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, 1 });

            method = FindExtensionMethod(typeof(TestClass), new object[] { "x" }, "SimpleMethod", false);
            Assert.IsNull(method);            
        }

        [Test]
        public void Find_SimpleOverloaded()
        {
            var class1 = new TestClass();

            var method = FindExtensionMethod(typeof(TestClass), new object[] { 1 }, "SimpleOverloadMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, 1 });

            method = FindExtensionMethod(typeof(TestClass), new object[] { "x" }, "SimpleOverloadMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, "x" });
        }

        [Test]
        public void Find_SimpleOverloaded_ArgsContainingThis()
        {
            var class1 = new TestClass();

            var method = FindExtensionMethod(typeof(TestClass), new object[] { class1, 1 }, "SimpleOverloadMethod", true);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, 1 });

            method = FindExtensionMethod(typeof(TestClass), new object[] { class1, "x" }, "SimpleOverloadMethod", true);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, "x" });
        }

        [Test]
        public void Find_NonOverloadedGenericEnumerable()
        {
            var class1 = Enumerable.Empty<TestClass>();

            var method = FindExtensionMethod(typeof(IEnumerable<TestClass>), new object[] { 1 }, "SimpleEnumerableGenericMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, 1 });

            method = FindExtensionMethod(typeof(IEnumerable<TestClass>), new object[] { "x" }, "SimpleEnumerableGenericMethod", false);
            Assert.IsNull(method);       
        }

        [Test]
        public void Find_OverloadedGenericEnumerable()
        {
            var class1 = Enumerable.Empty<TestClass>();

            var method = FindExtensionMethod(typeof(IEnumerable<TestClass>), new object[] { 1 }, "SimpleOverloadEnumerableGenericMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, 1 });

            method = FindExtensionMethod(typeof(IEnumerable<TestClass>), new object[] { "x" }, "SimpleOverloadEnumerableGenericMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1, "x" });
        }

        [Test]
        public void Find_InheritedType()
        {
            var genericTestClass = new TestClass<TestClass>();
            var nonGenericTestClass = new TestClass();

            // not really testing "generics" here, just inheritance

            var method = FindExtensionMethod(typeof(TestClass), new object[] { genericTestClass }, "GenericParameterMethod", false);
            Assert.IsNotNull(method);

            method = FindExtensionMethod(typeof(TestClass), new object[] { nonGenericTestClass }, "GenericParameterMethod", false);
            Assert.IsNotNull(method);
        }

        [Test]
        public void Find_TrueGeneric()
        {
            var c = new TestClass<int>();

            var method = FindExtensionMethod(c.GetType(), new object[] { }, "GenericMethod", false);
            Assert.IsNotNull(method);
        }

        [Test]
        public void GetMethodVsGetMethods()
        {
            Assert.Throws<AmbiguousMatchException>(() =>
            {
                var m = typeof (ExtensionMethodFinderTests).GetMethod("TestMethod6");
            });

            var ms = typeof (ExtensionMethodFinderTests).GetMethods().Where(x => x.Name == "TestMethod6");
            Assert.AreEqual(2, ms.Count());
        }

        #endregion

        #region Tests Set #2 - Working with Generics

        // To expand on Jon's answer, the reason this doesn't work is because in regular,
        // non-dynamic code extension methods work by doing a full search of all the
        // classes known to the compiler for a static class that has an extension method
        // that match. The search goes in order based on the namespace nesting and available
        // "using" directives in each namespace.
        //
        // That means that in order to get a dynamic extension method invocation resolved
        // correctly, somehow the DLR has to know at runtime what all the namespace nestings
        // and "using" directives were in your source code. We do not have a mechanism handy
        // for encoding all that information into the call site. We considered inventing
        // such a mechanism, but decided that it was too high cost and produced too much
        // schedule risk to be worth it.
        //
        // Eric Lippert, http://stackoverflow.com/questions/5311465/extension-method-and-dynamic-object-in-c-sharp
        //
        // And so...
        // Obviously MatchType is broken and incomplete, it does not handle
        // - ref & out parameters
        // - array types
        // - structs
        // - generics constraints
        // - generics variance
        // - ...

        [Test]
        public void Temp()
        {
            var t1 = typeof (IList<int>);
            var t2 = typeof (IList<>);
            Assert.IsTrue(t2.IsGenericTypeDefinition);
            Assert.AreEqual(t2, t1.GetGenericTypeDefinition());
            var m = typeof (ExtensionMethodFinderTests).GetMethod("TestMethod7");
            var parms = m.GetParameters();
            Assert.AreEqual(1, parms.Length);
            var parm = parms[0];
            var t3 = parm.ParameterType; // IList<T>
            Assert.AreEqual(t2, t3.GetGenericTypeDefinition());

            Assert.AreEqual(typeof (int), t1.GetGenericArguments()[0]);
            Assert.IsFalse(t1.GetGenericArguments()[0].IsGenericParameter);
            //Assert.AreEqual(???, t2.GetGenericArguments()[0]);
            Assert.IsTrue(t2.GetGenericArguments()[0].IsGenericParameter);
            Assert.AreEqual("T", t2.GetGenericArguments()[0].Name);
            Assert.IsTrue(t3.GetGenericArguments()[0].IsGenericParameter);
            Assert.AreEqual("T", t3.GetGenericArguments()[0].Name);
        }

        

        [Test]
        public void Find_Generic_Enumerable_Method()
        {
            MethodInfo method;
            var class1 = Enumerable.Empty<TestClass>();

            method = ExtensionMethodFinder.FindExtensionMethod(new NullCacheProvider(), typeof(IEnumerable<TestClass>), new object[] { }, "GenericMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class1 });

            var class2 = new TestClassCollection();

            method = ExtensionMethodFinder.FindExtensionMethod(new NullCacheProvider(), typeof(TestClassCollection), new object[] { }, "GenericMethod", false);
            Assert.IsNotNull(method);
            method.Invoke(null, new object[] { class2 });
        }      

        [Ignore("This is just testing the below GetMethodForArguments method - Stephen was working on this but it's not used in the core")]
        [Test]
        public void TypesTests()
        {
            Assert.IsTrue(typeof(int[]).Inherits<int[]>());
            Assert.IsFalse(typeof(int[]).Inherits<bool[]>());

            var m1 = typeof(ExtensionMethodFinderTests).GetMethod("TestMethod1");

            var a1A = new object[] { 1 };
            var m1A = GetMethodForArguments(m1, a1A);
            Assert.IsNotNull(m1A);
            m1A.Invoke(this, a1A);

            var a1B = new object[] { "foo" };
            var m1B = GetMethodForArguments(m1, a1B);
            Assert.IsNull(m1B);

            var m2 = typeof(ExtensionMethodFinderTests).GetMethod("TestMethod2");

            var m2A = GetMethodForArguments(m2, a1A);
            Assert.IsNotNull(m2A);
            m2A.Invoke(this, a1A);

            var m2B = GetMethodForArguments(m2, a1B);
            Assert.IsNotNull(m2B);
            m2B.Invoke(this, a1B);

            var m3 = typeof(ExtensionMethodFinderTests).GetMethod("TestMethod3");

            var a3A = new object[] { 1, 2 };
            var m3A = GetMethodForArguments(m3, a3A);
            Assert.IsNotNull(m3A);
            m3A.Invoke(this, a3A);

            var a3B = new object[] { 1, "foo" };
            var m3B = GetMethodForArguments(m3, a3B);
            Assert.IsNull(m3B);

            var m4 = typeof(ExtensionMethodFinderTests).GetMethod("TestMethod4");

            var m4A = GetMethodForArguments(m4, a3A);
            Assert.IsNotNull(m4A);
            m4A.Invoke(this, a3A);

            var m4B = GetMethodForArguments(m4, a3B);
            Assert.IsNotNull(m4B);
            m4B.Invoke(this, a3B);

            var m5 = typeof(ExtensionMethodFinderTests).GetMethod("TestMethod5");

            // note - currently that fails because we can't match List<T> with List<int32>
            var a5 = new object[] { new List<int>() };
            var m5A = GetMethodForArguments(m5, a5);
            Assert.IsNotNull(m5A);

            // note - should we also handle "ref" and "out" parameters?
            // SD: NO, lets not make this more complicated than it already is
            // note - should we pay attention to array types?
            // SD: NO, lets not make this more complicated than it already is
        }

        // gets the method that can apply to the arguments
        // either the method itself, or a generic one
        // or null if it couldn't match
        //
        // this is a nightmare - if we want to do it right, then we have
        // to re-do the whole compiler type inference stuff by ourselves?!
        //
        static MethodInfo GetMethodForArguments(MethodInfo method, IList<object> arguments)
        {
            var parameters = method.GetParameters();
            var genericArguments = method.GetGenericArguments();

            if (parameters.Length != arguments.Count) return null;

            var genericArgumentTypes = new Type[genericArguments.Length];
            var i = 0;
            for (; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                var argumentType = arguments[i].GetType();

                Console.WriteLine("{0} / {1}", parameterType, argumentType);

                if (parameterType == argumentType) continue; // match
                if (parameterType.IsGenericParameter) // eg T
                {
                    var pos = parameterType.GenericParameterPosition;
                    if (genericArgumentTypes[pos] != null)
                    {
                        // note - is this OK? what about variance and such?
                        // it is NOT ok, if the first pass is SomethingElse then next is Something
                        // it will fail... the specs prob. indicate how it works, trying to find a common
                        // type...
                        if (genericArgumentTypes[pos].IsAssignableFrom(argumentType) == false)
                            break;
                    }
                    else
                    {
                        genericArgumentTypes[pos] = argumentType;
                    }
                }
                else if (parameterType.IsGenericType) // eg List<T>
                {
                    if (argumentType.IsGenericType == false) break;

                    var pg = parameterType.GetGenericArguments();
                    var ag = argumentType.GetGenericArguments();

                    // then what ?!
                    // should _variance_ be of some importance?
                    Console.WriteLine("generic {0}", argumentType.IsGenericType);
                }
                else
                {
                    if (parameterType.IsAssignableFrom(argumentType) == false)
                        break;
                }
            }
            if (i != parameters.Length) return null;
            return genericArguments.Length == 0
                ? method
                : method.MakeGenericMethod(genericArgumentTypes);
        }

        #endregion

        public class TestClassCollection : List<TestClass>
        {
            
        }

    }

    #region Tests Elements

        
    static class ExtensionMethodFinderTestsExtensions
    {
        public static void GenericMethod<T>(this IEnumerable<T> source)
        { }

        public static void SimpleMethod(this ExtensionMethodFinderTests.TestClass source, int value)
        { }

        public static void SimpleOverloadMethod(this ExtensionMethodFinderTests.TestClass source, int value)
        { }

        public static void SimpleOverloadMethod(this ExtensionMethodFinderTests.TestClass source, string value)
        { }

        public static void SimpleEnumerableGenericMethod(this IEnumerable<ExtensionMethodFinderTests.TestClass> source, int value)
        { }

        public static void SimpleOverloadEnumerableGenericMethod(this IEnumerable<ExtensionMethodFinderTests.TestClass> source, int value)
        { }

        public static void SimpleOverloadEnumerableGenericMethod(this IEnumerable<ExtensionMethodFinderTests.TestClass> source, string value)
        { }

        public static void GenericParameterMethod(this ExtensionMethodFinderTests.TestClass source, ExtensionMethodFinderTests.TestClass value)
        { }

        public static void GenericMethod<T>(this ExtensionMethodFinderTests.TestClass<T> source)
        { }
    }

    #endregion
}
