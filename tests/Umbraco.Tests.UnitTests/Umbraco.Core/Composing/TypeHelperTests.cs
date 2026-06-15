// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Composing;

/// <summary>
///     Tests for TypeHelper
/// </summary>
[TestFixture]
public class TypeHelperTests
{
    private class Base<T>
    {
    }

    private interface IBase<T>
    {
    }

    private interface IDerived<T> : IBase<T>
    {
    }

    private class Derived<T> : Base<T>, IBase<T>
    {
    }

    private class Derived2<T> : Derived<T>
    {
    }

    private class DerivedI<T> : IDerived<T>
    {
    }

    [Test]
    public void Is_Static_Class()
    {
        Assert.That(TypeHelper.IsStaticClass(typeof(TypeHelper)), Is.True);
        Assert.That(TypeHelper.IsStaticClass(typeof(TypeHelperTests)), Is.False);
    }

    [Test]
    public void Find_Common_Base_Class()
    {
        var t1 = TypeHelper.GetLowestBaseType(
            typeof(OleDbCommand),
            typeof(OdbcCommand),
            typeof(SqlCommand));
        Assert.That(t1.Success, Is.False);

        var t2 = TypeHelper.GetLowestBaseType(
            typeof(OleDbCommand),
            typeof(OdbcCommand),
            typeof(SqlCommand),
            typeof(Component));
        Assert.That(t2.Success, Is.True);
        Assert.That(t2.Result, Is.EqualTo(typeof(Component)));

        var t3 = TypeHelper.GetLowestBaseType(
            typeof(OleDbCommand),
            typeof(OdbcCommand),
            typeof(SqlCommand),
            typeof(Component),
            typeof(Component).BaseType);
        Assert.That(t3.Success, Is.True);
        Assert.That(t3.Result, Is.EqualTo(typeof(MarshalByRefObject)));

        var t4 = TypeHelper.GetLowestBaseType(
            typeof(OleDbCommand),
            typeof(OdbcCommand),
            typeof(SqlCommand),
            typeof(Component),
            typeof(Component).BaseType,
            typeof(int));
        Assert.That(t4.Success, Is.False);

        var t5 = TypeHelper.GetLowestBaseType(typeof(PropertyAliasDto));
        Assert.That(t5.Success, Is.True);
        Assert.That(t5.Result, Is.EqualTo(typeof(PropertyAliasDto)));
    }

