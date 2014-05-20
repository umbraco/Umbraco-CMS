using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class PreValueCollectionTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var d = new PreValueCollection(new Dictionary<string, PreValue>
            {
                {"blah1", new PreValue(1, "test1", 1)},
                {"blah2", new PreValue(2, "test1", 3)},
                {"blah3", new PreValue(3, "test1", 2)}
            });

            var a = new PreValueCollection(new[]
            {
                new PreValue(1, "test1", 1),
                new PreValue(2, "test1", 3),
                new PreValue(3, "test1", 2)
            });

            var clone1 = (PreValueCollection)d.DeepClone();
            var clone2 = (PreValueCollection)a.DeepClone();

            Action<PreValueCollection, PreValueCollection> assert = (orig, clone) =>
            {
                Assert.AreNotSame(orig, clone);
                var oDic = orig.FormatAsDictionary();
                var cDic = clone.FormatAsDictionary();
                Assert.AreEqual(oDic.Keys.Count(), cDic.Keys.Count());
                foreach (var k in oDic.Keys)
                {
                    Assert.AreNotSame(oDic[k], cDic[k]);
                    Assert.AreEqual(oDic[k].Id, cDic[k].Id);
                    Assert.AreEqual(oDic[k].SortOrder, cDic[k].SortOrder);
                    Assert.AreEqual(oDic[k].Value, cDic[k].Value);
                }
            };

            assert(d, clone1);
            assert(a, clone2);
        }
    }
}