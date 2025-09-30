using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class TypeExtensionsTests
{
    private readonly string[] _publicObjectMethodNames = { nameof(GetType), nameof(ToString), nameof(Equals), nameof(GetHashCode) };

    private readonly string[] _nonPublicObjectMethodNames = { nameof(MemberwiseClone), "Finalize" };

    [Test]
    public void Can_Get_Public_Properties_Of_Interface()
    {
        var properties = typeof(ITheBaseThing).GetPublicProperties();
        Assert.AreEqual(1, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheBaseThing.TheBaseThingProperty), propertyNames);
    }

    [Test]
    public void Get_Public_Properties_Of_Interface_Contains_Inherited_Properties()
    {
        var properties = typeof(ITheThing).GetPublicProperties();
        Assert.AreEqual(2, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheBaseThing.TheBaseThingProperty), propertyNames);
        Assert.Contains(nameof(ITheThing.TheThingProperty), propertyNames);
    }

    [Test]
    public void Can_Get_Public_Properties_Of_Class()
    {
        var properties = typeof(TheBaseThing).GetPublicProperties();
        Assert.AreEqual(1, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(TheBaseThing.TheBaseThingProperty), propertyNames);
    }

    [Test]
    public void Get_Public_Properties_Of_Class_Contains_Inherited_Properties()
    {
        var properties = typeof(TheThing).GetPublicProperties();
        Assert.AreEqual(3, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(TheBaseThing.TheBaseThingProperty), propertyNames);
        Assert.Contains(nameof(TheThing.TheThingProperty), propertyNames);
        Assert.Contains(nameof(TheThing.TheExtraProperty), propertyNames);
    }

    [Test]
    public void Get_All_Properties_Of_Class_Contains_Internal_Properties()
    {
        var properties = typeof(TheThing).GetAllProperties();
        Assert.AreEqual(4, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(TheBaseThing.TheBaseThingProperty), propertyNames);
        Assert.Contains(nameof(TheThing.TheThingProperty), propertyNames);
        Assert.Contains(nameof(TheThing.TheExtraProperty), propertyNames);
        Assert.Contains(nameof(TheThing.TheInternalProperty), propertyNames);
    }

    [Test]
    public void Can_Get_Public_Methods_Of_Interface()
    {
        var methods = typeof(ITheBaseThing).GetPublicMethods();
        Assert.AreEqual(2, methods.Length);

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.Contains( $"get_{nameof(ITheBaseThing.TheBaseThingProperty)}", methodNames);
        Assert.Contains( nameof(ITheBaseThing.TheBaseThingMethod), methodNames);
    }

    [Test]
    public void Get_Public_Methods_Of_Interface_Contains_Inherited_Methods()
    {
        var methods = typeof(ITheThing).GetPublicMethods();
        Assert.AreEqual(5, methods.Length);

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.Contains( $"get_{nameof(ITheBaseThing.TheBaseThingProperty)}", methodNames);
        Assert.Contains( nameof(ITheBaseThing.TheBaseThingMethod), methodNames);
        Assert.Contains( $"get_{nameof(ITheThing.TheThingProperty)}", methodNames);
        Assert.Contains( $"set_{nameof(ITheThing.TheThingProperty)}", methodNames);
        Assert.Contains( nameof(ITheThing.TheThingMethod), methodNames);
    }

    [Test]
    public void Can_Get_Public_Methods_Of_Class()
    {
        var methods = typeof(TheBaseThing).GetPublicMethods();
        Assert.AreEqual(3 + _publicObjectMethodNames.Length, methods.Length);

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.Contains( $"get_{nameof(TheBaseThing.TheBaseThingProperty)}", methodNames);
        Assert.Contains( nameof(TheBaseThing.TheBaseThingMethod), methodNames);
        Assert.Contains( nameof(TheBaseThing.TheExtraMethod), methodNames);
        Assert.IsTrue(methodNames.ContainsAll(_publicObjectMethodNames));
    }

    [Test]
    public void Get_Public_Methods_Of_Class_Contains_Inherited_Methods()
    {
        var methods = typeof(TheThing).GetPublicMethods();
        Assert.AreEqual(7 + _publicObjectMethodNames.Length, methods.Length);

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.Contains( $"get_{nameof(TheBaseThing.TheBaseThingProperty)}", methodNames);
        Assert.Contains( nameof(TheBaseThing.TheBaseThingMethod), methodNames);
        Assert.Contains( nameof(TheBaseThing.TheExtraMethod), methodNames);
        Assert.Contains( $"get_{nameof(TheThing.TheThingProperty)}", methodNames);
        Assert.Contains( $"set_{nameof(TheThing.TheThingProperty)}", methodNames);
        Assert.Contains( $"get_{nameof(TheThing.TheExtraProperty)}", methodNames);
        Assert.Contains( nameof(TheThing.TheThingMethod), methodNames);
        Assert.IsTrue(methodNames.ContainsAll(_publicObjectMethodNames));
    }

    [Test]
    public void Can_Get_All_Methods_Of_Class()
    {
        var methods = typeof(TheBaseThing).GetAllMethods();
        Assert.AreEqual(4 + _publicObjectMethodNames.Length + _nonPublicObjectMethodNames.Length, methods.Length);

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.Contains( $"get_{nameof(TheBaseThing.TheBaseThingProperty)}", methodNames);
        Assert.Contains( nameof(TheBaseThing.TheBaseThingMethod), methodNames);
        Assert.Contains( nameof(TheBaseThing.TheExtraMethod), methodNames);
        Assert.Contains( nameof(TheBaseThing.TheInternalMethod), methodNames);
        Assert.IsTrue(methodNames.ContainsAll(_publicObjectMethodNames));
        Assert.IsTrue(methodNames.ContainsAll(_nonPublicObjectMethodNames));
    }

    [Test]
    public void Get_All_Methods_Of_Class_Contains_Inherited_Methods()
    {
        var methods = typeof(TheThing).GetAllMethods();
        Assert.AreEqual(9 + _publicObjectMethodNames.Length + _nonPublicObjectMethodNames.Length, methods.Length);

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.Contains( $"get_{nameof(TheBaseThing.TheBaseThingProperty)}", methodNames);
        Assert.Contains( nameof(TheBaseThing.TheBaseThingMethod), methodNames);
        Assert.Contains( nameof(TheBaseThing.TheExtraMethod), methodNames);
        Assert.Contains( nameof(TheBaseThing.TheInternalMethod), methodNames);
        Assert.Contains( $"get_{nameof(TheThing.TheThingProperty)}", methodNames);
        Assert.Contains( $"set_{nameof(TheThing.TheThingProperty)}", methodNames);
        Assert.Contains( $"get_{nameof(TheThing.TheExtraProperty)}", methodNames);
        Assert.Contains( $"get_{nameof(TheThing.TheInternalProperty)}", methodNames);
        Assert.Contains( nameof(TheThing.TheThingMethod), methodNames);
        Assert.IsTrue(methodNames.ContainsAll(_publicObjectMethodNames));
        Assert.IsTrue(methodNames.ContainsAll(_nonPublicObjectMethodNames));
    }

    [Test]
    public void Can_Get_Public_Properties_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetPublicProperties();
        Assert.AreEqual(1, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty), propertyNames);
    }

    [Test]
    public void Can_Get_All_Properties_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetAllProperties();
        Assert.AreEqual(2, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty), propertyNames);
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.TheInternalProperty), propertyNames);
    }

    [Test]
    public void Can_Get_Public_Methods_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetPublicMethods();
        Assert.AreEqual(2, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains($"get_{nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty)}", propertyNames);
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.ThePublicMethod), propertyNames);
    }

    [Test]
    public void Can_Get_All_Methods_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetAllMethods();
        Assert.AreEqual(4, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains($"get_{nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty)}", propertyNames);
        Assert.Contains($"get_{nameof(ITheInterfaceWithInternalDeclarations.TheInternalProperty)}", propertyNames);
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.ThePublicMethod), propertyNames);
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.TheInternalMethod), propertyNames);
    }

    public interface ITheThing : ITheBaseThing
    {
        string TheThingProperty { get; set; }

        int TheThingMethod(int input);
    }

    public interface ITheBaseThing
    {
        string TheBaseThingProperty { get; }

        int TheBaseThingMethod();
    }

    public class TheThing : TheBaseThing, ITheThing
    {
        public string TheThingProperty { get; set; }

        public int TheThingMethod(int input) => throw new NotImplementedException();

        public bool TheExtraProperty { get; }

        internal decimal TheInternalProperty { get; }
    }

    public class TheBaseThing : ITheBaseThing
    {
        public string TheBaseThingProperty { get; }

        public int TheBaseThingMethod() => throw new NotImplementedException();

        public void TheExtraMethod() => throw new NotImplementedException();

        internal void TheInternalMethod() => throw new NotImplementedException();
    }

    // it's not pretty, but it is possible to declare internal properties and methods in a public interface... we need to test those as well :/
    public interface ITheInterfaceWithInternalDeclarations
    {
        public int ThePublicProperty { get; }

        internal int TheInternalProperty { get; }

        public string ThePublicMethod();

        internal string TheInternalMethod();
    }
}
