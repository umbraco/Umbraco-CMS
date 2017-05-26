using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests
{
    [TestFixture]
    public class UdiTests
    {
        [Test]
        public void StringEntityCtorTest()
        {
            var udi = new StringUdi(Constants.UdiEntityType.AnyString, "test-id");
            Assert.AreEqual(Constants.UdiEntityType.AnyString, udi.EntityType);
            Assert.AreEqual("test-id", udi.Id);
            Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/test-id", udi.ToString());
        }

        [Test]
        public void StringEntityParseTest()
        {
            var udi = Udi.Parse("umb://" + Constants.UdiEntityType.AnyString + "/test-id");
            Assert.AreEqual(Constants.UdiEntityType.AnyString, udi.EntityType);
            Assert.IsInstanceOf<StringUdi>(udi);
            var stringEntityId = udi as StringUdi;
            Assert.IsNotNull(stringEntityId);
            Assert.AreEqual("test-id", stringEntityId.Id);
            Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/test-id", udi.ToString());
        }

        [Test]
        public void StringEncodingTest()
        {
            // absolute path is unescaped
            var uri = new Uri("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test");
            Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/this is a test", uri.ToString());
            Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test", uri.AbsoluteUri);
            Assert.AreEqual("/this%20is%20a%20test", uri.AbsolutePath);

            Assert.AreEqual("/this is a test", Uri.UnescapeDataString(uri.AbsolutePath));
            Assert.AreEqual("%2Fthis%20is%20a%20test", Uri.EscapeDataString("/this is a test"));
            Assert.AreEqual("/this%20is%20a%20test", Uri.EscapeUriString("/this is a test"));

            var udi = Udi.Parse("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test");
            Assert.AreEqual(Constants.UdiEntityType.AnyString, udi.EntityType);
            Assert.IsInstanceOf<StringUdi>(udi);
            var stringEntityId = udi as StringUdi;
            Assert.IsNotNull(stringEntityId);
            Assert.AreEqual("this is a test", stringEntityId.Id);
            Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test", udi.ToString());

            var udi2 = new StringUdi(Constants.UdiEntityType.AnyString, "this is a test");
            Assert.AreEqual(udi, udi2);

            var udi3 = new StringUdi(Constants.UdiEntityType.AnyString, "path to/this is a test.xyz");
            Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/path%20to/this%20is%20a%20test.xyz", udi3.ToString());
        }

        [Test, Ignore]
        public void StringEncodingTest2()
        {
            // reserved = : / ? # [ ] @ ! $ & ' ( ) * + , ; =
            // unreserved = alpha digit - . _ ~

            Assert.AreEqual("%3A%2F%3F%23%5B%5D%40%21%24%26%27%28%29%2B%2C%3B%3D.-_~%25", Uri.EscapeDataString(":/?#[]@!$&'()+,;=.-_~%"));
            Assert.AreEqual(":/?#[]@!$&'()+,;=.-_~%25", Uri.EscapeUriString(":/?#[]@!$&'()+,;=.-_~%"));

            // we cannot have reserved chars at random places
            // we want to keep the / in string udis

            var r = string.Join("/", "path/to/View[1].cshtml".Split('/').Select(Uri.EscapeDataString));
            Assert.AreEqual("path/to/View%5B1%5D.cshtml", r);
            Assert.IsTrue(Uri.IsWellFormedUriString("umb://partial-view-macro/" + r, UriKind.Absolute));

            // with the proper fix in StringUdi this should work:
            var udi1 = new StringUdi("partial-view-macro", "path/to/View[1].cshtml");
            Assert.AreEqual("umb://partial-view-macro/path/to/View%5B1%5D.cshtml", udi1.ToString());
            var udi2 = Udi.Parse("umb://partial-view-macro/path/to/View%5B1%5D.cshtml");
            Assert.AreEqual("path/to/View[1].cshtml", ((StringUdi) udi2).Id);
        }

        [Test]
        public void GuidEntityCtorTest()
        {
            var guid = Guid.NewGuid();
            var udi = new GuidUdi(Constants.UdiEntityType.AnyGuid, guid);
            Assert.AreEqual(Constants.UdiEntityType.AnyGuid, udi.EntityType);
            Assert.AreEqual(guid, udi.Guid);
            Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyGuid + "/" + guid.ToString("N"), udi.ToString());
        }

        [Test]
        public void GuidEntityParseTest()
        {
            var guid = Guid.NewGuid();
            var s = "umb://" + Constants.UdiEntityType.AnyGuid + "/" + guid.ToString("N");
            var udi = Udi.Parse(s);
            Assert.AreEqual(Constants.UdiEntityType.AnyGuid, udi.EntityType);
            Assert.IsInstanceOf<GuidUdi>(udi);
            var gudi = udi as GuidUdi;
            Assert.IsNotNull(gudi);
            Assert.AreEqual(guid, gudi.Guid);
            Assert.AreEqual(s, udi.ToString());
        }

        [Test]
        public void EqualityTest()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            Assert.IsTrue(new GuidUdi("type", guid1).Equals(new GuidUdi("type", guid1)));
            Assert.IsTrue(new GuidUdi("type", guid1) == new GuidUdi("type", guid1));

            Assert.IsTrue(((Udi)new GuidUdi("type", guid1)).Equals((Udi)new GuidUdi("type", guid1)));
            Assert.IsTrue((Udi)new GuidUdi("type", guid1) == (Udi)new GuidUdi("type", guid1));

            Assert.IsFalse(new GuidUdi("type", guid1).Equals(new GuidUdi("typex", guid1)));
            Assert.IsFalse(new GuidUdi("type", guid1) == new GuidUdi("typex", guid1));
            Assert.IsFalse(new GuidUdi("type", guid1).Equals(new GuidUdi("type", guid2)));
            Assert.IsFalse(new GuidUdi("type", guid1) == new GuidUdi("type", guid2));

            Assert.IsTrue(new GuidUdi("type", guid1).ToString() == new StringUdi("type", guid1.ToString("N")).ToString());
            Assert.IsFalse(new GuidUdi("type", guid1) == new StringUdi("type", guid1.ToString("N")));
        }

        [Test]
        public void DistinctTest()
        {
            var guid1 = Guid.NewGuid();
            var entities = new[]
            {
                new GuidUdi(Constants.UdiEntityType.AnyGuid, guid1),
                new GuidUdi(Constants.UdiEntityType.AnyGuid, guid1),
                new GuidUdi(Constants.UdiEntityType.AnyGuid, guid1),
            };
            Assert.AreEqual(1, entities.Distinct().Count());
        }

        [Test]
        public void CreateTest()
        {
            var guid = Guid.NewGuid();
            var udi = Udi.Create(Constants.UdiEntityType.AnyGuid, guid);
            Assert.AreEqual(Constants.UdiEntityType.AnyGuid, udi.EntityType);
            Assert.AreEqual(guid, ((GuidUdi)udi).Guid);

            Assert.Throws<InvalidOperationException>(() => Udi.Create(Constants.UdiEntityType.AnyString, guid));
            Assert.Throws<InvalidOperationException>(() => Udi.Create(Constants.UdiEntityType.AnyGuid, "foo"));
            Assert.Throws<ArgumentException>(() => Udi.Create("barf", "foo"));
        }

        [Test]
        public void RangeTest()
        {
            // can parse open string udi
            var stringUdiString = "umb://" + Constants.UdiEntityType.AnyString;
            Udi stringUdi;
            Assert.IsTrue(Udi.TryParse(stringUdiString, out stringUdi));
            Assert.AreEqual(string.Empty, ((StringUdi)stringUdi).Id);

            // can parse open guid udi
            var guidUdiString = "umb://" + Constants.UdiEntityType.AnyGuid;
            Udi guidUdi;
            Assert.IsTrue(Udi.TryParse(guidUdiString, out guidUdi));
            Assert.AreEqual(Guid.Empty, ((GuidUdi)guidUdi).Guid);

            // can create a range
            var range = new UdiRange(stringUdi, Constants.DeploySelector.ChildrenOfThis);

            // cannot create invalid ranges
            Assert.Throws<ArgumentException>(() => new UdiRange(guidUdi, "x"));
        }

        [Test]
        public void SerializationTest()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new UdiJsonConverter(), new UdiRangeJsonConverter() }
            };


            var guid = Guid.NewGuid();
            var udi = new GuidUdi(Constants.UdiEntityType.AnyGuid, guid);
            var json = JsonConvert.SerializeObject(udi, settings);
            Assert.AreEqual(string.Format("\"umb://any-guid/{0:N}\"", guid), json);

            var dudi = JsonConvert.DeserializeObject<Udi>(json, settings);
            Assert.AreEqual(Constants.UdiEntityType.AnyGuid, dudi.EntityType);
            Assert.AreEqual(guid, ((GuidUdi)dudi).Guid);

            var range = new UdiRange(udi, Constants.DeploySelector.ChildrenOfThis);
            json = JsonConvert.SerializeObject(range, settings);
            Assert.AreEqual(string.Format("\"umb://any-guid/{0:N}?children\"", guid), json);

            var drange = JsonConvert.DeserializeObject<UdiRange>(json, settings);
            Assert.AreEqual(udi, drange.Udi);
            Assert.AreEqual(string.Format("umb://any-guid/{0:N}", guid), drange.Udi.UriValue.ToString());
            Assert.AreEqual(Constants.DeploySelector.ChildrenOfThis, drange.Selector);
        }
    }
}