using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.CoreThings
{
    [TestFixture]
    public class TryConvertToTests : UmbracoTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            var settings = SettingsForTests.GetDefaultUmbracoSettings();

            // fixme - base should do it!
            Composition.RegisterUnique<IShortStringHelper>(_ => new DefaultShortStringHelper(settings));
        }

        [Test]
        public void ConvertToIntegerTest()
        {
            var conv = "100".TryConvertTo<int>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100, conv.Result);

            conv = "100.000".TryConvertTo<int>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100, conv.Result);

            conv = "100,000".TryConvertTo<int>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100, conv.Result);

            // oops
            conv = "100.001".TryConvertTo<int>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100, conv.Result);

            conv = 100m.TryConvertTo<int>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100, conv.Result);

            conv = 100.000m.TryConvertTo<int>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100, conv.Result);

            // oops
            conv = 100.001m.TryConvertTo<int>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100, conv.Result);
        }

        [Test]
        public void ConvertToDecimalTest()
        {
            var conv = "100".TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100m, conv.Result);

            conv = "100.000".TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100m, conv.Result);

            conv = "100,000".TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100m, conv.Result);

            conv = "100.001".TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100.001m, conv.Result);

            conv = 100m.TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100m, conv.Result);

            conv = 100.000m.TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100m, conv.Result);

            conv = 100.001m.TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100.001m, conv.Result);

            conv = 100.TryConvertTo<decimal>();
            Assert.IsTrue(conv);
            Assert.AreEqual(100m, conv.Result);
        }

        [Test]
        public void ConvertToDateTimeTest()
        {
            var conv = "2016-06-07".TryConvertTo<DateTime>();
            Assert.IsTrue(conv);
            Assert.AreEqual(new DateTime(2016, 6, 7), conv.Result);
        }
    }
}
