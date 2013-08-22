using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Tests.CoreStrings
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
