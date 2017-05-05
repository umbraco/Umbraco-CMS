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