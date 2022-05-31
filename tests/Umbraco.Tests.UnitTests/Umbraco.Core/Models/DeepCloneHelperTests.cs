// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class DeepCloneHelperTests
{
    [Test]
    public void Deep_Clone_Ref_Properties()
    {
        var test1 = new Test1 { MyTest1 = new Test1(), MyTest2 = new Test2() };

        var clone = (Test1)test1.DeepClone();

        Assert.AreNotSame(test1, clone);
        Assert.AreNotSame(test1.MyTest1, clone.MyTest1);
        Assert.AreSame(test1.MyTest2, clone.MyTest2);
    }

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

    public class Test1 : BaseCloneable
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Test1 MyTest1 { get; set; }

        public Test2 MyTest2 { get; set; }
    }

    public class Test2
    {
        public string Name { get; set; }

        public Test1 MyTest1 { get; set; }
    }

    public class Test3 : BaseCloneable
    {
        public string Name { get; set; }

        public object[] MyTest1 { get; set; }
    }

    public class Test4 : BaseCloneable
    {
        public string Name { get; set; }

        public Test1[] MyTest1 { get; set; }
    }

    public class Test5 : BaseCloneable
    {
        public string Name { get; set; }

        public IEnumerable MyTest1 { get; set; }
    }

    public class Test6 : BaseCloneable
    {
        public string Name { get; set; }

        public IEnumerable<Test1> MyTest1 { get; set; }
    }

    public class Test7 : BaseCloneable
    {
        public string Name { get; set; }

        public List<Test1> MyTest1 { get; set; }
    }

    public class Test8 : BaseCloneable
    {
        public string Name { get; set; }

        public ICollection<Test1> MyTest1 { get; set; }
    }

    public abstract class BaseCloneable : IDeepCloneable
    {
        public object DeepClone()
        {
            var clone = (IDeepCloneable)MemberwiseClone();
            DeepCloneHelper.DeepCloneRefProperties(this, clone);
            return clone;
        }
    }
}
