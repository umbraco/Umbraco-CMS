using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Contains unit tests for the <see cref="TypeExtensions"/> class, which provides extension methods for <see cref="System.Type"/>.
/// </summary>
[TestFixture]
public class TypeExtensionsTests
{
    private readonly string[] _publicObjectMethodNames = { nameof(GetType), nameof(ToString), nameof(Equals), nameof(GetHashCode) };

    private readonly string[] _nonPublicObjectMethodNames = { nameof(MemberwiseClone), "Finalize" };

    /// <summary>
    /// Tests that the method to get public properties of an interface returns the correct properties.
    /// </summary>
    [Test]
    public void Can_Get_Public_Properties_Of_Interface()
    {
        var properties = typeof(ITheBaseThing).GetPublicProperties();
        Assert.AreEqual(1, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheBaseThing.TheBaseThingProperty), propertyNames);
    }

    /// <summary>
    /// Tests that getting public properties of an interface includes properties inherited from base interfaces.
    /// </summary>
    [Test]
    public void Get_Public_Properties_Of_Interface_Contains_Inherited_Properties()
    {
        var properties = typeof(ITheThing).GetPublicProperties();
        Assert.AreEqual(2, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheBaseThing.TheBaseThingProperty), propertyNames);
        Assert.Contains(nameof(ITheThing.TheThingProperty), propertyNames);
    }

    /// <summary>
    /// Tests that public properties of a class can be retrieved correctly.
    /// </summary>
    [Test]
    public void Can_Get_Public_Properties_Of_Class()
    {
        var properties = typeof(TheBaseThing).GetPublicProperties();
        Assert.AreEqual(1, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(TheBaseThing.TheBaseThingProperty), propertyNames);
    }

    /// <summary>
    /// Tests that the method GetPublicProperties returns all public properties of a class, including inherited properties.
    /// </summary>
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

    /// <summary>
    /// Tests that GetAllProperties returns all properties of a class, including internal properties.
    /// </summary>
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

    /// <summary>
    /// Tests that public methods of an interface can be retrieved correctly.
    /// </summary>
    [Test]
    public void Can_Get_Public_Methods_Of_Interface()
    {
        var methods = typeof(ITheBaseThing).GetPublicMethods();
        Assert.AreEqual(2, methods.Length);

        var methodNames = methods.Select(p => p.Name).ToArray();
        Assert.Contains( $"get_{nameof(ITheBaseThing.TheBaseThingProperty)}", methodNames);
        Assert.Contains( nameof(ITheBaseThing.TheBaseThingMethod), methodNames);
    }

    /// <summary>
    /// Tests that the GetPublicMethods extension returns all public methods of an interface including those inherited from base interfaces.
    /// </summary>
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

    /// <summary>
    /// Tests that all public methods of a class, including inherited ones, can be retrieved correctly.
    /// </summary>
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

    /// <summary>
    /// Tests that the method to get public methods of a class includes inherited methods.
    /// </summary>
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

    /// <summary>
    /// Verifies that all methods of a class can be retrieved, including inherited, public, and non-public methods.
    /// This ensures the GetAllMethods extension returns a comprehensive list of method names for the specified type.
    /// </summary>
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

    /// <summary>
    /// Tests that GetAllMethods returns all methods of a class including inherited methods.
    /// </summary>
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

    /// <summary>
    /// Tests that only public properties are retrieved from an interface even when it contains internal declarations.
    /// </summary>
    [Test]
    public void Can_Get_Public_Properties_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetPublicProperties();
        Assert.AreEqual(1, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty), propertyNames);
    }

    /// <summary>
    /// Tests that all properties of an interface, including those with internal declarations, can be retrieved.
    /// </summary>
    [Test]
    public void Can_Get_All_Properties_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetAllProperties();
        Assert.AreEqual(2, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty), propertyNames);
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.TheInternalProperty), propertyNames);
    }

    /// <summary>
    /// Tests that public methods of an interface with internal declarations can be retrieved correctly.
    /// </summary>
    [Test]
    public void Can_Get_Public_Methods_Of_Interface_With_Internal_Declarations()
    {
        var properties = typeof(ITheInterfaceWithInternalDeclarations).GetPublicMethods();
        Assert.AreEqual(2, properties.Length);

        var propertyNames = properties.Select(p => p.Name).ToArray();
        Assert.Contains($"get_{nameof(ITheInterfaceWithInternalDeclarations.ThePublicProperty)}", propertyNames);
        Assert.Contains(nameof(ITheInterfaceWithInternalDeclarations.ThePublicMethod), propertyNames);
    }

    /// <summary>
    /// Tests that all methods of an interface, including those with internal declarations, can be retrieved.
    /// </summary>
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

    /// <summary>
    /// Represents a test interface used in TypeExtensionsTests.
    /// </summary>
    public interface ITheThing : ITheBaseThing
    {
        string TheThingProperty { get; set; }

    /// <summary>
    /// Executes the operation defined by this method using the specified input value.
    /// </summary>
    /// <param name="input">The integer input value for the operation.</param>
    /// <returns>The integer result of the operation.</returns>
        int TheThingMethod(int input);
    }

    /// <summary>
    /// Represents the base interface for test objects used in <see cref="TypeExtensionsTests"/>.
    /// Serves as a marker or contract for derived test types.
    /// </summary>
    public interface ITheBaseThing
    {
    /// <summary>
    /// Gets the value of the TheBaseThingProperty.
    /// </summary>
        string TheBaseThingProperty { get; }

    /// <summary>
    /// Defines a method that returns an integer value specific to the implementation of <see cref="ITheBaseThing"/>.
    /// </summary>
    /// <returns>An integer value representing the result of the method.</returns>
        int TheBaseThingMethod();
    }

    /// <summary>
    /// A helper class used for unit testing in <see cref="TypeExtensionsTests"/>.
    /// </summary>
    public class TheThing : TheBaseThing, ITheThing
    {
    /// <summary>
    /// Gets or sets the value of TheThingProperty.
    /// </summary>
        public string TheThingProperty { get; set; }

    /// <summary>
    /// Represents a placeholder method that, when implemented, performs an operation on the specified input integer and returns an integer result.
    /// Currently, this method is not implemented and will throw a <see cref="NotImplementedException"/>.
    /// </summary>
    /// <param name="input">The input integer to process.</param>
    /// <returns>An integer result of the operation.</returns>
        public int TheThingMethod(int input) => throw new NotImplementedException();

    /// <summary>
    /// Gets a value indicating whether this instance has the extra property set.
    /// </summary>
        public bool TheExtraProperty { get; }

        internal decimal TheInternalProperty { get; }
    }

    /// <summary>
    /// Serves as the base class for unit tests related to type extension methods in the TypeExtensionsTests suite.
    /// </summary>
    public class TheBaseThing : ITheBaseThing
    {
    /// <summary>
    /// Gets the value of the TheBaseThingProperty.
    /// </summary>
        public string TheBaseThingProperty { get; }

    /// <summary>
    /// Placeholder method for TheBaseThing. This method is not implemented and will always throw a <see cref="NotImplementedException"/> when called.
    /// </summary>
    /// <returns>This method does not return a value; it always throws an exception.</returns>
        public int TheBaseThingMethod() => throw new NotImplementedException();

    /// <summary>
    /// An extra method for TheBaseThing that is not yet implemented.
    /// </summary>
        public void TheExtraMethod() => throw new NotImplementedException();

        internal void TheInternalMethod() => throw new NotImplementedException();
    }

    // it's not pretty, but it is possible to declare internal properties and methods in a public interface... we need to test those as well :/
    /// <summary>
    /// Defines an interface with internal declarations for testing purposes.
    /// </summary>
    public interface ITheInterfaceWithInternalDeclarations
    {
    /// <summary>
    /// Gets the value of the public property defined in the interface.
    /// </summary>
        public int ThePublicProperty { get; }

        internal int TheInternalProperty { get; }

    /// <summary>
    /// Executes the public method declared in the interface and returns a string result.
    /// </summary>
    /// <returns>The string result produced by the method implementation.</returns>
        public string ThePublicMethod();

        internal string TheInternalMethod();
    }
}
