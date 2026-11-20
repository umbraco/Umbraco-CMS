// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

[TestFixture]
public class ExpressionValueEvaluatorTests
{
    private static string? _staticField = "static field";

    private static string StaticProperty => "static property";

    [Test]
    public void Can_Get_Value_Of_Constant()
        => Assert.AreEqual(42, ExpressionValueEvaluator.GetValue(Expression.Constant(42)));

    [Test]
    public void Can_Get_Value_Of_Null_Constant()
        => Assert.IsNull(ExpressionValueEvaluator.GetValue(Expression.Constant(null, typeof(string))));

    [Test]
    public void Can_Get_Value_Of_Captured_Local()
    {
        var id = 1234;
        Assert.AreEqual(1234, ExpressionValueEvaluator.GetValue(Body(() => id)));
    }

    [Test]
    public void Can_Get_Value_Of_Property_Of_Captured_Object()
    {
        var item = new Item { Name = "name" };
        Assert.AreEqual("name", ExpressionValueEvaluator.GetValue(Body(() => item.Name)));
    }

    [Test]
    public void Can_Get_Value_Of_Nested_Members_Of_Captured_Object()
    {
        var item = new Item { Child = new Item { Field = 7 } };
        Assert.AreEqual(7, ExpressionValueEvaluator.GetValue(Body(() => item.Child.Field)));
    }

    [Test]
    public void Can_Get_Value_Of_Static_Members()
    {
        Assert.AreEqual("static field", ExpressionValueEvaluator.GetValue(Body(() => _staticField)));
        Assert.AreEqual("static property", ExpressionValueEvaluator.GetValue(Body(() => StaticProperty)));
    }

    [Test]
    public void Can_Get_Value_Of_Captured_Nullable()
    {
        int? hasValue = 5;
        int? noValue = null;
        Assert.AreEqual(5, ExpressionValueEvaluator.GetValue(Body(() => hasValue)));
        Assert.IsNull(ExpressionValueEvaluator.GetValue(Body(() => noValue)));
    }

    [Test]
    public void Can_Get_Value_Of_Value_Converted_To_Nullable()
    {
        var id = 5;
        Expression<Func<int?>> expression = () => id;

        Assert.AreEqual(ExpressionType.Convert, expression.Body.NodeType);
        Assert.AreEqual(5, ExpressionValueEvaluator.GetValue(expression.Body));
    }

    [Test]
    public void Can_Get_Value_Of_Conversion_That_Changes_The_Value()
    {
        var kind = ItemKind.Second;
        Expression<Func<int>> expression = () => (int)kind;

        object? value = ExpressionValueEvaluator.GetValue(expression.Body);

        Assert.IsInstanceOf<int>(value);
        Assert.AreEqual(2, value);
    }

    [Test]
    public void Can_Get_Value_Of_Method_Call()
    {
        var item = new Item { Name = "name" };
        Assert.AreEqual("NAME", ExpressionValueEvaluator.GetValue(Body(() => item.Name!.ToUpperInvariant())));
    }

    [Test]
    public void Can_Get_Value_Of_Indexer()
    {
        var items = new List<string> { "a", "b" };
        Assert.AreEqual("b", ExpressionValueEvaluator.GetValue(Body(() => items[1])));
    }

    [Test]
    public void Can_Get_Value_Of_New_Object()
        => Assert.AreEqual(new DateTime(2020, 1, 2), ExpressionValueEvaluator.GetValue(Body(() => new DateTime(2020, 1, 2))));

    [Test]
    public void Exception_From_Property_Getter_Is_Not_Wrapped()
    {
        var item = new Item();
        Assert.Throws<ArgumentException>(() => ExpressionValueEvaluator.GetValue(Body(() => item.Throwing)));
    }

    [Test]
    public void Member_Access_On_Null_Throws_NullReferenceException()
    {
        Item? item = null;
        Assert.Throws<NullReferenceException>(() => ExpressionValueEvaluator.GetValue(Body(() => item!.Name)));
    }

    [Test]
    public void Expression_Using_A_Lambda_Parameter_Throws_InvalidOperationException()
    {
        Expression<Func<Item, string?>> expression = x => x.Name;
        Assert.Throws<InvalidOperationException>(() => ExpressionValueEvaluator.GetValue(expression.Body));
    }

    [Test]
    public void Does_Not_Compile_Code_Per_Evaluation()
    {
        var item = new Item { Name = "name", Child = new Item { Field = 1 } };

        JitCompilationAssert.DoesNotCompilePerRun(i =>
        {
            ExpressionValueEvaluator.GetValue(Body(() => i));
            ExpressionValueEvaluator.GetValue(Body(() => item.Child.Field));
            ExpressionValueEvaluator.GetValue(Body(() => item.Name!.ToUpperInvariant()));
        });
    }

    private static Expression Body<T>(Expression<Func<T>> expression) => expression.Body;

    private enum ItemKind
    {
        First = 1,
        Second = 2,
    }

    private class Item
    {
        public int Field;

        public string? Name { get; set; }

        public Item Child { get; set; } = null!;

        public string Throwing => throw new ArgumentException($"Thrown by the getter of {Name}.");
    }
}
