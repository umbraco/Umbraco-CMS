// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

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
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.AnyString));
        Assert.That(udi.Id, Is.EqualTo("test-id"));
        Assert.That(udi.ToString(), Is.EqualTo("umb://" + Constants.UdiEntityType.AnyString + "/test-id"));
    }

    [Test]
    public void StringUdiParseTest()
    {
        var udi = UdiParser.Parse("umb://" + Constants.UdiEntityType.AnyString + "/test-id");
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.AnyString));
        Assert.That(udi, Is.InstanceOf<StringUdi>());
        var stringEntityId = udi as StringUdi;
        Assert.That(stringEntityId, Is.Not.Null);
        Assert.That(stringEntityId.Id, Is.EqualTo("test-id"));
        Assert.That(udi.ToString(), Is.EqualTo("umb://" + Constants.UdiEntityType.AnyString + "/test-id"));

        udi = UdiParser.Parse("umb://" + Constants.UdiEntityType.AnyString + "/DA845952BE474EE9BD6F6194272AC750");
        Assert.That(udi, Is.InstanceOf<StringUdi>());
    }

    [Test]
    public void StringEncodingTest()
    {
        // absolute path is unescaped
        var uri = new Uri("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test");
        Assert.That(uri.ToString(), Is.EqualTo("umb://" + Constants.UdiEntityType.AnyString + "/this is a test"));
        Assert.That(uri.AbsoluteUri, Is.EqualTo("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test"));
        Assert.That(uri.AbsolutePath, Is.EqualTo("/this%20is%20a%20test"));

        Assert.That(Uri.UnescapeDataString(uri.AbsolutePath), Is.EqualTo("/this is a test"));
        Assert.That(Uri.EscapeDataString("/this is a test"), Is.EqualTo("%2Fthis%20is%20a%20test"));
#pragma warning disable SYSLIB0013 // Uri.EscapeUriString is obsolete - testing legacy Uri escaping behavior
        Assert.That(Uri.EscapeUriString("/this is a test"), Is.EqualTo("/this%20is%20a%20test"));
#pragma warning restore SYSLIB0013

        var udi = UdiParser.Parse("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test");
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.AnyString));
        Assert.That(udi, Is.InstanceOf<StringUdi>());
        var stringEntityId = udi as StringUdi;
        Assert.That(stringEntityId, Is.Not.Null);
        Assert.That(stringEntityId.Id, Is.EqualTo("this is a test"));
        Assert.That(udi.ToString(), Is.EqualTo("umb://" + Constants.UdiEntityType.AnyString + "/this%20is%20a%20test"));

        var udi2 = new StringUdi(Constants.UdiEntityType.AnyString, "this is a test");
        Assert.That(udi2, Is.EqualTo(udi));

        var udi3 = new StringUdi(Constants.UdiEntityType.AnyString, "path to/this is a test.xyz");
        Assert.That(udi3.ToString(), Is.EqualTo("umb://" + Constants.UdiEntityType.AnyString + "/path%20to/this%20is%20a%20test.xyz"));
    }

    [Test]
    public void StringEncodingTest2()
    {
        // reserved = : / ? # [ ] @ ! $ & ' ( ) * + , ; =
        // unreserved = alpha digit - . _ ~
        Assert.That(Uri.EscapeDataString(":/?#[]@!$&'()+,;=.-_~%"), Is.EqualTo("%3A%2F%3F%23%5B%5D%40%21%24%26%27%28%29%2B%2C%3B%3D.-_~%25"));
#pragma warning disable SYSLIB0013 // Uri.EscapeUriString is obsolete - testing legacy Uri escaping behavior
        Assert.That(Uri.EscapeUriString(":/?#[]@!$&'()+,;=.-_~%"), Is.EqualTo(":/?#[]@!$&'()+,;=.-_~%25"));
#pragma warning restore SYSLIB0013

        // we cannot have reserved chars at random places
        // we want to keep the / in string udis
        var r = string.Join("/", "path/to/View[1].cshtml".Split('/').Select(Uri.EscapeDataString));
        Assert.That(r, Is.EqualTo("path/to/View%5B1%5D.cshtml"));
        Assert.That(Uri.IsWellFormedUriString("umb://partial-view/" + r, UriKind.Absolute), Is.True);

        // with the proper fix in StringUdi this should work:
        var udi1 = new StringUdi("partial-view", "path/to/View[1].cshtml");
        Assert.That(udi1.ToString(), Is.EqualTo("umb://partial-view/path/to/View%5B1%5D.cshtml"));
        var udi2 = UdiParser.Parse("umb://partial-view/path/to/View%5B1%5D.cshtml");
        Assert.That(((StringUdi)udi2).Id, Is.EqualTo("path/to/View[1].cshtml"));
    }

    [Test]
    public void GuidUdiCtorTest()
    {
        var guid = Guid.NewGuid();
        var udi = new GuidUdi(Constants.UdiEntityType.AnyGuid, guid);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.AnyGuid));
        Assert.That(udi.Guid, Is.EqualTo(guid));
        Assert.That(udi.ToString(), Is.EqualTo("umb://" + Constants.UdiEntityType.AnyGuid + "/" + guid.ToString("N")));
    }

    [Test]
    public void GuidUdiParseTest()
    {
        var guid = Guid.NewGuid();
        var s = "umb://" + Constants.UdiEntityType.AnyGuid + "/" + guid.ToString("N");
        var udi = UdiParser.Parse(s);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.AnyGuid));
        Assert.That(udi, Is.InstanceOf<GuidUdi>());
        var gudi = udi as GuidUdi;
        Assert.That(gudi, Is.Not.Null);
        Assert.That(gudi.Guid, Is.EqualTo(guid));
        Assert.That(udi.ToString(), Is.EqualTo(s));
    }

    [Test]
    public void EqualityTest()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        Assert.That(new GuidUdi("type", guid1), Is.EqualTo(new GuidUdi("type", guid1)));
        Assert.That(new GuidUdi("type", guid1), Is.EqualTo(new GuidUdi("type", guid1)));

        Assert.That(new GuidUdi("type", guid1), Is.EqualTo(new GuidUdi("type", guid1)));
        Assert.That(new GuidUdi("type", guid1), Is.EqualTo(new GuidUdi("type", guid1)));

        Assert.That(new GuidUdi("type", guid1), Is.Not.EqualTo(new GuidUdi("typex", guid1)));
        Assert.That(new GuidUdi("type", guid1), Is.Not.EqualTo(new GuidUdi("typex", guid1)));
        Assert.That(new GuidUdi("type", guid1), Is.Not.EqualTo(new GuidUdi("type", guid2)));
        Assert.That(new GuidUdi("type", guid1), Is.Not.EqualTo(new GuidUdi("type", guid2)));

        Assert.That(new GuidUdi("type", guid1).ToString(), Is.EqualTo(new StringUdi("type", guid1.ToString("N")).ToString()));
        Assert.That(new GuidUdi("type", guid1), Is.Not.EqualTo(new StringUdi("type", guid1.ToString("N"))));
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
        Assert.That(entities.Distinct().Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateTest()
    {
        var guid = Guid.NewGuid();
        var udi = Udi.Create(Constants.UdiEntityType.AnyGuid, guid);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.AnyGuid));
        Assert.That(((GuidUdi)udi).Guid, Is.EqualTo(guid));

        // *not* testing whether Udi.Create(type, invalidValue) throws
        // because we don't throw anymore - see U4-10409
    }

    [Test]
    public void CreateRootUdi_ForGuidEntityType_ReturnsRootGuidUdi()
    {
        var udi = Udi.Create(Constants.UdiEntityType.Document);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.Document));
        Assert.That(udi.IsRoot, Is.True);
        Assert.That(udi, Is.InstanceOf<GuidUdi>());
        Assert.That(((GuidUdi)udi).Guid, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void CreateRootUdi_ForStringEntityType_ReturnsRootStringUdi()
    {
        var udi = Udi.Create(Constants.UdiEntityType.Language);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.Language));
        Assert.That(udi.IsRoot, Is.True);
        Assert.That(udi, Is.InstanceOf<StringUdi>());
        Assert.That(((StringUdi)udi).Id, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateRootUdi_ForUnknownEntityType_ThrowsArgumentException()
    {
        const string unknownType = "not-a-real-entity-type";
        ArgumentException? ex = Assert.Throws<ArgumentException>(() => Udi.Create(unknownType));
        Assert.That(ex!.Message, Does.Contain(unknownType));
    }

    [Test]
    public void CreateRootUdi_WhenCalledTwice_ReturnsSameCachedInstance()
    {
        var first = Udi.Create(Constants.UdiEntityType.Media);
        var second = Udi.Create(Constants.UdiEntityType.Media);
        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public void RootUdiTest()
    {
        var stringUdi = new StringUdi(Constants.UdiEntityType.AnyString, string.Empty);
        Assert.That(stringUdi.IsRoot, Is.True);
        Assert.That(stringUdi.ToString(), Is.EqualTo("umb://any-string/"));

        var guidUdi = new GuidUdi(Constants.UdiEntityType.AnyGuid, Guid.Empty);
        Assert.That(guidUdi.IsRoot, Is.True);
        Assert.That(guidUdi.ToString(), Is.EqualTo("umb://any-guid/00000000000000000000000000000000"));

        var udi = UdiParser.Parse("umb://any-string/");
        Assert.That(udi.IsRoot, Is.True);
        Assert.That(udi, Is.InstanceOf<StringUdi>());

        udi = UdiParser.Parse("umb://any-guid/00000000000000000000000000000000");
        Assert.That(udi.IsRoot, Is.True);
        Assert.That(udi, Is.InstanceOf<GuidUdi>());

        udi = UdiParser.Parse("umb://any-guid/");
        Assert.That(udi.IsRoot, Is.True);
        Assert.That(udi, Is.InstanceOf<GuidUdi>());
    }

    [Test]
    public void RangeTest()
    {
        // can parse open string udi
        var stringUdiString = "umb://" + Constants.UdiEntityType.AnyString;
        Assert.That(UdiParser.TryParse(stringUdiString, out var stringUdi), Is.True);
        Assert.That(((StringUdi)stringUdi).Id, Is.EqualTo(string.Empty));

        // can parse open guid udi
        var guidUdiString = "umb://" + Constants.UdiEntityType.AnyGuid;
        Assert.That(UdiParser.TryParse(guidUdiString, out var guidUdi), Is.True);
        Assert.That(((GuidUdi)guidUdi).Guid, Is.EqualTo(Guid.Empty));

        // can create a range
        var range = new UdiRange(stringUdi, Constants.DeploySelector.ChildrenOfThis);

        // cannot create invalid ranges
        Assert.Throws<ArgumentException>(() => new UdiRange(guidUdi, "x"));
    }

    [Test]
    [TestCase(Constants.DeploySelector.This)]
    [TestCase(Constants.DeploySelector.ThisAndChildren)]
    [TestCase(Constants.DeploySelector.ThisAndDescendants)]
    [TestCase(Constants.DeploySelector.ChildrenOfThis)]
    [TestCase(Constants.DeploySelector.DescendantsOfThis)]
    [TestCase(Constants.DeploySelector.EntitiesOfType)]
    public void RangeParseTest(string selector)
    {
        var expected = new UdiRange(Udi.Create(Constants.UdiEntityType.AnyGuid, Guid.NewGuid()), selector);
        var actual = UdiRange.Parse(expected.ToString());

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void TryParseTest()
    {
        // try parse to "Udi"
        var stringUdiString = "umb://document/b9a56165-6c4e-4e79-8277-620430174ad3";
        Assert.That(UdiParser.TryParse(stringUdiString, out Udi udi1), Is.True);
        Assert.That(udi1 is GuidUdi guidUdi1 ? guidUdi1.Guid.ToString() : string.Empty, Is.EqualTo("b9a56165-6c4e-4e79-8277-620430174ad3"));

        // try parse to "Udi"
        Assert.That(UdiParser.TryParse("nope", out Udi udi2), Is.False);
        Assert.That(udi2, Is.Null);

        // try parse to "GuidUdi?"
        Assert.That(UdiParser.TryParse(stringUdiString, out GuidUdi? guidUdi3), Is.True);
        Assert.That(guidUdi3.Guid.ToString(), Is.EqualTo("b9a56165-6c4e-4e79-8277-620430174ad3"));

        // try parse to "GuidUdi?"
        Assert.That(UdiParser.TryParse("nope", out GuidUdi? guidUdi4), Is.False);
        Assert.That(guidUdi4, Is.Null);

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
                    Assert.Fail($"Error in class Constants.UdiEntityType, type \"{value}\" is not declared by GetTypes.");
                }

                types.Remove(value);
            }
        }

        Assert.That(
            types.Count, Is.EqualTo(0), $"Error in class Constants.UdiEntityType, GetTypes declares types that don't exist ({string.Join(",", types.Keys.Select(x => "\"" + x + "\""))}).");
    }

    [Test]
    public void KnownTypes()
    {
        // cannot parse an unknown type, udi is null
        // this will scan
        Assert.That(UdiParser.TryParse("umb://whatever/1234", out var udi), Is.False);
        Assert.That(udi, Is.Null);

        UdiParser.ResetUdiTypes();

        // unless we want to know
        Assert.That(UdiParser.TryParse("umb://whatever/1234", true, out udi), Is.False);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.Unknown));
        Assert.That(udi.GetType().FullName, Is.EqualTo("Umbraco.Cms.Core.UnknownTypeUdi"));

        UdiParser.ResetUdiTypes();

        // not known
        Assert.That(UdiParser.TryParse("umb://foo/A87F65C8D6B94E868F6949BA92C93045", true, out udi), Is.False);
        Assert.That(udi.EntityType, Is.EqualTo(Constants.UdiEntityType.Unknown));
        Assert.That(udi.GetType().FullName, Is.EqualTo("Umbraco.Cms.Core.UnknownTypeUdi"));
    }
}
