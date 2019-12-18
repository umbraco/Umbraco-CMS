using NUnit.Framework;

namespace Umbraco.Tests.Testing.TestingTests
{
    // these 4 test classes validate that our test class pattern *should* be:
    // - test base classes *not* marked as [TestFixture] but having a virtual SetUp
    //   method marked as [SetUp]
    // - test classes inheriting from base class, marked as [TestFixture], and
    //   overriding the SetUp method *without* repeating the [SetUp] attribute
    // - same for TearDown
    //
    // ie what InheritsTestClassWithoutAttribute does - and it works

    [TestFixture]
    public class InheritsTestClassWithAttribute : NUnitTestClassBase
    {
        public const int ConstValue = 77;

        protected int Value { get; set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            Value = ConstValue;
        }

        [Test]
        public void AssertValues()
        {
            Assert.AreEqual(ConstBaseValue, BaseValue);
            Assert.AreEqual(ConstValue, Value);
        }
    }

    [TestFixture]
    public class InheritsTestClassWithoutAttribute : NUnitTestClassBase
    {
        public const int ConstValue = 88;

        protected int Value { get; set; }

        // not needed!
        public override void SetUp()
        {
            base.SetUp();
            Value = ConstValue;
        }

        [Test]
        public void AssertValues()
        {
            Assert.AreEqual(ConstBaseValue, BaseValue);
            Assert.AreEqual(ConstValue, Value);
        }
    }

    [TestFixture]
    public class InheritTestFixtureWithAttribute : NUnitTestFixtureBase
    {
        public const int ConstValue = 99;

        protected int Value { get; set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            Value = ConstValue;
        }

        [Test]
        public void AssertValues()
        {
            Assert.AreEqual(ConstBaseValue, BaseValue);
            Assert.AreEqual(ConstValue, Value);
        }
    }

    [TestFixture]
    public class InheritTestFixtureWithoutAttribute : NUnitTestFixtureBase
    {
        public const int ConstValue = 66;

        protected int Value { get; set; }

        // not needed!
        public override void SetUp()
        {
            base.SetUp();
            Value = ConstValue;
        }

        [Test]
        public void AssertValues()
        {
            Assert.AreEqual(ConstBaseValue, BaseValue);
            Assert.AreEqual(ConstValue, Value);
        }
    }

    public class NUnitTestClassBase
    {
        public const int ConstBaseValue = 33;

        protected int BaseValue { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            BaseValue = ConstBaseValue;
        }
    }

    [TestFixture]
    public class NUnitTestFixtureBase
    {
        public const int ConstBaseValue = 42;

        protected int BaseValue { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            BaseValue = ConstBaseValue;
        }
    }
}
