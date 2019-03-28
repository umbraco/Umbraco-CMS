using NUnit.Framework;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class CultureTypeTests
    {
        [Test]
        public void All()
        {
            var ct = CultureType.All;

            Assert.AreEqual(ct.Culture, "*");
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsFalse(ct.IsDefaultCulture);
        }

        [Test]
        public void Invariant()
        {
            var ct = CultureType.Invariant;

            Assert.AreEqual(ct.Culture, null);
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsFalse(ct.IsDefaultCulture);
        }

        [Test]
        public void Single_Default_Culture()
        {
            var ct = CultureType.Explicit("en-US", true);

            Assert.AreEqual(ct.Culture, "en-US");
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsTrue(ct.IsDefaultCulture);
        }

        [Test]
        public void Single_Non_Default_Culture()
        {
            var ct = CultureType.Explicit("en-US", false);

            Assert.AreEqual(ct.Culture, "en-US");
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsFalse(ct.IsDefaultCulture);
        }

        [Test]
        public void Try_Create_Explicit_Default_Culture()
        {
            var success = CultureType.TryCreate(ContentVariation.Culture, "en-US", true, out var ct);
            Assert.IsTrue(success);

            Assert.AreEqual(ct.Culture, "en-US");
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsTrue(ct.IsDefaultCulture);
        }

        [Test]
        public void Try_Create_Explicit_Non_Default_Culture()
        {
            var success = CultureType.TryCreate(ContentVariation.Culture, "en-US", false, out var ct);
            Assert.IsTrue(success);

            Assert.AreEqual(ct.Culture, "en-US");
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsFalse(ct.IsDefaultCulture);
        }

        [Test]
        public void Try_Create_Wildcard_Invariant()
        {
            var success = CultureType.TryCreate(ContentVariation.Nothing, "*", false, out var ct);
            Assert.IsTrue(success);

            Assert.AreEqual(ct.Culture, null);
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsFalse(ct.IsDefaultCulture);
        }

        [Test]
        public void Try_Create_Wildcard_Variant()
        {
            var success = CultureType.TryCreate(ContentVariation.Culture, "*", false, out var ct);
            Assert.IsTrue(success);

            Assert.AreEqual(ct.Culture, "*");
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsFalse(ct.IsDefaultCulture);
        }

        [Test]
        public void Try_Create_Null_Variant()
        {
            var success = CultureType.TryCreate(ContentVariation.Culture, null, false, out var ct);
            Assert.IsFalse(success);
        }

        [Test]
        public void Try_Create_Null_Invariant()
        {
            var success = CultureType.TryCreate(ContentVariation.Nothing, null, false, out var ct);
            Assert.IsTrue(success);

            Assert.AreEqual(ct.Culture, null);
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantProperties));
            Assert.IsTrue(ct.CultureBehavior.HasFlag(CultureType.Behavior.InvariantCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.ExplicitCulture));
            Assert.IsFalse(ct.CultureBehavior.HasFlag(CultureType.Behavior.AllCultures));
            Assert.IsFalse(ct.IsDefaultCulture);
        }
    }
}