    [Test]
    public void MatchTypesTest()
    {
        var bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(int), typeof(int), bindings), Is.True);
        Assert.That(bindings.Count, Is.EqualTo(0));

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(int), typeof(string), bindings), Is.False);
        Assert.That(bindings.Count, Is.EqualTo(0));

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(List<int>), typeof(IEnumerable), bindings), Is.True);
        Assert.That(bindings.Count, Is.EqualTo(0));

        var t1 = typeof(IList<>); // IList<>
        var a1 = t1.GetGenericArguments()[0]; // <T>
        t1 = t1.MakeGenericType(a1); // IList<T>
        var t2 = a1;

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(int), t2, bindings), Is.True);
        Assert.That(bindings, Has.Count.EqualTo(1));
        Assert.That(bindings["T"], Is.EqualTo(typeof(int)));

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(IList<int>), t1, bindings), Is.True);
        Assert.That(bindings, Has.Count.EqualTo(1));
        Assert.That(bindings["T"], Is.EqualTo(typeof(int)));

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(List<int>), typeof(IList<int>), bindings), Is.True);
        Assert.That(bindings.Count, Is.EqualTo(0));

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(List<int>), t1, bindings), Is.True);
        Assert.That(bindings, Has.Count.EqualTo(1));
        Assert.That(bindings["T"], Is.EqualTo(typeof(int)));

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(Dictionary<int, string>), typeof(IDictionary<,>), bindings), Is.True);
        Assert.That(bindings, Has.Count.EqualTo(2));
        Assert.That(bindings["TKey"], Is.EqualTo(typeof(int)));
        Assert.That(bindings["TValue"], Is.EqualTo(typeof(string)));

        t1 = typeof(IDictionary<,>); // IDictionary<,>
        a1 = t1.GetGenericArguments()[0]; // <TKey>
        t1 = t1.MakeGenericType(a1, a1); // IDictionary<TKey,TKey>

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(Dictionary<int, string>), t1, bindings), Is.False);

        bindings = new Dictionary<string, Type>();
        Assert.That(TypeHelper.MatchType(typeof(Dictionary<int, int>), t1, bindings), Is.True);
        Assert.That(bindings, Has.Count.EqualTo(1));
        Assert.That(bindings["TKey"], Is.EqualTo(typeof(int)));
    }

    [Test]
    public void MatchType()
    {
        // both are OK
        Assert.That(TypeHelper.MatchType(typeof(List<int>), typeof(IEnumerable<>)), Is.True);

        var t1 = typeof(IDictionary<,>); // IDictionary<,>
        var a1 = t1.GetGenericArguments()[0];
        t1 = t1.MakeGenericType(a1, a1); // IDictionary<T,T>

        // both are OK
        Assert.That(TypeHelper.MatchType(typeof(Dictionary<int, int>), t1), Is.True);

        Assert.That(TypeHelper.MatchType(typeof(Dictionary<int, string>), t1), Is.False);

        // these are all of there from Is_Assignable_To_Generic_Type
        Assert.That(TypeHelper.MatchType(typeof(Derived<int>), typeof(Base<>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(List<int>), typeof(IEnumerable<>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(Derived<int>), typeof(Derived<>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(Derived2<int>), typeof(Base<>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(DerivedI<int>), typeof(IBase<>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(Derived2<int>), typeof(IBase<>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(int?), typeof(Nullable<>)), Is.True);

        Assert.That(TypeHelper.MatchType(typeof(Derived<int>), typeof(object)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(Derived<int>), typeof(List<>)), Is.False);
        Assert.That(TypeHelper.MatchType(typeof(Derived<int>), typeof(IEnumerable<>)), Is.False);
        Assert.That(TypeHelper.MatchType(typeof(Derived<int>), typeof(Base<int>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(List<int>), typeof(IEnumerable<int>)), Is.True);
        Assert.That(TypeHelper.MatchType(typeof(int), typeof(Nullable<>)), Is.False);

        // This get's the "Type" from the Count extension method on IEnumerable<T>, however the type IEnumerable<T> isn't
        // IEnumerable<> and it is not a generic definition, this attempts to explain that:
        // http://blogs.msdn.com/b/haibo_luo/archive/2006/02/17/534480.aspx
        var genericEnumerableNonGenericDefinition = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Single(x => x.Name == "Count" && x.GetParameters().Count() == 1)
            .GetParameters()
            .Single()
            .ParameterType;

        Assert.That(TypeHelper.MatchType(typeof(List<int>), genericEnumerableNonGenericDefinition), Is.True);
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
        Assert.That(t, Is.Not.Null);
        Assert.That(t.IsGenericTypeDefinition, Is.True);
        Assert.That(t.Name, Is.EqualTo("IList`1"));
        Assert.That(t.FullName, Is.EqualTo("System.Collections.Generic.IList`1"));
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IList`1[T]"));

        t = typeof(IList<>);
        Assert.That(t.IsGenericTypeDefinition, Is.True);
        Assert.That(t.Name, Is.EqualTo("IList`1"));
        Assert.That(t.FullName, Is.EqualTo("System.Collections.Generic.IList`1"));
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IList`1[T]"));

        t = typeof(IDictionary<,>);
        Assert.That(t.IsGenericTypeDefinition, Is.True);
        Assert.That(t.Name, Is.EqualTo("IDictionary`2"));
        Assert.That(t.FullName, Is.EqualTo("System.Collections.Generic.IDictionary`2"));
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IDictionary`2[TKey,TValue]"));

        t = typeof(IDictionary<,>);
        t = t.MakeGenericType(t.GetGenericArguments()[0], t.GetGenericArguments()[1]);
        Assert.That(t.Name, Is.EqualTo("IDictionary`2"));
        Assert.That(t.FullName, Is.EqualTo("System.Collections.Generic.IDictionary`2"));
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IDictionary`2[TKey,TValue]"));

        t = typeof(IDictionary<,>);
        t = t.MakeGenericType(t.GetGenericArguments()[0], t.GetGenericArguments()[0]);
        Assert.That(t.IsGenericTypeDefinition, Is.False); // not anymore
        Assert.That(t.Name, Is.EqualTo("IDictionary`2"));
        Assert.That(t.FullName, Is.Null); // see note above
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IDictionary`2[TKey,TKey]"));

        t = typeof(IList<>);
        Assert.That(t.IsGenericTypeDefinition, Is.True);
        t = t.MakeGenericType(t.GetGenericArguments()[0]);
        Assert.That(t.Name, Is.EqualTo("IList`1"));
        Assert.That(t.FullName, Is.EqualTo("System.Collections.Generic.IList`1"));
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IList`1[T]"));

        t = typeof(IList<int>);
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IList`1[System.Int32]"));

        t = typeof(IDictionary<,>);
        t = t.MakeGenericType(typeof(int), t.GetGenericArguments()[1]);
        Assert.That(t.IsGenericTypeDefinition, Is.False); // not anymore
        Assert.That(t.Name, Is.EqualTo("IDictionary`2"));
        Assert.That(t.FullName, Is.Null); // see note above
        Assert.That(t.ToString(), Is.EqualTo("System.Collections.Generic.IDictionary`2[System.Int32,TValue]"));
    }

    /// <summary>
    ///     USED ONLY FOR TESTS ABOVE
    /// </summary>
    public class PropertyAliasDto
    {
        public string Alias { get; set; }
    }
}
