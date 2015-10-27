using NUnit.Framework;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Strings;

namespace Umbraco.Tests.Strings
{
	[TestFixture]
    public class ShortStringHelperResolverTest
    {
	    [SetUp]
	    public void Setup()
	    {
            ShortStringHelperResolver.Reset();
	    }

        [TearDown]
        public void TearDown()
        {
            ShortStringHelperResolver.Reset();
        }

        [Test]
        public void FreezesHelperWhenResolutionFreezes()
        {
            var helper = new MockShortStringHelper();
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(helper);
            Assert.IsFalse(helper.IsFrozen);
            Resolution.Freeze();
            Assert.AreSame(helper, ShortStringHelperResolver.Current.Helper);
            Assert.IsTrue(helper.IsFrozen);
        }
    }
}
