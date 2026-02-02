using NUnit.Framework;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Strings;

[TestFixture]
public class Utf8ToAsciiConverterInterfaceTests
{
    [Test]
    public void IUtf8ToAsciiConverter_HasConvertStringMethod()
    {
        var type = typeof(IUtf8ToAsciiConverter);
        var method = type.GetMethod("Convert", new[] { typeof(string), typeof(char) });

        Assert.IsNotNull(method);
        Assert.AreEqual(typeof(string), method.ReturnType);
    }

    [Test]
    public void IUtf8ToAsciiConverter_HasConvertSpanMethod()
    {
        var type = typeof(IUtf8ToAsciiConverter);
        var methods = type.GetMethods().Where(m => m.Name == "Convert").ToList();

        Assert.That(methods.Count, Is.GreaterThanOrEqualTo(2), "Should have at least 2 Convert overloads");
    }
}
