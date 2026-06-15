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
        Assert.That(properties.Length, Is.EqualTo(1));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain(nameof(ITheBaseThing.TheBaseThingProperty)));
    }

    [Test]
    public void Get_Public_Properties_Of_Interface_Contains_Inherited_Properties()
    {
        var properties = typeof(ITheThing).GetPublicProperties();
        Assert.That(properties.Length, Is.EqualTo(2));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain(nameof(ITheBaseThing.TheBaseThingProperty)));
        Assert.That(propertyNames, Does.Contain(nameof(ITheThing.TheThingProperty)));
    }

    [Test]
    public void Can_Get_Public_Properties_Of_Class()
    {
        var properties = typeof(TheBaseThing).GetPublicProperties();
        Assert.That(properties.Length, Is.EqualTo(1));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain(nameof(TheBaseThing.TheBaseThingProperty)));
    }

    [Test]
    public void Get_Public_Properties_Of_Class_Contains_Inherited_Properties()
    {
        var properties = typeof(TheThing).GetPublicProperties();
        Assert.That(properties.Length, Is.EqualTo(3));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain(nameof(TheBaseThing.TheBaseThingProperty)));
        Assert.That(propertyNames, Does.Contain(nameof(TheThing.TheThingProperty)));
        Assert.That(propertyNames, Does.Contain(nameof(TheThing.TheExtraProperty)));
    }

    [Test]
    public void Get_All_Properties_Of_Class_Contains_Internal_Properties()
    {
        var properties = typeof(TheThing).GetAllProperties();
        Assert.That(properties.Length, Is.EqualTo(4));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain(nameof(TheBaseThing.TheBaseThingProperty)));
        Assert.That(propertyNames, Does.Contain(nameof(TheThing.TheThingProperty)));
        Assert.That(propertyNames, Does.Contain(nameof(TheThing.TheExtraProperty)));
        Assert.That(propertyNames, Does.Contain(nameof(TheThing.TheInternalProperty)));
    }

    [Test]
    public void Can_Get_Public_Methods_Of_Interface()
    {
        var methods = typeof(ITheBaseThing).GetPublicMethods();
        Assert.That(methods.Length, Is.EqualTo(2));

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.That( methodNames, Does.Contain($"get_{nameof(ITheBaseThing.TheBaseThingProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(ITheBaseThing.TheBaseThingMethod)));
    }

    [Test]
    public void Get_Public_Methods_Of_Interface_Contains_Inherited_Methods()
    {
        var methods = typeof(ITheThing).GetPublicMethods();
        Assert.That(methods.Length, Is.EqualTo(5));

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.That( methodNames, Does.Contain($"get_{nameof(ITheBaseThing.TheBaseThingProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(ITheBaseThing.TheBaseThingMethod)));
        Assert.That( methodNames, Does.Contain($"get_{nameof(ITheThing.TheThingProperty)}"));
        Assert.That( methodNames, Does.Contain($"set_{nameof(ITheThing.TheThingProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(ITheThing.TheThingMethod)));
    }

    [Test]
    public void Can_Get_Public_Methods_Of_Class()
    {
        var methods = typeof(TheBaseThing).GetPublicMethods();
        Assert.That(methods.Length, Is.EqualTo(3 + _publicObjectMethodNames.Length));

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheBaseThing.TheBaseThingProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheBaseThingMethod)));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheExtraMethod)));
        Assert.That(methodNames.ContainsAll(_publicObjectMethodNames), Is.True);
    }

    [Test]
    public void Get_Public_Methods_Of_Class_Contains_Inherited_Methods()
    {
        var methods = typeof(TheThing).GetPublicMethods();
        Assert.That(methods.Length, Is.EqualTo(7 + _publicObjectMethodNames.Length));

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheBaseThing.TheBaseThingProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheBaseThingMethod)));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheExtraMethod)));
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheThing.TheThingProperty)}"));
        Assert.That( methodNames, Does.Contain($"set_{nameof(TheThing.TheThingProperty)}"));
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheThing.TheExtraProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(TheThing.TheThingMethod)));
        Assert.That(methodNames.ContainsAll(_publicObjectMethodNames), Is.True);
    }

    [Test]
    public void Can_Get_All_Methods_Of_Class()
    {
        var methods = typeof(TheBaseThing).GetAllMethods();
        Assert.That(methods.Length, Is.EqualTo(4 + _publicObjectMethodNames.Length + _nonPublicObjectMethodNames.Length));

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheBaseThing.TheBaseThingProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheBaseThingMethod)));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheExtraMethod)));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheInternalMethod)));
        Assert.That(methodNames.ContainsAll(_publicObjectMethodNames), Is.True);
        Assert.That(methodNames.ContainsAll(_nonPublicObjectMethodNames), Is.True);
    }

    [Test]
    public void Get_All_Methods_Of_Class_Contains_Inherited_Methods()
    {
        var methods = typeof(TheThing).GetAllMethods();
        Assert.That(methods.Length, Is.EqualTo(9 + _publicObjectMethodNames.Length + _nonPublicObjectMethodNames.Length));

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheBaseThing.TheBaseThingProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheBaseThingMethod)));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheExtraMethod)));
        Assert.That( methodNames, Does.Contain(nameof(TheBaseThing.TheInternalMethod)));
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheThing.TheThingProperty)}"));
        Assert.That( methodNames, Does.Contain($"set_{nameof(TheThing.TheThingProperty)}"));
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheThing.TheExtraProperty)}"));
        Assert.That( methodNames, Does.Contain($"get_{nameof(TheThing.TheInternalProperty)}"));
        Assert.That( methodNames, Does.Contain(nameof(TheThing.TheThingMethod)));
        Assert.That(methodNames.ContainsAll(_publicObjectMethodNames), Is.True);
        Assert.That(methodNames.ContainsAll(_nonPublicObjectMethodNames), Is.True);
    }

    [Test]
    public void Can_Get_Public_Properties_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetPublicProperties();
        Assert.That(properties.Length, Is.EqualTo(1));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain(nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty)));
    }

    [Test]
    public void Can_Get_All_Properties_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetAllProperties();
        Assert.That(properties.Length, Is.EqualTo(2));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain(nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty)));
        Assert.That(propertyNames, Does.Contain(nameof(ITheInterfaceWithInternalDeclarations.TheInternalProperty)));
    }

    [Test]
    public void Can_Get_Public_Methods_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetPublicMethods();
        Assert.That(properties.Length, Is.EqualTo(2));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain($"get_{nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty)}"));
        Assert.That(propertyNames, Does.Contain(nameof(ITheInterfaceWithInternalDeclarations.ThePublicMethod)));
    }

    [Test]
    public void Can_Get_All_Methods_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetAllMethods();
        Assert.That(properties.Length, Is.EqualTo(4));

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.That(propertyNames, Does.Contain($"get_{nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty)}"));
        Assert.That(propertyNames, Does.Contain($"get_{nameof(ITheInterfaceWithInternalDeclarations.TheInternalProperty)}"));
        Assert.That(propertyNames, Does.Contain(nameof(ITheInterfaceWithInternalDeclarations.ThePublicMethod)));
        Assert.That(propertyNames, Does.Contain(nameof(ITheInterfaceWithInternalDeclarations.TheInternalMethod)));
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
