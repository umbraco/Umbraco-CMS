using NUnit.Framework;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class CultureImpactTests
    {
        [Test]
        public void All_Cultures()
        {
            var impact = CultureImpact.All;

            Assert.AreEqual(impact.Culture, "*");

            Assert.IsTrue(impact.ImpactsInvariantProperties);
            Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
            Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
            Assert.IsFalse(impact.ImpactsExplicitCulture);
            Assert.IsTrue(impact.ImpactsAllCultures);
            Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
        }

        [Test]
        public void Invariant_Culture()
        {
            var impact = CultureImpact.Invariant;

            Assert.AreEqual(impact.Culture, null);

            Assert.IsTrue(impact.ImpactsInvariantProperties);
            Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
            Assert.IsTrue(impact.ImpactsOnlyInvariantCulture);
            Assert.IsFalse(impact.ImpactsExplicitCulture);
            Assert.IsFalse(impact.ImpactsAllCultures);
            Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
        }

        [Test]
        public void Explicit_Default_Culture()
        {
            var impact = CultureImpact.Explicit("en-US", true);

            Assert.AreEqual(impact.Culture, "en-US");

            Assert.IsTrue(impact.ImpactsInvariantProperties);
            Assert.IsTrue(impact.ImpactsAlsoInvariantProperties);
            Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
            Assert.IsTrue(impact.ImpactsExplicitCulture);
            Assert.IsFalse(impact.ImpactsAllCultures);
            Assert.IsTrue(impact.ImpactsOnlyDefaultCulture);
        }

        [Test]
        public void Explicit_NonDefault_Culture()
        {
            var impact = CultureImpact.Explicit("en-US", false);

            Assert.AreEqual(impact.Culture, "en-US");

            Assert.IsFalse(impact.ImpactsInvariantProperties);
            Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
            Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
            Assert.IsTrue(impact.ImpactsExplicitCulture);
            Assert.IsFalse(impact.ImpactsAllCultures);
            Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
        }

        [Test]
        public void TryCreate_Explicit_Default_Culture()
        {
            var success = CultureImpact.TryCreate("en-US", true, ContentVariation.Culture, false, out var impact);
            Assert.IsTrue(success);

            Assert.AreEqual(impact.Culture, "en-US");

            Assert.IsTrue(impact.ImpactsInvariantProperties);
            Assert.IsTrue(impact.ImpactsAlsoInvariantProperties);
            Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
            Assert.IsTrue(impact.ImpactsExplicitCulture);
            Assert.IsFalse(impact.ImpactsAllCultures);
            Assert.IsTrue(impact.ImpactsOnlyDefaultCulture);
        }

        [Test]
        public void TryCreate_Explicit_NonDefault_Culture()
        {
            var success = CultureImpact.TryCreate("en-US", false, ContentVariation.Culture, false, out var impact);
            Assert.IsTrue(success);

            Assert.AreEqual(impact.Culture, "en-US");

            Assert.IsFalse(impact.ImpactsInvariantProperties);
            Assert.IsFalse(impact.ImpactsAlsoInvariantProperties);
            Assert.IsFalse(impact.ImpactsOnlyInvariantCulture);
            Assert.IsTrue(impact.ImpactsExplicitCulture);
            Assert.IsFalse(impact.ImpactsAllCultures);
            Assert.IsFalse(impact.ImpactsOnlyDefaultCulture);
        }

        [Test]
        public void TryCreate_AllCultures_For_Invariant()
        {
            var success = CultureImpact.TryCreate("*", false, ContentVariation.Nothing, false, out var impact);
            Assert.IsTrue(success);

            Assert.AreEqual(impact.Culture, null);

            Assert.AreSame(CultureImpact.Invariant, impact);
        }

        [Test]
        public void TryCreate_AllCultures_For_Variant()
        {
            var success = CultureImpact.TryCreate("*", false, ContentVariation.Culture, false, out var impact);
            Assert.IsTrue(success);

            Assert.AreEqual(impact.Culture, "*");

            Assert.AreSame(CultureImpact.All, impact);
        }

        [Test]
        public void TryCreate_Invariant_For_Variant()
        {
            var success = CultureImpact.TryCreate(null, false, ContentVariation.Culture, false, out var impact);
            Assert.IsFalse(success);
        }

        [Test]
        public void TryCreate_Invariant_For_Invariant()
        {
            var success = CultureImpact.TryCreate(null, false, ContentVariation.Nothing, false, out var impact);
            Assert.IsTrue(success);

            Assert.AreSame(CultureImpact.Invariant, impact);
        }
    }
}
