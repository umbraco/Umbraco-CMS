using System.Drawing;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Media;

public class SvgDimensionExtractorTests
{

    [Test]
    public void Can_Parse_Attribute_Width_Height_No_Unit()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="100" height="50" viewBox="0 0 301 152">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = new SvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(100, 50)));
    }

    [Test]
    public void Can_Parse_Attribute_Width_Height_Pixels()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="100px" height="50px" viewBox="0 0 301 152">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = new SvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(100, 50)));
    }

    [Test]
    public void Can_Parse_And_Fallback_Attribute_Width_Height_Percent()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="100%" height="50%" viewBox="0 0 301 152">
                      <rect x="0" y="0" width="50" height="75" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = new SvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(301, 152)));
    }

    [Test]
    public void Can_Parse_And_Fallback_Width_Height_Em()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="10em" height="5em" viewBox="0 0 301 152">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = new SvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(301, 152)));
    }

    [Test]
    public void Can_Parse_From_ViewBox()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 301 152">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = new SvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(301, 152)));
    }

    [Test]
    public void Can_Parse_From_ViewBox_Single_Digits()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 8 4">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = new SvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(8, 4)));
    }
}
