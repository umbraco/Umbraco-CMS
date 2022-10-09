// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.CoreThings;

[TestFixture]
public class UdiTests
{
    [SetUp]
    public void SetUp() => UdiParser.ResetUdiTypes();

    [Test]
    public void StringUdiCtorTest()
    {
        var udi = new StringUdi(Constants.UdiEntityType.AnyString, "test-id");
        Assert.AreEqual(Constants.UdiEntityType.AnyString, udi.EntityType);
        Assert.AreEqual("test-id", udi.Id);
        Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/test-id", udi.ToString());
    }

    [Test]
    public void StringUdiParseTest()
    {
        var udi = UdiParser.Parse("umb://" + Constants.UdiEntityType.AnyString + "/test-id");
        Assert.AreEqual(Constants.UdiEntityType.AnyString, udi.EntityType);
        Assert.IsInstanceOf<StringUdi>(udi);
        var stringEntityId = udi as StringUdi;
        Assert.IsNotNull(stringEntityId);
        Assert.AreEqual("test-id", stringEntityId.Id);
        Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyString + "/test-id", udi.ToString());

        udi = UdiParser.Parse("umb://" + Constants.UdiEntityType.AnyString + "/DA845952BE474EE9BD6F6194272AC750");
        Assert.IsInstanceOf<StringUdi>(udi);
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

        var udi = UdiParser.Parse("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test");
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

