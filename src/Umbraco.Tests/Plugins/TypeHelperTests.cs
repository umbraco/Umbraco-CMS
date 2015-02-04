using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.DynamicsAndReflection;
using Umbraco.Web.Cache;
using Umbraco.Web.Models;
using Umbraco.Web.Scheduling;
using UmbracoExamine.DataServices;

namespace Umbraco.Tests.Plugins
{
    /// <summary>
    /// Tests for TypeHelper
    /// </summary>
    [TestFixture]
    public class TypeHelperTests
    {

        [Test]
        public void Is_Generic_Assignable()
        {
            var type1 = typeof (DynamicPublishedContentList);
            var type2 = typeof (IEnumerable<IPublishedContent>);
            var type3 = typeof(IQueryable<IPublishedContent>);
            var type4 = typeof(List<IPublishedContent>);
            var type5 = typeof(IEnumerable<>);

            Assert.IsTrue(TypeHelper.IsTypeAssignableFrom(type2, type1));
            Assert.IsFalse(TypeHelper.IsTypeAssignableFrom(type3, type1));
            Assert.IsTrue(TypeHelper.IsTypeAssignableFrom(type2, type4));

            //Will always fail which is correct, you cannot 'assign' IEnumerable<IPublishedContent> simply to IEnumerable<>
            //Assert.IsTrue(TypeHelper.IsTypeAssignableFrom(type5, type2));

            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(type5, type2));
        }

