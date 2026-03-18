// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the DeepCloneHelper class.
/// </summary>
[TestFixture]
public class DeepCloneHelperTests
{
    /// <summary>
    /// Tests that the DeepClone method correctly clones reference properties of an object.
    /// Verifies that some reference properties (like MyTest1) are deeply cloned, resulting in new instances,
    /// while others (like MyTest2) are referenced as the same instance in the clone.
    /// </summary>
    [Test]
    public void Deep_Clone_Ref_Properties()
    {
        var test1 = new Test1 { MyTest1 = new Test1(), MyTest2 = new Test2() };

        var clone = (Test1)test1.DeepClone();

        Assert.AreNotSame(test1, clone);
        Assert.AreNotSame(test1.MyTest1, clone.MyTest1);
        Assert.AreSame(test1.MyTest2, clone.MyTest2);
    }

    /// <summary>
    /// Tests that the DeepClone method correctly clones an array property,
    /// ensuring that the cloned array and its elements are not the same instances as the original.
    /// </summary>
    [Test]
    public void Deep_Clone_Array_Property()
    {
        var test1 = new Test3 { MyTest1 = new object[] { new Test1(), new Test1() } };

        var clone = (Test3)test1.DeepClone();

        Assert.AreNotSame(test1, clone);
        Assert.AreEqual(test1.MyTest1.Length, clone.MyTest1.Length);
        for (var i = 0; i < test1.MyTest1.Length; i++)
        {
            Assert.IsNotNull(clone.MyTest1.ElementAt(i));
            Assert.AreNotSame(clone.MyTest1.ElementAt(i), test1.MyTest1.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that the DeepClone method correctly clones a typed array property,
    /// ensuring that the cloned array and its elements are distinct instances from the original.
    /// </summary>
    [Test]
    public void Deep_Clone_Typed_Array_Property()
    {
        var test1 = new Test4 { MyTest1 = new[] { new Test1(), new Test1() } };

        var clone = (Test4)test1.DeepClone();

        Assert.AreNotSame(test1, clone);
        Assert.AreEqual(test1.MyTest1.Length, clone.MyTest1.Length);
        for (var i = 0; i < test1.MyTest1.Length; i++)
        {
            Assert.IsNotNull(clone.MyTest1.ElementAt(i));
            Assert.AreNotSame(clone.MyTest1.ElementAt(i), test1.MyTest1.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that the DeepClone method correctly clones an object with an enumerable property,
    /// ensuring that the cloned enumerable contains distinct instances from the original.
    /// </summary>
    [Test]
    public void Deep_Clone_Enumerable_Property()
    {
        var test1 = new Test5 { MyTest1 = new[] { new Test1(), new Test1() } };

        var clone = (Test5)test1.DeepClone();

        Assert.AreNotSame(test1, clone);

        Assert.AreEqual(test1.MyTest1.Cast<object>().Count(), clone.MyTest1.Cast<object>().Count());
        for (var i = 0; i < test1.MyTest1.Cast<object>().Count(); i++)
        {
            Assert.IsNotNull(clone.MyTest1.Cast<object>().ElementAt(i));
            Assert.AreNotSame(clone.MyTest1.Cast<object>().ElementAt(i), test1.MyTest1.Cast<object>().ElementAt(i));
        }
    }

    /// <summary>
    /// Verifies that deep cloning an object with a typed enumerable property
    /// creates a new instance of the enumerable and its elements, ensuring that
    /// the cloned enumerable and its items are not the same references as the originals.
    /// </summary>
    [Test]
    public void Deep_Clone_Typed_Enumerable_Property()
    {
        var test1 = new Test6 { MyTest1 = new[] { new Test1(), new Test1() } };

        var clone = (Test6)test1.DeepClone();

        Assert.AreNotSame(test1, clone);
        Assert.AreEqual(test1.MyTest1.Count(), clone.MyTest1.Count());
        for (var i = 0; i < test1.MyTest1.Count(); i++)
        {
            Assert.IsNotNull(clone.MyTest1.ElementAt(i));
            Assert.AreNotSame(clone.MyTest1.ElementAt(i), test1.MyTest1.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that a custom enumerable property is deep cloned correctly.
    /// </summary>
    [Test]
    public void Deep_Clone_Custom_Enumerable_Property()
    {
        var test1 = new Test7 { MyTest1 = new List<Test1> { new(), new() } };

        var clone = (Test7)test1.DeepClone();

        Assert.AreNotSame(test1, clone);
        Assert.AreEqual(test1.MyTest1.Count(), clone.MyTest1.Count());
        for (var i = 0; i < test1.MyTest1.Count(); i++)
        {
            Assert.IsNotNull(clone.MyTest1.ElementAt(i));
            Assert.AreNotSame(clone.MyTest1.ElementAt(i), test1.MyTest1.ElementAt(i));
        }
    }

    /// <summary>
    /// Tests that a deep clone of an object with a custom enumerable interface property
    /// correctly clones the enumerable and its elements, ensuring no references are shared.
    /// </summary>
    [Test]
    public void Deep_Clone_Custom_Enumerable_Interface_Property()
    {
        var test1 = new Test8 { MyTest1 = new List<Test1> { new(), new() } };

        var clone = (Test8)test1.DeepClone();

        Assert.AreNotSame(test1, clone);
        Assert.AreEqual(test1.MyTest1.Count(), clone.MyTest1.Count());
        for (var i = 0; i < test1.MyTest1.Count(); i++)
        {
            Assert.IsNotNull(clone.MyTest1.ElementAt(i));
            Assert.AreNotSame(clone.MyTest1.ElementAt(i), test1.MyTest1.ElementAt(i));
        }
    }

    /// <summary>
    /// Verifies that when deep cloning an object, collection properties containing non-cloneable elements are not deep cloned, and the original collection instance is preserved in the clone.
    /// </summary>
    [Test]
    public void Cannot_Deep_Clone_Collection_Properties_That_Are_Not_Cloneable()
    {
        var test1 = new Test3
        {
            MyTest1 = new object[]
            {
                new Test1(), "hello",

                // Not cloneable so this property will get skipped.
                new Test2(),
            },
        };

        var clone = (Test3)test1.DeepClone();

        // It skipped this property so these will now be the same.
        Assert.AreSame(clone.MyTest1, test1.MyTest1);
    }

    /// <summary>
    /// Verifies that the DeepCloneHelper correctly creates a deep copy of an object, ensuring all nested objects are cloned and not referenced.
    /// </summary>
    public class Test1 : BaseCloneable
    {
    /// <summary>
    /// Gets or sets the name of the <see cref="Test1"/> instance.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the age.
    /// </summary>
        public int Age { get; set; }

    /// <summary>
    /// Gets or sets the nested <see cref="Test1"/> instance for testing deep cloning.
    /// </summary>
        public Test1 MyTest1 { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Test2"/> instance associated with this <see cref="Test1"/>.
    /// </summary>
        public Test2 MyTest2 { get; set; }
    }

    /// <summary>
    /// Tests the deep cloning functionality of the DeepCloneHelper with a particular test case to ensure objects are cloned correctly.
    /// </summary>
    public class Test2
    {
    /// <summary>
    /// Gets or sets the name associated with this <see cref="Test2"/> instance.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets an instance of <see cref="Test1"/> associated with this <see cref="Test2"/>.
    /// </summary>
        public Test1 MyTest1 { get; set; }
    }

    /// <summary>
    /// Tests the deep cloning functionality of the DeepCloneHelper in a specific scenario, ensuring that all nested objects are properly cloned and references are not shared.
    /// </summary>
    public class Test3 : BaseCloneable
    {
    /// <summary>
    /// Gets or sets the name of this <see cref="Test3"/> instance.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the array of test objects used in the <c>Test3</c> class for deep clone testing.
    /// </summary>
        public object[] MyTest1 { get; set; }
    }

    /// <summary>
    /// Tests the deep cloning functionality of the DeepCloneHelper for complex objects.
    /// </summary>
    public class Test4 : BaseCloneable
    {
    /// <summary>
    /// Gets or sets the name of this <see cref="Test4"/> instance.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the MyTest1 array.
    /// </summary>
        public Test1[] MyTest1 { get; set; }
    }

    /// <summary>
    /// Tests the deep cloning functionality for the scenario covered by Test5, ensuring that the cloned object is a deep copy and maintains expected state.
    /// </summary>
    public class Test5 : BaseCloneable
    {
    /// <summary>
    /// Gets or sets the name of this <see cref="Test5"/> instance.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the enumerable collection associated with the <see cref="Test5.MyTest1"/> property.
    /// The specific contents and purpose of this collection depend on the implementation of <see cref="Test5"/>.
    /// </summary>
        public IEnumerable MyTest1 { get; set; }
    }

    /// <summary>
    /// Tests the deep cloning functionality of the DeepCloneHelper for complex object graphs, ensuring all nested objects are properly cloned.
    /// </summary>
    public class Test6 : BaseCloneable
    {
    /// <summary>
    /// Gets or sets the name of this <see cref="Test6"/> instance.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the collection of Test1 instances.
    /// </summary>
        public IEnumerable<Test1> MyTest1 { get; set; }
    }

    /// <summary>
    /// Tests the DeepCloneHelper functionality for scenario 7. This scenario should be described in more detail to clarify what specific aspect or edge case is being validated.
    /// </summary>
    public class Test7 : BaseCloneable
    {
    /// <summary>
    /// Gets or sets the name associated with this <see cref="Test7"/> instance.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the list of Test1 instances.
    /// </summary>
        public List<Test1> MyTest1 { get; set; }
    }

    /// <summary>
    /// Tests the DeepCloneHelper functionality for scenario 8, ensuring that objects are correctly deep cloned under specific conditions.
    /// </summary>
    public class Test8 : BaseCloneable
    {
    /// <summary>
    /// Gets or sets the Name property of the <see cref="Test8"/> class.
    /// </summary>
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the collection of Test1 instances.
    /// </summary>
        public ICollection<Test1> MyTest1 { get; set; }
    }

    /// <summary>
    /// Represents a base class for cloneable objects used in deep clone helper tests.
    /// </summary>
    public abstract class BaseCloneable : IDeepCloneable
    {
    /// <summary>
    /// Creates a deep clone of the current object.
    /// </summary>
    /// <returns>A deep cloned copy of the current object.</returns>
        public object DeepClone()
        {
            var clone = (IDeepCloneable)MemberwiseClone();
            DeepCloneHelper.DeepCloneRefProperties(this, clone);
            return clone;
        }
    }
}