    [Test]
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
        var udi2 = UdiParser.Parse("umb://partial-view-macro/path/to/View%5B1%5D.cshtml");
        Assert.AreEqual("path/to/View[1].cshtml", ((StringUdi)udi2).Id);
    }

    [Test]
    public void GuidUdiCtorTest()
    {
        var guid = Guid.NewGuid();
        var udi = new GuidUdi(Constants.UdiEntityType.AnyGuid, guid);
        Assert.AreEqual(Constants.UdiEntityType.AnyGuid, udi.EntityType);
        Assert.AreEqual(guid, udi.Guid);
        Assert.AreEqual("umb://" + Constants.UdiEntityType.AnyGuid + "/" + guid.ToString("N"), udi.ToString());
    }

    [Test]
    public void GuidUdiParseTest()
    {
        var guid = Guid.NewGuid();
        var s = "umb://" + Constants.UdiEntityType.AnyGuid + "/" + guid.ToString("N");
        var udi = UdiParser.Parse(s);
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

        Assert.IsTrue(new GuidUdi("type", guid1).Equals(new GuidUdi("type", guid1)));
        Assert.IsTrue(new GuidUdi("type", guid1) == new GuidUdi("type", guid1));

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
        GuidUdi[] entities =
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

        // *not* testing whether Udi.Create(type, invalidValue) throws
        // because we don't throw anymore - see U4-10409
    }

    [Test]
    public void RootUdiTest()
    {
        var stringUdi = new StringUdi(Constants.UdiEntityType.AnyString, string.Empty);
        Assert.IsTrue(stringUdi.IsRoot);
        Assert.AreEqual("umb://any-string/", stringUdi.ToString());

        var guidUdi = new GuidUdi(Constants.UdiEntityType.AnyGuid, Guid.Empty);
        Assert.IsTrue(guidUdi.IsRoot);
        Assert.AreEqual("umb://any-guid/00000000000000000000000000000000", guidUdi.ToString());

        var udi = UdiParser.Parse("umb://any-string/");
        Assert.IsTrue(udi.IsRoot);
        Assert.IsInstanceOf<StringUdi>(udi);

        udi = UdiParser.Parse("umb://any-guid/00000000000000000000000000000000");
        Assert.IsTrue(udi.IsRoot);
        Assert.IsInstanceOf<GuidUdi>(udi);

        udi = UdiParser.Parse("umb://any-guid/");
        Assert.IsTrue(udi.IsRoot);
        Assert.IsInstanceOf<GuidUdi>(udi);
    }

    [Test]
    public void RangeTest()
    {
        // can parse open string udi
        var stringUdiString = "umb://" + Constants.UdiEntityType.AnyString;
        Assert.IsTrue(UdiParser.TryParse(stringUdiString, out var stringUdi));
        Assert.AreEqual(string.Empty, ((StringUdi)stringUdi).Id);

        // can parse open guid udi
        var guidUdiString = "umb://" + Constants.UdiEntityType.AnyGuid;
        Assert.IsTrue(UdiParser.TryParse(guidUdiString, out var guidUdi));
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
            Converters = new JsonConverter[] { new UdiJsonConverter(), new UdiRangeJsonConverter() },
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

    [Test]
    public void ValidateUdiEntityType()
    {
        var types = UdiParser.GetKnownUdiTypes();

        foreach (var fi in typeof(Constants.UdiEntityType).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            // IsLiteral determines if its value is written at
            //   compile time and not changeable
            // IsInitOnly determine if the field can be set
            //   in the body of the constructor
            // for C# a field which is readonly keyword would have both true
            //   but a const field would have only IsLiteral equal to true
            if (fi.IsLiteral && fi.IsInitOnly == false)
            {
                var value = fi.GetValue(null).ToString();

                if (types.ContainsKey(value) == false)
                {
                    Assert.Fail("Error in class Constants.UdiEntityType, type \"{0}\" is not declared by GetTypes.", value);
                }

                types.Remove(value);
            }
        }

        Assert.AreEqual(
            0,
            types.Count,
            "Error in class Constants.UdiEntityType, GetTypes declares types that don't exist ({0}).",
            string.Join(",", types.Keys.Select(x => "\"" + x + "\"")));
    }

    [Test]
    public void KnownTypes()
    {
        // cannot parse an unknown type, udi is null
        // this will scan
        Assert.IsFalse(UdiParser.TryParse("umb://whatever/1234", out var udi));
        Assert.IsNull(udi);

        UdiParser.ResetUdiTypes();

        // unless we want to know
        Assert.IsFalse(UdiParser.TryParse("umb://whatever/1234", true, out udi));
        Assert.AreEqual(Constants.UdiEntityType.Unknown, udi.EntityType);
        Assert.AreEqual("Umbraco.Cms.Core.UnknownTypeUdi", udi.GetType().FullName);

        UdiParser.ResetUdiTypes();

        // not known
        Assert.IsFalse(UdiParser.TryParse("umb://foo/A87F65C8D6B94E868F6949BA92C93045", true, out udi));
        Assert.AreEqual(Constants.UdiEntityType.Unknown, udi.EntityType);
        Assert.AreEqual("Umbraco.Cms.Core.UnknownTypeUdi", udi.GetType().FullName);

        // scanned
        UdiParserServiceConnectors
            .RegisterServiceConnector<
                FooConnector>(); // this is the equivalent of scanning but we'll just manually register this one
        Assert.IsTrue(UdiParser.TryParse("umb://foo/A87F65C8D6B94E868F6949BA92C93045", out udi));
        Assert.IsInstanceOf<GuidUdi>(udi);

        // known
        Assert.IsTrue(UdiParser.TryParse("umb://foo/A87F65C8D6B94E868F6949BA92C93045", true, out udi));
        Assert.IsInstanceOf<GuidUdi>(udi);

        // can get method for Deploy compatibility
        var method = typeof(UdiParser).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(bool) }, null);
        Assert.IsNotNull(method);
    }

    [UdiDefinition("foo", UdiType.GuidUdi)]
    public class FooConnector : IServiceConnector
    {
        public IArtifact GetArtifact(Udi udi) => throw new NotImplementedException();

        public IArtifact GetArtifact(object entity) => throw new NotImplementedException();

        public ArtifactDeployState ProcessInit(IArtifact art, IDeployContext context) =>
            throw new NotImplementedException();

        public void Process(ArtifactDeployState dart, IDeployContext context, int pass) =>
            throw new NotImplementedException();

        public void Explode(UdiRange range, List<Udi> udis) => throw new NotImplementedException();

        public NamedUdiRange GetRange(Udi udi, string selector) => throw new NotImplementedException();

        public NamedUdiRange GetRange(string entityType, string sid, string selector) =>
            throw new NotImplementedException();

        public bool Compare(IArtifact art1, IArtifact art2, ICollection<Difference> differences = null) =>
            throw new NotImplementedException();
    }
}