        [Test]
        public void Is_Assignable_To_Generic_Type()
        {
            //modified from: https://gist.github.com/klmr/4174727
            //using a version modified from: http://stackoverflow.com/a/1075059/1968

            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(typeof(Base<>), typeof(Derived<int>)));
            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(typeof(IEnumerable<>), typeof(List<int>)));
            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(typeof(Derived<>), typeof(Derived<int>)));
            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(typeof(Base<>), typeof(Derived2<int>)));
            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(typeof(IBase<>), typeof(DerivedI<int>)));
            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(typeof(IBase<>), typeof(Derived2<int>)));
            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(typeof(Nullable<>), typeof(int?)));

            Assert.IsFalse(TypeHelper.IsAssignableFromGeneric(typeof(Object), typeof(Derived<int>)));
            Assert.IsFalse(TypeHelper.IsAssignableFromGeneric(typeof(List<>), typeof(Derived<int>)));
            Assert.IsFalse(TypeHelper.IsAssignableFromGeneric(typeof(IEnumerable<>), typeof(Derived<int>)));
            Assert.IsFalse(TypeHelper.IsAssignableFromGeneric(typeof(Base<int>), typeof(Derived<int>)));
            Assert.IsFalse(TypeHelper.IsAssignableFromGeneric(typeof(IEnumerable<int>), typeof(List<int>)));
            Assert.IsFalse(TypeHelper.IsAssignableFromGeneric(typeof(Nullable<>), typeof(int)));

            //This get's the "Type" from the Count extension method on IEnumerable<T>, however the type IEnumerable<T> isn't
            // IEnumerable<> and it is not a generic definition, this attempts to explain that:
            // http://blogs.msdn.com/b/haibo_luo/archive/2006/02/17/534480.aspx

            var genericEnumerableNonGenericDefinition = typeof (Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Count" && x.GetParameters().Count() == 1)
                .GetParameters()
                .Single()
                .ParameterType;

            Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(genericEnumerableNonGenericDefinition, typeof(List<int>)));          
        }

        class Base<T> { }

        interface IBase<T> { }

        interface IDerived<T> : IBase<T> { }

        class Derived<T> : Base<T>, IBase<T> { }

        class Derived2<T> : Derived<T> { }

        class DerivedI<T> : IDerived<T> { }

        [Test]
        public void Is_Static_Class()
        {
            Assert.IsTrue(TypeHelper.IsStaticClass(typeof(TypeHelper)));
            Assert.IsFalse(TypeHelper.IsStaticClass(typeof(TypeHelperTests)));
        }

        [Test]
        public void Find_Common_Base_Class()
        {
            var t1 = TypeHelper.GetLowestBaseType(typeof (OleDbCommand),
                                                  typeof (OdbcCommand),
                                                  typeof (SqlCommand));
            Assert.IsFalse(t1.Success);

            var t2 = TypeHelper.GetLowestBaseType(typeof (OleDbCommand),
                                                  typeof (OdbcCommand),
                                                  typeof (SqlCommand),
                                                  typeof (Component));
            Assert.IsTrue(t2.Success);
            Assert.AreEqual(typeof(Component), t2.Result);

            var t3 = TypeHelper.GetLowestBaseType(typeof (OleDbCommand),
                                                  typeof (OdbcCommand),
                                                  typeof (SqlCommand),
                                                  typeof (Component),
                                                  typeof (Component).BaseType);
            Assert.IsTrue(t3.Success);
            Assert.AreEqual(typeof(MarshalByRefObject), t3.Result);

            var t4 = TypeHelper.GetLowestBaseType(typeof(OleDbCommand),
                                                   typeof(OdbcCommand),
                                                   typeof(SqlCommand),
                                                   typeof(Component),
                                                   typeof(Component).BaseType,
                                                   typeof(int));
            Assert.IsFalse(t4.Success);

            var t5 = TypeHelper.GetLowestBaseType(typeof(PropertyAliasDto));
            Assert.IsTrue(t5.Success);
            Assert.AreEqual(typeof(PropertyAliasDto), t5.Result);

            var t6 = TypeHelper.GetLowestBaseType(typeof (IApplicationEventHandler),
                                                  typeof (Scheduler),
                                                  typeof(CacheRefresherEventHandler));
            Assert.IsTrue(t6.Success);
            Assert.AreEqual(typeof(IApplicationEventHandler), t6.Result);

        }

        [Test]
        public void MatchTypesTest()
        {
            var bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(int), typeof(int), bindings));
            Assert.AreEqual(0, bindings.Count);

            bindings = new Dictionary<string, Type>();
            Assert.IsFalse(TypeHelper.MatchType(typeof(int), typeof(string), bindings));
            Assert.AreEqual(0, bindings.Count);

            bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(List<int>), typeof(System.Collections.IEnumerable), bindings));
            Assert.AreEqual(0, bindings.Count);

            var t1 = typeof(IList<>); // IList<>
            var a1 = t1.GetGenericArguments()[0]; // <T>
            t1 = t1.MakeGenericType(a1); // IList<T>
            var t2 = a1;

            bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(int), t2, bindings));
            Assert.AreEqual(1, bindings.Count);
            Assert.AreEqual(typeof(int), bindings["T"]);

            bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(IList<int>), t1, bindings));
            Assert.AreEqual(1, bindings.Count);
            Assert.AreEqual(typeof(int), bindings["T"]);

            bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(List<int>), typeof(IList<int>), bindings));
            Assert.AreEqual(0, bindings.Count);

            bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(List<int>), t1, bindings));
            Assert.AreEqual(1, bindings.Count);
            Assert.AreEqual(typeof(int), bindings["T"]);

            bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(Dictionary<int, string>), typeof(IDictionary<,>), bindings));
            Assert.AreEqual(2, bindings.Count);
            Assert.AreEqual(typeof(int), bindings["TKey"]);
            Assert.AreEqual(typeof(string), bindings["TValue"]);

            t1 = typeof(IDictionary<,>); // IDictionary<,>
            a1 = t1.GetGenericArguments()[0]; // <TKey>
            t1 = t1.MakeGenericType(a1, a1); // IDictionary<TKey,TKey>

            bindings = new Dictionary<string, Type>();
            Assert.IsFalse(TypeHelper.MatchType(typeof(Dictionary<int, string>), t1, bindings));

            bindings = new Dictionary<string, Type>();
            Assert.IsTrue(TypeHelper.MatchType(typeof(Dictionary<int, int>), t1, bindings));
            Assert.AreEqual(1, bindings.Count);
            Assert.AreEqual(typeof(int), bindings["TKey"]);
        }

        [Test]
        public void MatchType_Vs_IsAssignableFromGeneric()
        {
            // both are OK
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(List<int>), typeof(IEnumerable<>));

            var t1 = typeof (IDictionary<,>); // IDictionary<,>
            var a1 = t1.GetGenericArguments()[0];
            t1 = t1.MakeGenericType(a1, a1); // IDictionary<T,T>

            // both are OK
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(Dictionary<int, int>), t1);

            // TODO FIXME - IsAssignableFromGeneric returns true! does not manage generic parameters binding
            Assert_MatchType_Vs_IsAssignableFromGeneric(false, typeof(Dictionary<int, string>), t1);

            // these are all of there from Is_Assignable_To_Generic_Type
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(Derived<int>), typeof(Base<>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(List<int>), typeof(IEnumerable<>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(Derived<int>), typeof(Derived<>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(Derived2<int>), typeof(Base<>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(DerivedI<int>), typeof(IBase<>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(Derived2<int>), typeof(IBase<>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(int?), typeof(Nullable<>));

            // TODO FIXME - those that are marked below should actually be (true,...) but were false in Is_Assignable_To_Generic_Type

            // TODO FIXME - why would IsAssignableFromGeneric returns false? Derived<T> inherits from Object
            Assert_MatchType_Vs_IsAssignableFromGeneric(false, typeof(Derived<int>), typeof(Object));
            Assert_MatchType_Vs_IsAssignableFromGeneric(false, typeof(Derived<int>), typeof(List<>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(false, typeof(Derived<int>), typeof(IEnumerable<>));
            // TODO FIXME - why would IsAssignableFromGeneric returns false? Derived<T> inherits from Base<T>
            Assert_MatchType_Vs_IsAssignableFromGeneric(false, typeof(Derived<int>), typeof(Base<int>));
            // TODO FIXME - why would IsAssignableFromGeneric returns false? List<T> implements IEnumerable<T>
            Assert_MatchType_Vs_IsAssignableFromGeneric(false, typeof(List<int>), typeof(IEnumerable<int>));
            Assert_MatchType_Vs_IsAssignableFromGeneric(false, typeof(int), typeof(Nullable<>));

            //This get's the "Type" from the Count extension method on IEnumerable<T>, however the type IEnumerable<T> isn't
            // IEnumerable<> and it is not a generic definition, this attempts to explain that:
            // http://blogs.msdn.com/b/haibo_luo/archive/2006/02/17/534480.aspx

            var genericEnumerableNonGenericDefinition = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(x => x.Name == "Count" && x.GetParameters().Count() == 1)
                .GetParameters()
                .Single()
                .ParameterType;

            Assert_MatchType_Vs_IsAssignableFromGeneric(true, typeof(List<int>), genericEnumerableNonGenericDefinition);

        }

        private void Assert_MatchType_Vs_IsAssignableFromGeneric(bool expected, Type implementation, Type contract)
        {
            if (expected)
            {
                Assert.IsTrue(TypeHelper.MatchType(implementation, contract));
                Assert.IsTrue(TypeHelper.IsAssignableFromGeneric(contract, implementation));
            }
            else
            {
                Assert.IsFalse(TypeHelper.MatchType(implementation, contract));
                Assert.IsFalse(TypeHelper.IsAssignableFromGeneric(contract, implementation));
            }
        }

        [Test]
        public void CreateOpenGenericTypes()
        {
            // readings
            // http://stackoverflow.com/questions/13466078/create-open-constructed-type-from-string
            // http://stackoverflow.com/questions/6704722/c-sharp-language-how-to-get-type-of-bound-but-open-generic-class

            // note that FullName returns "The fully qualified name of the type, including its namespace but not its
            // assembly; or null if the current instance represents a generic type parameter, an array type, pointer
            // type, or byref type based on a type parameter, or a generic type that is not a generic type definition
            // but contains unresolved type parameters."

            var t = Type.GetType("System.Collections.Generic.IList`1");
            Assert.IsNotNull(t);
            Assert.IsTrue(t.IsGenericTypeDefinition);
            Assert.AreEqual("IList`1", t.Name);
            Assert.AreEqual("System.Collections.Generic.IList`1", t.FullName);
            Assert.AreEqual("System.Collections.Generic.IList`1[T]", t.ToString());

            t = typeof (IList<>);
            Assert.IsTrue(t.IsGenericTypeDefinition);
            Assert.AreEqual("IList`1", t.Name);
            Assert.AreEqual("System.Collections.Generic.IList`1", t.FullName);
            Assert.AreEqual("System.Collections.Generic.IList`1[T]", t.ToString());

            t = typeof(IDictionary<,>);
            Assert.IsTrue(t.IsGenericTypeDefinition);
            Assert.AreEqual("IDictionary`2", t.Name);
            Assert.AreEqual("System.Collections.Generic.IDictionary`2", t.FullName);
            Assert.AreEqual("System.Collections.Generic.IDictionary`2[TKey,TValue]", t.ToString());

            t = typeof(IDictionary<,>);
            t = t.MakeGenericType(t.GetGenericArguments()[0], t.GetGenericArguments()[1]); 
            Assert.AreEqual("IDictionary`2", t.Name);
            Assert.AreEqual("System.Collections.Generic.IDictionary`2", t.FullName);
            Assert.AreEqual("System.Collections.Generic.IDictionary`2[TKey,TValue]", t.ToString());

            t = typeof(IDictionary<,>);
            t = t.MakeGenericType(t.GetGenericArguments()[0], t.GetGenericArguments()[0]);
            Assert.IsFalse(t.IsGenericTypeDefinition); // not anymore
            Assert.AreEqual("IDictionary`2", t.Name);
            Assert.IsNull(t.FullName); // see note above
            Assert.AreEqual("System.Collections.Generic.IDictionary`2[TKey,TKey]", t.ToString());

            t = typeof(IList<>);
            Assert.IsTrue(t.IsGenericTypeDefinition);
            t = t.MakeGenericType(t.GetGenericArguments()[0]);
            Assert.AreEqual("IList`1", t.Name);
            Assert.AreEqual("System.Collections.Generic.IList`1", t.FullName);
            Assert.AreEqual("System.Collections.Generic.IList`1[T]", t.ToString());

            t = typeof(IList<int>);
            Assert.AreEqual("System.Collections.Generic.IList`1[System.Int32]", t.ToString());

            t = typeof (IDictionary<,>);
            t = t.MakeGenericType(typeof(int), t.GetGenericArguments()[1]);
            Assert.IsFalse(t.IsGenericTypeDefinition); // not anymore
            Assert.AreEqual("IDictionary`2", t.Name);
            Assert.IsNull(t.FullName); // see note above
            Assert.AreEqual("System.Collections.Generic.IDictionary`2[System.Int32,TValue]", t.ToString());
        }
    }
}