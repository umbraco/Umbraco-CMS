// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models
{
    [TestFixture]
    public class CultureImpactTests
    {
        private CultureImpactService BasicImpactService => createCultureImpactService();

        [Test]
        public void Get_Culture_For_Invariant_Errors()
        {
            var result = BasicImpactService.GetCultureForInvariantErrors(
                Mock.Of<IContent>(x => x.Published == true),
                new[] { "en-US", "fr-FR" },
                "en-US");
            Assert.AreEqual("en-US", result); // default culture is being saved so use it

            result = BasicImpactService.GetCultureForInvariantErrors(
                Mock.Of<IContent>(x => x.Published == false),
                new[] { "fr-FR" },
                "en-US");
            Assert.AreEqual("fr-FR", result); // default culture not being saved with not published version, use the first culture being saved

            result = BasicImpactService.GetCultureForInvariantErrors(
                Mock.Of<IContent>(x => x.Published == true),
                new[] { "fr-FR" },
                "en-US");
            Assert.AreEqual(null, result); // default culture not being saved with published version, use null
        }

        [Test]
        public void All_Cultures()
        {
            CultureImpact impact = BasicImpactService.ImpactAll();

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
            CultureImpact impact = BasicImpactService.ImpactInvariant();

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
            var impact = BasicImpactService.ImpactExplicit("en-US", true);

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
            var impact = BasicImpactService.ImpactExplicit("en-US", false);

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
            var success = BasicImpactService.TryCreate("en-US", true, ContentVariation.Culture, false, false, out CultureImpact impact);
            Assert.IsTrue(success);

            Assert.IsNotNull(impact);
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
            var success = BasicImpactService.TryCreate("en-US", false, ContentVariation.Culture, false, false, out CultureImpact impact);
            Assert.IsTrue(success);

            Assert.IsNotNull(impact);
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
            var success = BasicImpactService.TryCreate("*", false, ContentVariation.Nothing, false, false, out CultureImpact impact);
            Assert.IsTrue(success);

            Assert.IsNotNull(impact);
            Assert.AreEqual(impact.Culture, null);

            Assert.AreSame(BasicImpactService.ImpactInvariant(), impact);
        }

        [Test]
        public void TryCreate_AllCultures_For_Variant()
        {
            var success = BasicImpactService.TryCreate("*", false, ContentVariation.Culture, false, false, out CultureImpact impact);
            Assert.IsTrue(success);

            Assert.IsNotNull(impact);
            Assert.AreEqual(impact.Culture, "*");

            Assert.AreSame(BasicImpactService.ImpactAll(), impact);
        }

        [Test]
        public void TryCreate_Invariant_For_Variant()
        {
            var success = BasicImpactService.TryCreate(null, false, ContentVariation.Culture, false, false, out CultureImpact impact);
            Assert.IsFalse(success);
        }

        [Test]
        public void TryCreate_Invariant_For_Invariant()
        {
            var success = BasicImpactService.TryCreate(null, false, ContentVariation.Nothing, false, false, out CultureImpact impact);
            Assert.IsTrue(success);

            Assert.AreSame(BasicImpactService.ImpactInvariant(), impact);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Edit_Invariant_From_Non_Default_Impacts_Invariant_Properties(bool allowEditInvariantFromNonDefault)
        {
            var sut = createCultureImpactService(new SecuritySettings { AllowEditInvariantFromNonDefault = allowEditInvariantFromNonDefault });
            var impact = sut.ImpactExplicit("da", false);

            Assert.AreEqual(allowEditInvariantFromNonDefault, impact.ImpactsAlsoInvariantProperties);
        }

        private CultureImpactService createCultureImpactService(SecuritySettings securitySettings = null)
        {
            securitySettings ??= new SecuritySettings
            {
                AllowEditInvariantFromNonDefault = false,
            };

            return new CultureImpactService(new TestOptionsMonitor<SecuritySettings>(securitySettings));
        }

    }
}
