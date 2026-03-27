using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Media;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Media;

public class SvgDimensionExtractorTests
{
    [Test]
    public void Returns_Null_For_Non_Readable_Stream()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<svg/>"));
        stream.Close();
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Restores_Stream_Position_After_Reading()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="100" height="50">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        stream.Position = 10;
        var sut = CreateSvgDimensionExtractor();

        sut.GetDimensions(stream);

        Assert.That(stream.Position, Is.EqualTo(10));
    }

    [Test]
    public void Returns_Null_For_Invalid_Xml()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("this is not xml"));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Returns_Null_For_No_Dimensions_And_No_ViewBox()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Falls_Back_To_ViewBox_For_Decimal_Width_Height()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="100.5" height="50.3" viewBox="0 0 200 100">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(200, 100)));
    }

    [Test]
    public void Can_Parse_Attribute_Width_Height_No_Unit()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="100" height="50" viewBox="0 0 301 152">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

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
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(100, 50)));
    }

    [Test]
    public void Returns_Null_For_Zero_Width_Height()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="0" height="0">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Returns_Null_For_Negative_Width_Height()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" width="-10" height="-5">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.Null);
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
        var sut = CreateSvgDimensionExtractor();

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
        var sut = CreateSvgDimensionExtractor();

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
        var sut = CreateSvgDimensionExtractor();

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
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(8, 4)));
    }

    [Test]
    public void Can_Parse_ViewBox_With_Fractional_Values()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 99.7 50.3">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(100, 50)));
    }

    [Test]
    public void Can_Parse_ViewBox_With_Comma_Separators()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0,0,301,152">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.EqualTo(new Size(301, 152)));
    }

    [Test]
    public void Returns_Null_For_Negative_ViewBox_Dimensions()
    {
        var svg = """
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 -100 -50">
                      <rect x="0" y="0" width="300" height="150" />
                  </svg>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        var sut = CreateSvgDimensionExtractor();

        Size? result = sut.GetDimensions(stream);

        Assert.That(result, Is.Null);
    }

    private SvgDimensionExtractor CreateSvgDimensionExtractor() => new(NullLogger<SvgDimensionExtractor>.Instance);
}
