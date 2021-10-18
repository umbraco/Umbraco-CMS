using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.Web.Mvc
{
    [TestFixture]
    public class ViewDataDictionaryExtensionTests
    {
        [Test]
        public void Merge_View_Data()
        {
            var source = new ViewDataDictionary();
            var dest = new ViewDataDictionary();
            source.Add("Test1", "Test1");
            dest.Add("Test2", "Test2");

            dest.MergeViewDataFrom(source);

            Assert.AreEqual(2, dest.Count);
        }

        [Test]
        public void Merge_View_Data_Retains_Destination_Values()
        {
            var source = new ViewDataDictionary();
            var dest = new ViewDataDictionary();
            source.Add("Test1", "Test1");
            dest.Add("Test1", "MyValue");
            dest.Add("Test2", "Test2");

            dest.MergeViewDataFrom(source);

            Assert.AreEqual(2, dest.Count);
            Assert.AreEqual("MyValue", dest["Test1"]);
            Assert.AreEqual("Test2", dest["Test2"]);
        }

    }
}
