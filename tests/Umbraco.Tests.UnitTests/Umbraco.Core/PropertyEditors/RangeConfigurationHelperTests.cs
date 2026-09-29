using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class RangeConfigurationHelperTests
{
    [Test]
    public void Returns_False_For_Null()
    {
        var result = RangeConfigurationHelper.TryGetBounds(null, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(min, Is.Null);
            Assert.That(max, Is.Null);
        });
    }

    [Test]
    public void Returns_False_For_Non_Range_Value()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RangeConfigurationHelper.TryGetBounds("not-a-range", out _, out _), Is.False);
            Assert.That(RangeConfigurationHelper.TryGetBounds(42, out _, out _), Is.False);
        });
    }

    [Test]
    public void Reads_Typed_NumberRange()
    {
        var result = RangeConfigurationHelper.TryGetBounds(new NumberRange { Min = 2, Max = 5 }, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.EqualTo(2));
            Assert.That(max, Is.EqualTo(5));
        });
    }

    [Test]
    public void Reads_Typed_DecimalRange()
    {
        var result = RangeConfigurationHelper.TryGetBounds(new DecimalRange { Min = 1.5m, Max = 9.5m }, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.EqualTo(1.5m));
            Assert.That(max, Is.EqualTo(9.5m));
        });
    }

    [Test]
    public void Reads_JsonObject_With_Integer_Bounds()
    {
        var value = new JsonObject { ["min"] = 2, ["max"] = 5 };

        var result = RangeConfigurationHelper.TryGetBounds(value, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.EqualTo(2));
            Assert.That(max, Is.EqualTo(5));
        });
    }

    [Test]
    public void Reads_JsonObject_With_Decimal_Bounds()
    {
        var value = new JsonObject { ["min"] = 0.25m, ["max"] = 7.75m };

        var result = RangeConfigurationHelper.TryGetBounds(value, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.EqualTo(0.25m));
            Assert.That(max, Is.EqualTo(7.75m));
        });
    }

    [Test]
    public void Reads_JsonObject_With_Only_One_Bound()
    {
        var value = new JsonObject { ["min"] = 3 };

        var result = RangeConfigurationHelper.TryGetBounds(value, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.EqualTo(3));
            Assert.That(max, Is.Null);
        });
    }

    [Test]
    public void Reads_Empty_JsonObject_As_Unbounded()
    {
        var result = RangeConfigurationHelper.TryGetBounds(new JsonObject(), out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.Null);
            Assert.That(max, Is.Null);
        });
    }

    [Test]
    public void Reads_JsonElement_Object()
    {
        JsonElement value = JsonDocument.Parse("{\"min\":1,\"max\":10}").RootElement.Clone();

        var result = RangeConfigurationHelper.TryGetBounds(value, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.EqualTo(1));
            Assert.That(max, Is.EqualTo(10));
        });
    }

    [Test]
    public void Reads_Dictionary_With_Mixed_Numeric_Types()
    {
        var value = new Dictionary<string, object> { ["min"] = 4, ["max"] = 8.5d };

        var result = RangeConfigurationHelper.TryGetBounds(value, out decimal? min, out decimal? max);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(min, Is.EqualTo(4));
            Assert.That(max, Is.EqualTo(8.5m));
        });
    }
}
